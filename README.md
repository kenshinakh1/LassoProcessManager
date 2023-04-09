# Lasso Process Manager

## What is this?
This is a little project I tossed together for fun after getting an AMD 7950x3D cpu. I had some issues with my bios bugging the threadmanagement when PBO is enabled.
Windows would put everything onto the cache CCD, even when no game is running. 
I also had weird issues where alt tabing out of a game will switch the game onto CCD1, and tabbing back to the game creates a blip or weird stutter as the thread management tries to swap the game between CCDs.
I saw online most people resorted to process lasso and saw that was a super cool program.
Except this is much simpler and easier for me to get going, and it uses much less CPU overhead (basically 0% for the 7950x3D).
Plus, this is free :).

With this tool, you can set default profiles for all applications. 
For specific processes like games, you can define the profile below and lasso it to CCD0 for the 3d vcache.
You could even specify delay profiles for games that don't like having their affinity mask changed instantly.
Profiles can be made to specify certain cores too, like if you only want a process to run on the last 2 cores, it's easy to do.
Most importantly, it's really light weight.

The program is pretty barebones for now and I really only set it up with barebones code to get something going. 
I assume AMD and Microsoft will continously improve the thread

## Requirements to run
Install .net 6.0 runtime to run program.

x64: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.15-windows-x64-installer

x86: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.15-windows-x86-installer

Run the program by extracting the release zip and running LassoProcessManager.exe with Admin. 
Admin is required in order to set process affinity.

## Building on your machine
If you're looking to compile on your own machine, grab Visual Studios 2022 and the .net 6.0 SDK

IDE: https://visualstudio.microsoft.com/vs/

SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## Configuring
To configure the Lasso Process Manager, make sure to exit the program first.
Open Config.json with notepad or any text editor. You should be familiar with json formatting before editing this.


Below are each config explained.

### `AutoApplyDefaultProfile`

Values:
1. `true` 
2. `false`

Set `true` to automatically apply the specified default profile in config `DefaultProfile` for processes that have no profiles defined.

### `DefaultProfile`
`string` value. Default profile name to use with `AutoApplyDefaultProfile` config. 

### `Profiles`
Array of `LassoProfile` in json format. 

`LassoProfile` has the following properties:

1. `Name` - `string` value display name for the profile.
2. `Description` - `string` value of description of profile.
3. `AffinityMaskHex` - `string` value in hex format for the processor affinity mask. This can be calculated using online tools: https://bitsum.com/tools/cpu-affinity-calculator/
4. `DelayMS` - `int` value of delay time in milliseconds for applying this profile on a process.

I provide some sample profiles below:
```
"Profiles": [
    {
      // Calculate affinity mask hex using https://bitsum.com/tools/cpu-affinity-calculator/
      "Name": "Cache",
      "Description": "Cache profile on CCD0 with 3dvcache. Processors 0-15.",
      "AffinityMaskHex": "0x000000000000FFFF",
      "DelayMS": 0
    },
    {
      "Name": "Frequency",
      "Description": "Frequency profile on CCD1. Processors 16-31.",
      "AffinityMaskHex": "0x00000000FFFF0000",
      "DelayMS": 0
    },
    {
      "Name": "AllCore",
      "Description": "A profile on all cores. Processors 16-31.",
      "AffinityMaskHex": "0x00000000FFFFFFFF",
      "DelayMS": 0
    },
    {
      "Name": "FrequencyCore_30_32",
      "Description": "Frequency profile on processors 30-31.",
      "AffinityMaskHex": "0x00000000C0000000",
      "DelayMS": 0
    },
    {
      "Name": "Cache_Delay1s",
      "Description": "Cache profile on CCD0 with 3dvcache with 1s delay. Processors 0-15.",
      "AffinityMaskHex": "0x000000000000FFFF",
      "DelayMS": 1000
    }
  ],
```

### `ProcessRules`
Array of `ProcessRule` in json format.

`ProcessRule` has the following properties:
1. `ProcessName` - `string` value of the process name. Usually extension is not needed here.
2. `Profile` - `string` value of the `LassoProfile`'s name to load when this process is found.

### `FolderRules`
Array of `FolderRule` in json format.

`FolderRule` has the following properties:
1. `FolderPath` - `string` value of the folder path. This is the base folder where all exe or processes below this folder path will be a match for this rule. Make sure to escape backward slash from Windows path.
ie. `C:\Games\Game1` should be `C:\\Games\\Game1`.
2. `Profile` - `string` value of the `LassoProfile`'s name to load.

