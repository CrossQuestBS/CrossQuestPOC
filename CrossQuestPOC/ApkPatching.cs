using System.Diagnostics;

namespace CrossQuestPOC
{
    public static class ApkPatching
    {
        public static void PatchGame(string extractedBuildPath, string apkPath)
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
                    //TODO: Make this not hardcoded!
                    FileUtility.CopyFile(Path.Join(projectDirectory, "ResourceFiles", "AndroidManifest.xml"), Path.Join(unpackedApkPath, "AndroidManifest.xml"));
                    
                    // Copy Managed Resources files
                    foreach (var file in Directory.GetFiles(Path.Join(extractedBuildPath, $"{assetDataPath}/Managed/Resources")))
                    {
                        FileUtility.CopyFile(file, Path.Join(unpackedApkPath, $"{assetDataPath}/Managed/Resources", Path.GetFileName(file)));
                    }
                    
                    foreach (var file in Directory.GetFiles(Path.Join(extractedBuildPath, $"{libPath}")))
                    {
                        FileUtility.CopyFile(file, Path.Join(unpackedApkPath, $"{libPath}", Path.GetFileName(file)));
                    }

                    foreach (var filePath in filesToCopy)
                    {
                        var extractedFilePath = Path.Join(extractedBuildPath, filePath);
                        var unpackedFilePath = Path.Join(unpackedApkPath, filePath);
                        FileUtility.CopyFile(extractedFilePath, unpackedFilePath);
                    }
                    
                    Console.WriteLine("Finished copying files, now pressing enter");
                    proc.StandardInput.WriteLine("");
                }
            }
        }
    }
}