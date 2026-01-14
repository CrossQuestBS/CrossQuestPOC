# CrossQuest (Proof of concept)

This is a proof of concept for an alternative Beat Saber Quest modding ecosystem.

## How it works
1. Gets list of assemblies from PC game (uses files in FilesToGet)
2. Creates a new Unity Project using Unity Editor CLI (batchmode)
3. Copy over files from UnityBaseProject to Unity Project
4. Copy assemblies from PC game to Unity Project
5. Install mods defined in ModFile provided
6. Compile using Unity Editor CLI (batchmode)
7. Extract IL2CPP and other configuration files compiled APK
8. Copy over the extracted files to Quest Game APK
9. Clear Game Cache to delete cached IL2CPP Metadata
10. Installs game to device

## Demo video on Beat Saber 1.42.0
https://github.com/user-attachments/assets/0468cbe5-4a7c-4181-9b87-d264d0d8f3b7



## Requirements to run
* Beat Saber Quest APK 1.42.0
* Beat Saber PC game (Available through cross-buy)
* [Unity Hub](https://unity.com/download)
* [Unity Editor 6000.0.40f1](https://unity.com/releases/editor/whats-new/6000.0.40f1#installs)
   * Android Build Support component
* [UnityBaseProject](https://github.com/CrossQuestBS/UnityBaseProject/tree/1-42-0)
* [reapk-cli](https://github.com/DanTheMan827/reapk-cli)
   * Build using Cargo
* [apktool](https://apktool.org/)


## CLI

### Running the POC

```
./CrossQuestPOC \
    --unity-editor "UNITY_EXECUTABLE_PATH_HERE" \
    --base-project "PATH_TO_UNITY_BASE_PROJECT" \
    --pc-path "PATH_TO_BEATSABER_MANAGED_DLL_FOLDER" \ 
    --mod-file "PATH_TO_MOD_DEFINITION_JSON" \
    --build-path "PATH_FOR_COMPILED_APK.apk" \
    --apk-path "PATH_FOR_BEATSABER_APK"
```

## Mod definition file

```json
[
    {
        "Id": "unique-id-folder-compatible-name",
        "Name": "MOD NAME",
        "Path": "PATH TO MOD DIRECTORY"
    }
]
```

## Mods

### OculusPatcher
**REQUIRED**

This mod fixes up DataModels.dll to reference correct OculusPlatformExtensions.

https://github.com/CrossQuestBS/OculusPatcher

### QuestPatcher
**REQUIRED**

This mod updates the game assemblies to have Quest specific code.

https://github.com/CrossQuestBS/QuestPatcher

### CustomSongEnabler

This mod patches the game assemblies to force enable CustomSongs, and to fetch the CustomLevel folder from OBB path.

https://github.com/CrossQuestBS/CustomSongEnabler

### ImageCoverExpander
First port of PC mod to test CrossQuest POC

https://github.com/CrossQuestBS/ImageCoverExpander

## Are you interested to help out with development, create mods or to test it out?
Join CrossQuest Discord server!

https://discord.gg/2vAFccFBsu

## Known issues
The current Proof of Concept is still WIP, and have made assumptions on the current game install.

### Game does not install
You can patch the base game once with [QuestPatcher application](https://github.com/Lauriethefish/QuestPatcher/releases) to get make the patch process work correctly and have the OBB files copied.

Alternatively you can take backup of OBB files, uninstall game, and install the game again from the patched APK.

### Path not found issues

The compiled CLI needs to be running from `CrossQuestPOC/bin/Debug/net10.0` as there are some hardcoded paths.


### Rust panic error?

The POC does not check if Unity had a success compile, so there might be error if the compile failed and it tries to the patch the APK.
