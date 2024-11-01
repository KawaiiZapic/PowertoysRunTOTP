# PowerToys Run TOTP Plugin
[![Mentioned in Awesome PowerToys Run Plugins](https://awesome.re/mentioned-badge-flat.svg)](https://github.com/hlaueriksson/awesome-powertoys-run-plugins)  
A plugin helps you to copy your two-factor verification code in PowerToys Run


## Screenshot
![screenshot](./assets/screenshot.png)

## Important: About the authenticator
**Lost 2FA authenticator means lost access to your account!**   
Never use this plugin as the only authenticator app, also use Google Authenticator or something else to keep a backup of your authenticators.  
All authenticator will be encrypted with Encryption API of Windows in current user scope, if you re-install Windows or switch a account to login in to Windows, all data can't be decrypted and you will lost all authenticator in plugin.  
Before reset your computer / re-install Windows, follow instruction below to export all authenticators.  

## Installation
#### Manual
1. Download plugin from Release
2. Extract it to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`
#### Via [ptr](https://github.com/8LWXpg/ptr)
```
ptr add TOTP KawaiiZapic/PowertoysRunTOTP
```
## Upgrade
#### Manual
1. Download plugin from Release
2. Delete `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\TOTP` (Will not lose any data)
3. Extract it to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`
#### Via [ptr](https://github.com/8LWXpg/ptr)
```
ptr update TOTP
```

## Usage
1. **Add an authenticator**   
Plugin can scan QR code on screen to add an authenticator, typing `[!` in search bar to show plugin action menu, this method only support standard OTPAuth URI (starts with `otpauth://`).  
Plugin support standard OTPAuth URI (starts with `otpauth://`) and Google Authenticator Export URI(starts with `otpauth-migration://`), you can paste it to the PT Run search bar and you will see an option to add it to plugin.  

2. **Delete & rename authenticator**   
Search for the authenticator that you want to rename & delete, on the right side of search result you can rename & delete it.  
Delete a authenticator must be confirmed by typing `DELETE` in the confirm dialog.  

3. **Export all authenticators**  
If you want to re-install os, all data will be not decryptable. For that purpose, you can decrypt & export all data to a JSON file by typing `[!` in search bar, and select "Export authenticators".  
Data may can't import across a big version gap, so do better to import the data from same version of plugin. All history version can be found in releases.  
Even there is a way to export your authenticators, **Never use this plugin as the only authenticator app**.

## About Encryption
Plugin uses `System.Security.Cryptography.ProtectedData` to encrypt every key. All keys will be decrypted in memory if needed.  
Encrypted data only can be decrypted in your current machine with the current account login, if you reinstall Windows or change the account you log in to, the key cannot be decrypted.  
The encryption also can be decrypted by other program running by your account, so the encryption only prevent you accidently share the config file of plugin.  
Before reset your computer / re-install Windows, follow instruction to export all authenticators.  

## Build Plugin
1. Clone repo
2. Restore NuGet packages & build
