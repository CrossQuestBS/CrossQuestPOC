using System.Diagnostics;

namespace CrossQuestPOC
{
    public class UnityEditor
    {

        public static string ExecutablePath = "";
        
        
        public static void CreateUnityProject(string projectPath)
        {
            var arguments = "-batchmode ";
            arguments += $"-createProject \"{projectPath}\" ";
            arguments += "-quit -logfile - ";
    
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = ExecutablePath, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
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

        public static bool CompileUnityProject(string projectPath, string buildPath, string activeBuildProfile)
        {
            var arguments = "-batchmode ";
            arguments += $"-project-path \"{projectPath}\" ";
            arguments += "-quit -logfile - ";
            arguments += $"-activeBuildProfile \"{activeBuildProfile}\" ";
            arguments += $"-build \"{buildPath}\"";
    
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = ExecutablePath, Arguments = arguments, RedirectStandardOutput = true, CreateNoWindow = true}; 
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

            return File.Exists(buildPath);
        }

        public static void CopyAssembliesTo(string unityPath, string assembliesPath, Dictionary<string, string[]> assemblies)
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
 
        public static void CopyProjectTo(string from, string to)
        {
            var newAssets = Path.Join(to, "Assets");
            var newProjectSettings = Path.Join(to, "ProjectSettings");
            var newPackages = Path.Join(to, "Packages");
    
            Directory.Delete(newAssets, true);
            Directory.Delete(newProjectSettings, true);
            Directory.Delete(newPackages, true);
    
            var baseAssets = Path.Join(from, "Assets");
            var baseProjectSettings = Path.Join(from, "ProjectSettings");
            var basePackages = Path.Join(from, "Packages");


            FileUtility.CopyFolder(baseAssets, newAssets);
            FileUtility.CopyFolder(baseProjectSettings, newProjectSettings);
            FileUtility.CopyFolder(basePackages, newPackages);
        }
    }
}