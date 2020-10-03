using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscordRPC;

namespace GenshinDiscordRpc
{
    static class Program
    {
        private const string AppId_Zh = "757909870258159646";
        private const string AppId_En = "761911105081442335";

        [STAThread]
        static void Main()
        {
            using var self = new Mutex(true, "Genshin DiscordRPC", out var allow);
            if (!allow)
            {
                MessageBox.Show("Genshin DiscordRPC is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            if (Properties.Settings.Default.IsFirstTime)
            {
                AutoStart.Set();
                Properties.Settings.Default.IsFirstTime = false;
                Properties.Settings.Default.Save();
            }
            
            Task.Run(async () =>
            {
                using var clientZh = new DiscordRpcClient(AppId_Zh);
                using var clientEn = new DiscordRpcClient(AppId_En);
                clientZh.Initialize();
                clientEn.Initialize();

                var playing = false;
                
                while (true)
                {
                    await Task.Delay(1000);

                    Debug.Print($"InLoop");

                    var hndlZh = FindWindow("UnityWndClass", "原神");
                    var hndlEn = FindWindow("UnityWndClass", "Genshin Impact");
                    if (hndlZh == IntPtr.Zero && 
                        hndlEn == IntPtr.Zero)
                    {
                        Debug.Print($"Not found game process.");
                        playing = false;
                        if (clientEn.CurrentPresence != null)
                        {
                            clientEn.ClearPresence();
                        }
                        if (clientZh.CurrentPresence != null)
                        {
                            clientZh.ClearPresence();
                        }
                        continue;
                    }

                    try
                    {
                        var procEn = Process.GetProcesses().FirstOrDefault(x => x.MainWindowHandle == hndlEn);
                        var procZh = Process.GetProcesses().FirstOrDefault(x => x.MainWindowHandle == hndlZh);
                        if (procEn == null && 
                            procZh == null)
                        {
                            throw new Exception($"Not match game process.");
                        }

                        Debug.Print($"Check process with {hndlZh} | {procZh?.ProcessName} || {hndlEn} | {procEn?.ProcessName}");
                        
                        if ("YuanShen".Equals(procZh?.ProcessName))
                        {
                            if (!playing)
                            {
                                playing = true;

                                clientZh.SetPresence(new RichPresence
                                {
                                    Assets = new Assets
                                    {
                                        LargeImageKey = "genshin",
                                        LargeImageText = "原神",
                                    },
                                    Timestamps = Timestamps.Now,
                                    State = "提瓦特大陆"
                                });

                                Debug.Print($"Set RichPresence to {procZh?.ProcessName}");
                            }
                            else
                            {
                                Debug.Print($"Keep RichPresence to {procZh?.ProcessName}");
                            }
                        }
                        else if ("GenshinImpact".Equals(procEn?.ProcessName))
                        {
                            if (!playing)
                            {
                                playing = true;

                                clientEn.SetPresence(new RichPresence
                                {
                                    Assets = new Assets
                                    {
                                        LargeImageKey = "genshin",
                                        LargeImageText = "Genshin Impact",
                                    },
                                    Timestamps = Timestamps.Now,
                                    State = "Teyvat continent"
                                });

                                Debug.Print($"Set RichPresence to  {procEn?.ProcessName}");
                            }
                            else
                            {
                                Debug.Print($"Keep RichPresence to {procEn?.ProcessName}");
                            }
                        }
                        else
                        {
                            playing = false;
                            if (clientEn.CurrentPresence != null)
                            {
                                clientEn.ClearPresence();
                            }
                            if (clientZh.CurrentPresence != null)
                            {
                                clientZh.ClearPresence();
                            }
                            Debug.Print($"Clear RichPresence");
                        }
                    }
                    catch (Exception e)
                    {
                        playing = false;
                        if (clientEn.CurrentPresence != null)
                        {
                            clientEn.ClearPresence();
                        }
                        if (clientZh.CurrentPresence != null)
                        {
                            clientZh.ClearPresence();
                        }
                        Debug.Print($"{e.Message}{Environment.NewLine}{e.StackTrace}");
                    }

                    GC.Collect();
                    GC.WaitForFullGCComplete();
                }
            });

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var notifyMenu = new ContextMenu();
            var exitButton = new MenuItem("Exit");
            var autoButton = new MenuItem("AutoStart" + "    " + (AutoStart.Check() ? "√" : "✘"));
            notifyMenu.MenuItems.Add(0, autoButton);
            notifyMenu.MenuItems.Add(1, exitButton);
            
            var notifyIcon = new NotifyIcon()
            {
                BalloonTipIcon = ToolTipIcon.Info,
                ContextMenu = notifyMenu,
                Text = "Genshin DiscordRPC",
                Icon = Properties.Resources.genshin,
                Visible = true,
            };

            exitButton.Click += (sender, args) =>
            {
                notifyIcon.Visible = false;
                Thread.Sleep(100);
                Environment.Exit(0);
            };
            autoButton.Click += (sender, args) =>
            {
                var x = AutoStart.Check();
                if (x)
                {
                    AutoStart.Remove();
                }
                else
                {
                    AutoStart.Set();
                }

                autoButton.Text = "AutoStart" + "    " + (AutoStart.Check() ? "√" : "✘");
            };

            
            Application.Run();
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
