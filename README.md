# Powertoys Run TOTP Plugin
A plugin help you to copy your two-factor verify code in Powertoys Run


## Screenshot
![screenshot](./assets/screenshot.png)

## Installtion
1. Download plugin from Release
2. Extract it to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`

## Usage
1. Add account  
Plugin support standard OTPAuth URI (starts with `otpauth://`) and Google Authenticator Export URI(starts with `otpauth-migration://`), you can paste it to search bar and you will see option to add it to list.  
You can use QRCode scanner to resolve QRCode to link, Accounts with same key in list will not be added.  
You can also add manually by edit config file in `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\Zapic.Plugin.TOTP\TOTPList.json`:
    ```json
    [
      {
        "Name": "Github: Hello",
        "Key": "12313"
      },
      {
        "Name": "Twitter: @Hello",
        "Key": "12313213"
     }
    ]
    ```
    Change to config file will be applied immediately.

2. Delete account  
There is no way to delete account by GUI.   
You can edit file in `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\Zapic.Plugin.TOTP\TOTPList.json` to delete account you don't want.  
Change to config file will be applied immediately.

## Build Plugin
1. Clone repo
2. Use "Publish" to build plugin and copy dependcies
3. Use `ILRepack` to bundle all dependcies to single file(Powertoys doesn't support load dll by PTRun plugin).
   ```
    ILRepack.exe /lib:"C:\Program Files\Microsoft Visual Studio\2022\Community\dotnet\runtime\shared\Microsoft.NETCore.App\6.0.16" /out:PowerToysRunTOTP.dll PowertoysRunTOTP\bin\Release\net6.0-windows\publish\win-x64\PowerToysRunTOTP.dll PowertoysRunTOTP\bin\Release\net6.0-windows\publish\win-x64\Otp.NET.dll PowertoysRunTOTP\bin\Release\net6.0-windows\publish\win-x64\Google.Protobuf.dll
   ```
4. Use ILRepack generate `PowerToysRunTOTP.dll` to replace orignal out.
