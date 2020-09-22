using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace GenshinDiscordRpc
{
    static class AutoStart
    {
        public static void Set()
        {
            try
            {
                // Open Base Key.
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (baseKey == null)
                {
                    // wtf?
                    Console.WriteLine(@"Cannot find HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    return;
                }

                baseKey.SetValue("Genshin-DiscordRpc", Assembly.GetEntryAssembly().Location);
                Console.WriteLine("AutoStartup has been set.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to set autostartup: {e.Message}");
            }
        }

        public static void Remove()
        {
            try
            {
                // Open Base Key.
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (baseKey == null)
                {
                    // wtf?
                    Console.WriteLine(@"Cannot find HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    return;
                }

                baseKey.DeleteValue("Genshin-DiscordRpc", false);
                Console.WriteLine("AutoStartup has been deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to set autostartup: {e.Message}");
            }
        }

        public static bool Check()
        {
            try
            {
                // Open Base Key.
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (baseKey == null)
                {
                    // wtf?
                    Console.WriteLine(@"Cannot find HKCU\Software\Microsoft\Windows\CurrentVersion\Run");
                    return false;
                }

                var ace = baseKey.GetValue("Genshin-DiscordRpc");
                var exe = Assembly.GetEntryAssembly().Location;
                return exe.Equals(ace);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to set autostartup: {e.Message}");
            }

            return false;
        }
    }
}
