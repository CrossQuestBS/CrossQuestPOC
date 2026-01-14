// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using CrossQuestPOC;

static void CopyFolder( string sourceFolder, string destFolder )
{
    if (!Directory.Exists( destFolder ))
        Directory.CreateDirectory( destFolder );
    string[] files = Directory.GetFiles( sourceFolder );
    foreach (string file in files)
    {
        string name = Path.GetFileName( file );
        string dest = Path.Combine( destFolder, name );
        File.Copy( file, dest );
    }
    string[] folders = Directory.GetDirectories( sourceFolder );
    foreach (string folder in folders)
    {
        string name = Path.GetFileName( folder );
        string dest = Path.Combine( destFolder, name );
        CopyFolder( folder, dest );
    }
}

static string[] GetFiles(string folder, string filePath)
{
    var lines = File.ReadAllLines(filePath);
    var files = Directory.GetFiles(folder).Where(t => lines.Contains(t.Trim().Split("/").Last()))
        .ToArray();
    
    Console.WriteLine(files + "\n");
    foreach (var file in files)  
    {
        Console.WriteLine(file.Split("/").Last());
    }
    Console.WriteLine(files.Length);

    return files;
}


static Dictionary<string, string[]> GetAssembliesOculusPC(string folder)
{
    var output = new Dictionary<string, String[]>();
    
    string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
    string path = Path.Join(projectDirectory, "FilesToGet");
    
    foreach (var file in Directory.GetFiles(path))
    {
        output.Add(file.Split("/").Last().Split(".txt").First(), GetFiles(folder, file));
    }

    return output;
}

static string CreateLink(string[] files)
{
    string output = "<linker>\n";
    foreach (var file in files)
    {
        output += String.Format("   <assembly fullname=\"" + file.Split("/").Last().Split(".dll").First() + "\" preserve=\"all\"/>\n");
    }

    output += "</linker>";
    return output;
}

static void CopyToUnityProject(string unityPath, string assembliesPath, Dictionary<string, string[]> assemblies)
{
    var plugins = Path.Join(unityPath, "Assets", "Plugins");

    var pluginFolders = Directory.GetDirectories(plugins);
    
    foreach (var keyPair in assemblies)
    {
        var pluginPath = pluginFolders.First(t => t.Split("/").Last() == keyPair.Key);

        if (pluginPath == "")
        {
            Console.WriteLine("Failed to find path for key: " + keyPair.Key);
            continue;
        }

        foreach (var assemblyFile in keyPair.Value)
        {
            var unityAssemblyPath = Path.Join(pluginPath, Path.GetFileName(assemblyFile));
            if (File.Exists(unityAssemblyPath))
                continue;
            
            File.Copy(assemblyFile, unityAssemblyPath);
        }
    }

    var linkPath = Path.Join(plugins, "link.xml");
    
    if (!File.Exists(linkPath))
    {
        var linkedFile = CreateLink(Directory.GetFiles(assembliesPath).Where(x => !x.Split("/").Last().StartsWith("System") && !x.Split("/").Last().StartsWith("Mono")).ToArray());
        File.WriteAllText(linkPath, linkedFile);
    }
}


static void TryCompile(string unityEditorPath, string projectPath, string outputPath)
{
    var arguments = "-batchmode ";
    arguments += $"-project-path \"{projectPath}\" ";
    arguments += "-quit -logfile - ";
    arguments += "-activeBuildProfile \"Assets/Settings/Build Profiles/Compile.asset\" ";
    arguments += $"-build \"{outputPath}\"";
    
    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = unityEditorPath, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
    Process proc = new Process() { StartInfo = startInfo, };
    proc.Start();

    while (!proc.StandardOutput.EndOfStream)
    {
        string line = proc.StandardOutput.ReadLine();
        Console.WriteLine(line);
    }
}

static void ExtractAPK(string buildPath)
{
    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "apktool", Arguments = $"d \"{buildPath}\" -f"}; 
    Process proc = new Process() { StartInfo = startInfo, };
    proc.Start();
    proc.WaitForExit();
}

static void CopyFile(string from, string to)
{
    File.Copy(from, to, true);
    Debug.WriteLine($"Copying file from {from} to {to}");
    
}

static void PatchGame(string extractedBuildPath, string apkPath)
{
    string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
    
    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "reapk-cli", Arguments = $"{apkPath}", RedirectStandardOutput = true, CreateNoWindow = true, RedirectStandardInput = true}; 
    Process proc = new Process() { StartInfo = startInfo, };
    
    proc.Start();
    var unpackedApkPath = "";

    var findPath = "Unpacked APK path: ";

    var libPath = "lib/arm64-v8a";
    var assetDataPath = "assets/bin/Data";
    
    string[] filesToCopy = { 
        $"{assetDataPath}/boot.config", 
        $"{assetDataPath}/ScriptingAssemblies.json",
        $"{assetDataPath}/RuntimeInitializeOnLoads.json",
        $"{assetDataPath}/RuntimeInitializeOnLoads.json",
        $"{assetDataPath}/Managed/Metadata/global-metadata.dat"
    }; 

    while (!proc.StandardOutput.EndOfStream)
    {
        string line = proc.StandardOutput.ReadLine();
        Console.WriteLine(line);

        if (line is not null && line.Contains(findPath))
        {
            unpackedApkPath = line.Split(findPath).Last().Trim();
        }


        if (line is not null && line.Contains("Press Enter to continue..."))
        {
            // Copy Manifest
            CopyFile(Path.Join(projectDirectory, "ResourceFiles", "AndroidManifest.xml"), Path.Join(unpackedApkPath, "AndroidManifest.xml"));
            
            // Copy Managed Resources files
            foreach (var file in Directory.GetFiles(Path.Join(extractedBuildPath, $"{assetDataPath}/Managed/Resources")))
            {
                CopyFile(file, Path.Join(unpackedApkPath, $"{assetDataPath}/Managed/Resources", Path.GetFileName(file)));
            }
            
            foreach (var file in Directory.GetFiles(Path.Join(extractedBuildPath, $"{libPath}")))
            {
                CopyFile(file, Path.Join(unpackedApkPath, $"{libPath}", Path.GetFileName(file)));
            }

            foreach (var filePath in filesToCopy)
            {
                var extractedFilePath = Path.Join(extractedBuildPath, filePath);
                var unpackedFilePath = Path.Join(unpackedApkPath, filePath);
                CopyFile(extractedFilePath, unpackedFilePath);
            }
            
            Console.WriteLine("Finished copying files, now pressing enter");
            proc.StandardInput.WriteLine("");
        }
    }
}

