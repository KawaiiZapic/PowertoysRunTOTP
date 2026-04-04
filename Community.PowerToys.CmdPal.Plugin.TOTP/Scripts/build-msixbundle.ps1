$ErrorActionPreference = 'Stop'

$projectRoot = (Split-Path $MyInvocation.MyCommand.Path | Get-Item).Parent.FullName

Push-Location $projectRoot

$BuildOptions = "-p:GenerateAppxPackageOnBuild=true"
if ($args -contains "-sideload") {
    $BuildOptions += " -p:PublishUnsignedPackage=true"
    Write-Host "Building for sideloading (unsigned package)"
} else {
    Write-Host "Building for store submission (signed package)"
}
if ($args -contains "-debug") {
    $BuildOptions += " --configuration Debug"
    Write-Host "Debug profile selected"
} else {
    $BuildOptions += " --configuration Release"
    Write-Host "Release profile selected"
}

function Execute-Command {
    param(
        [string]$Command
    )
    
    Write-Host "$> $Command"
    Invoke-Expression -Command "& $Command"
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        throw "Command failed with exit code $exitCode"
    }
    
    return $output
}

function Determine-Makeappx {
    try {
        $makeappxPath = (Get-Command "makeappx.exe" -ErrorAction Stop).Source
        Write-Host "Found makeappx.exe at: $makeappxPath"
        return $makeappxPath
    } catch { }
    try {
        $arch = switch ($env:PROCESSOR_ARCHITECTURE) { 
            "AMD64" { "x64" } 
            "x86" { "x86" } 
            "ARM64" { "arm64" } 
            default { "x64" } 
        }; 
        $makeappxPath = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin\*\$arch\makeappx.exe" -ErrorAction Stop | Sort-Object Name -Descending | Select-Object -First 1
        Write-Host "Found makeappx at: $makeappxPath"
        return $makeappxPath
    } catch { }
    throw "makeappx.exe not found. Please ensure the Windows 10 SDK is installed."
}

$makeappxPath = Determine-Makeappx

Remove-Item -Path "AppPackages\" -Recurse -Force -ErrorAction SilentlyContinue

Execute-Command "dotnet build -p:Platform=x64 $BuildOptions"
Execute-Command "dotnet build -p:Platform=arm64 $BuildOptions"

$x64MsixPath = Get-ChildItem -Path "AppPackages\x64" -Filter "*.msix" -Recurse | Select-Object -First 1
$arm64MsixPath = Get-ChildItem -Path "AppPackages\arm64" -Filter "*.msix" -Recurse | Select-Object -First 1
 
if ($x64MsixPath -and $arm64MsixPath) {
    $x64FileName = $x64MsixPath.Name
    $arm64FileName = $arm64MsixPath.Name
    $bundleMappingContent = @'
[Files]
"{0}" "{1}"
"{2}" "{3}"
'@ -f $x64MsixPath.FullName, $x64FileName, $arm64MsixPath.FullName, $arm64FileName

    $bundleMappingContent | Out-File -FilePath ".\AppPackages\bundle_mapping.txt" -Encoding UTF8
    $OutputFileName = ".\AppPackages\{0}" -f $x64FileName.Replace("_x64", "").Replace('.msix', '_Bundle.msixbundle')
    Execute-Command "'$makeappxPath' bundle /v /f .\AppPackages\bundle_mapping.txt /p $OutputFileName"
} else {
    Write-Host "msix packages not found"
    Exit 1
}