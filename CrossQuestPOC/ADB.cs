using System.Diagnostics;

namespace CrossQuestPOC
{
    public static class ADB
    {
        public static void InstallAPK(string apkPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "adb", Arguments = $"install -r \"{apkPath}\""}; 
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit();
        }

        public static void ClearCache(string packageId)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "adb", Arguments = $"shell pm clear {packageId}"}; 
            Process proc = new Process() { StartInfo = startInfo, };
    
            proc.Start();
            proc.WaitForExit();
        }
    }
}