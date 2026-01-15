using System.Text.Json;

namespace CrossQuestPOC
{
    public static class Modding
    {
        private static ModDefinition[] GetModDefintions(string path)
        {
            string jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ModDefinition[]>(jsonString);
        }

        public static void InstallMods(string unityProject, string modJson)
        {
            var modDefintions = GetModDefintions(modJson);
            var mods = Path.Join(unityProject, "Assets", "Plugins", "Mods");

            Directory.CreateDirectory(mods);
    
            foreach (var modDef in modDefintions)
            {
                Console.WriteLine($"Installing mod: {modDef}");
                if (Directory.Exists(modDef.Path) && !Directory.Exists(Path.Join(mods, modDef.Id)))
                {
                    FileUtility.CopyFolder(modDef.Path, Path.Join(mods, modDef.Id));
                }
            }
        }
        private static string[] GetAssembliesFiles(string folder, string filePath)
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

        
        public static Dictionary<string, string[]> GetAssembliesOculusPC(string folder)
        {
            var output = new Dictionary<string, String[]>();
    
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            string path = Path.Join(projectDirectory, "FilesToGet");
    
            foreach (var file in Directory.GetFiles(path))
            {
                output.Add(file.Split("/").Last().Split(".txt").First(), GetAssembliesFiles(folder, file));
            }

            return output;
        }
    }
}