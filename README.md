# lstwoMODSInstaller
Program to install and update [lstwoMODS](https://github.com/lstwo/lstwomods) and its dependencies and install and manage Custom Items.

WINDOWS X64 ONLY

# Set Up

> [!IMPORTANT]
> This program requires the .NET 8.0 Desktop Runtime. You can download it [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.14-windows-x64-installer).

1. **Make sure you are logged into GitHub. You'll need this later.**

   If you don't have an account yet you can create one [here](https://github.com/signup). If you do you can log in [here](https://github.com/login).
   
2. **Download the lstwoMODS Installer**
  
> [!NOTE]
> Windows Defender will most likely falsely flag this file as malicious.
> To get around this
> open Windows Security >
> Go to Virus & threat protection >
> Protection history >
> Click on the newest one >
> Click yes if it asks you about making changes to your device >
> Click Actions >
> Click Allow >
> Download the file again

   You can download the newest version from [the releases page](https://github.com/lstwoSTUDIOS/lstwoMODSInstaller/releases). You only need to download one file:
   If you plan on working with Custom Items a lot download the **.msi** file. If you just want to install lstwoMODS download the **.zip** file.

3. **Set Up and run the program**

   The setup will be different depending on the file you downloaded:
   - **.MSI:** Run the **.msi** file and follow the instructions on screen. Once installed open the lstwoMODS Installer from the start menu.
   - **.ZIP:** Right click the file and extract it. Once extracted open the lstwoMODS Installer.exe (Application) file

4. **Authorize with GitHub**

   When started for the first time it should open a GitHub page. Click continue and type in the code displayed in the dialog box. Click OK and wait until it says "PAT initialized".

5. **Select Game**

   Wait until the dropdown at the top shows what games you can install then select the game to install the mods for.

7. **Install Mods**

   Install lstwoMODS Core and the game mods by clicking their install buttons.

> [!IMPORTANT]
> Some mods will only work if you first install lstwoMODS Core and then the game mods because they need to overwrite some of lstwoMODS Core's files

7. (Optional) **Install Additional Mods**

   If the game has them you can also install other mods here

# Troubleshooting

## Installing a mod makes the game not open anymore

### Cause

This might be because of an incompatibility or issue in the mod.

### Fix

Find the mod on github and check the readme or issues page for any notes.

## Installer doesn't open

### Cause

This is most likely caused by the fact that .NET is not installed. This could also happen if you aren't using 64-Bit Windows (10+), because MacOS, Linux and 32-Bit Windows is not supported.

### Fix

If you are running 64-Bit Windows, all you need to do is download and install the [.NET 6.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.36-windows-x64-installer)

## 403: Rate limit exceeded

### Cause

This error means that your installer has been sending too many requests to GitHub. This is because GitHub has a limit on how often requests can be made. It can happen when opening / closing the installer a lot.

### Fix

This error can simply be fixed by waiting until it doesn't happen anymore.



If you need help with any other problems related to lstwoMODS, join my discord here: https://discord.gg/cKWcxccQXU