static void InstallGame(string apkPath)
{
    var clearCache = "shell pm clear com.beatgames.beatsaber";
    
    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "adb", Arguments = clearCache}; 
    Process proc = new Process() { StartInfo = startInfo, };
    
    proc.Start();
    proc.WaitForExit();
    
    var argument2 = $"install -r \"{apkPath}\"";
    startInfo = new ProcessStartInfo() { FileName = "adb", Arguments = argument2}; 
    proc = new Process() { StartInfo = startInfo, };
    proc.Start();
    proc.WaitForExit();
}

static ModDefinition[] GetModDefintions(string path)
{
    string jsonString = File.ReadAllText(path);
    return JsonSerializer.Deserialize<ModDefinition[]>(jsonString);
}

static void InstallMods(string unityProject, string modJson)
{
    var modDefintions = GetModDefintions(modJson);
    var mods = Path.Join(unityProject, "Assets", "Plugins", "Mods");

    Directory.CreateDirectory(mods);
    
    foreach (var modDef in modDefintions)
    {
        Console.WriteLine($"Installing mod: {modDef}");
        if (Directory.Exists(modDef.Path) && !Directory.Exists(Path.Join(mods, modDef.Id)))
        {
            CopyFolder(modDef.Path, Path.Join(mods, modDef.Id));
        }
    }
}

static void CreateAndCopyBaseUnityProject(string unityEditorPath, string unityProjectPath, string newPath)
{
    var arguments = "-batchmode ";
    arguments += $"-createProject \"{newPath}\" ";
    arguments += "-quit -logfile - ";
    
    ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = unityEditorPath, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
    Process proc = new Process() { StartInfo = startInfo, };
    proc.Start();

    while (!proc.StandardOutput.EndOfStream)
    {
        string line = proc.StandardOutput.ReadLine();
        Console.WriteLine(line);
    }

    var newAssets = Path.Join(newPath, "Assets");
    var newProjectSettings = Path.Join(newPath, "ProjectSettings");
    var newPackages = Path.Join(newPath, "Packages");
    
    Directory.Delete(newAssets, true);
    Directory.Delete(newProjectSettings, true);
    Directory.Delete(newPackages, true);
    
    var baseAssets = Path.Join(unityProjectPath, "Assets");
    var baseProjectSettings = Path.Join(unityProjectPath, "ProjectSettings");
    var basePackages = Path.Join(unityProjectPath, "Packages");


    CopyFolder(baseAssets, newAssets);
    CopyFolder(baseProjectSettings, newProjectSettings);
    CopyFolder(basePackages, newPackages);
}


static void ModGame(string unityEditorExecutable, string unityProjectPath, string gamePath, string buildPath, string apkPath, string modFile)
{
    Console.WriteLine($"Getting game assemblies from path: {gamePath}");
    Thread.Sleep(2000);
    var assembliesPC = GetAssembliesOculusPC(gamePath);
    
    var parentPath = Directory.GetParent(unityProjectPath).FullName;
    var newUnityPath = Path.Join(parentPath, Guid.NewGuid().ToString());
    
    Console.WriteLine($"Create a new Unity Project at path {unityProjectPath}");
    Thread.Sleep(2000);
    CreateAndCopyBaseUnityProject(unityEditorExecutable, unityProjectPath, newUnityPath);
    
    Console.WriteLine($"Copying game assemblies from path: {gamePath} to {unityProjectPath}");
    Thread.Sleep(2000);
    CopyToUnityProject(newUnityPath, gamePath, assembliesPC);
    
    Console.WriteLine($"Installing mods to {newUnityPath}");
    Thread.Sleep(2000);
    InstallMods(newUnityPath, modFile);
    
    Console.WriteLine($"Compiling to build path: {buildPath}");
    Thread.Sleep(2000);
    TryCompile(unityEditorExecutable, newUnityPath, buildPath);
    
    Console.WriteLine($"Extracting APK from: {buildPath}");
    Thread.Sleep(2000);
    ExtractAPK(buildPath);
    
    Console.WriteLine($"Patching APK: {apkPath}");
    Thread.Sleep(2000);
    PatchGame(buildPath.Split(".apk").First(), apkPath);
    
    Console.WriteLine($"Installing game!");
    Thread.Sleep(2000);
    InstallGame(apkPath);
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
    var projectPath = parseResult.GetRequiredValue(baseProjectPath);
    var apkPath = parseResult.GetRequiredValue(beatsaberApkPath);
    var monoPath = parseResult.GetRequiredValue(beatsaberMonoPath);
    var modFile = parseResult.GetRequiredValue(modList);
    var build = parseResult.GetRequiredValue(buildPath);
    ModGame(unityEditorPath, projectPath, monoPath, build, apkPath, modFile);
    return 0;
});

ParseResult parseResult = rootCommand.Parse(args);
parseResult.Invoke();


