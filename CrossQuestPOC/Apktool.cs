using System.Diagnostics;

namespace CrossQuestPOC
{
    public static class Apktool
    {
        public static void ExtractAPK(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "apktool", Arguments = $"d \"{path}\" -f"}; 
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
        }
    }
}