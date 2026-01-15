// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using CrossQuestPOC;


static void ModGame(string unityProjectPath, string gamePath, string buildPath, string apkPath, string modFile)
{
    Console.WriteLine($"Getting game assemblies from path: {gamePath}");
    Thread.Sleep(2000);
    var assembliesPC = Modding.GetAssembliesOculusPC(gamePath);
    
    var parentPath = Directory.GetParent(unityProjectPath).FullName;
    var newUnityPath = Path.Join(parentPath, Guid.NewGuid().ToString());
    
    Console.WriteLine($"Create a new Unity Project at path {unityProjectPath}");
    Thread.Sleep(2000);
    UnityEditor.CreateUnityProject(newUnityPath);
    Console.WriteLine($"Copying base unity project to new Unity Project: {unityProjectPath}");
    UnityEditor.CopyProjectTo(unityProjectPath, newUnityPath);
    
    
    Console.WriteLine($"Copying game assemblies from path: {gamePath} to {unityProjectPath}");
    Thread.Sleep(2000);
    UnityEditor.CopyAssembliesTo(newUnityPath, gamePath, assembliesPC);
    
    Console.WriteLine($"Installing mods to {newUnityPath}");
    Thread.Sleep(2000);
    Modding.InstallMods(newUnityPath, modFile);
    
    Console.WriteLine($"Compiling to build path: {buildPath}");
    Thread.Sleep(2000);

    if (UnityEditor.CompileUnityProject(newUnityPath, buildPath, "Assets/Settings/Build Profiles/Compile.asset"))
    {
        Console.WriteLine($"Extracting APK from: {buildPath}");
        Thread.Sleep(2000);
        Apktool.ExtractAPK(buildPath);
    
        Console.WriteLine($"Patching APK: {apkPath}");
        Thread.Sleep(2000);
        ApkPatching.PatchGame(buildPath.Split(".apk").First(), apkPath);
    
        Console.WriteLine($"Clearing game cache!");
        Thread.Sleep(2000);
        ADB.ClearCache("com.beatgames.beatsaber");
        
        Console.WriteLine($"Installing game!");
        ADB.InstallAPK(apkPath);
        return;
    }
    
    Console.WriteLine($"Failed to compile file: {buildPath}");
}

Option<string> unityEditorExecutable = new("--unity-editor")
{
    Description = "Path to Unity Editor executable",
};

Option<string> baseProjectPath = new("--base-project")
{
    Description = "Path to Unity Base Project",
};

Option<string> beatsaberApkPath = new("--apk-path")
{
    Description = "Path to Beat Saber APK",
};

Option<string> beatsaberMonoPath = new("--pc-path")
{
    Description = "Path to Beat Saber PC game",
};

Option<string> modList = new("--mod-file")
{
    Description = "Path to json file containing mod information",
};

Option<string> buildPath = new("--build-path")
{
    Description = "Path to buildPath",
};


RootCommand rootCommand = new("Patches Beat Saber on Quest using CrossQuest");
rootCommand.Options.Add(unityEditorExecutable);
rootCommand.Options.Add(baseProjectPath);
rootCommand.Options.Add(beatsaberApkPath);
rootCommand.Options.Add(beatsaberMonoPath);
rootCommand.Options.Add(modList);
rootCommand.Options.Add(buildPath);


rootCommand.SetAction(parseResult =>
{
    var unityEditorPath = parseResult.GetRequiredValue(unityEditorExecutable);
    UnityEditor.ExecutablePath = unityEditorPath;
    var projectPath = parseResult.GetRequiredValue(baseProjectPath);
    var apkPath = parseResult.GetRequiredValue(beatsaberApkPath);
    var monoPath = parseResult.GetRequiredValue(beatsaberMonoPath);
    var modFile = parseResult.GetRequiredValue(modList);
    var build = parseResult.GetRequiredValue(buildPath);
    ModGame(projectPath, monoPath, build, apkPath, modFile);
    return 0;
});

ParseResult parseResult = rootCommand.Parse(args);
parseResult.Invoke();


