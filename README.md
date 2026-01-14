# CrossQuest (Proof of concept)

This is a proof of concept for an alternative Beat Saber Quest modding ecosystem.

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

### Are you interested to help out with development, create mods or to test it out?
Join CrossQuest Discord server!

https://discord.gg/2vAFccFBsu

