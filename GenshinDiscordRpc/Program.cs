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
        private const string AppId = "757909870258159646";

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
                using var client = new DiscordRpcClient(AppId);
                client.Initialize();

                var playing = false;
                
                while (true)
                {
                    await Task.Delay(1000);

                    Debug.Print($"InLoop");

                    var hndl = FindWindow("UnityWndClass", "原神");
                    if (hndl == IntPtr.Zero)
                    {
                        Debug.Print($"Not found game process.");
                        continue;
                    }

                    try
                    {
                        var proc = Process.GetProcesses().FirstOrDefault(x => x.MainWindowHandle == hndl);
                        if (proc == null)
                        {
                            Debug.Print($"Not match game process.");
                            continue;
                        }

                        Debug.Print($"Check process with {hndl} | {proc?.ProcessName}");
                        if ("YuanShen".Equals(proc.ProcessName))
                        {
                            if (!playing)
                            {
                                playing = true;

                                client.SetPresence(new RichPresence
                                {
                                    Assets = new Assets
                                    {
                                        LargeImageKey = "genshin",
                                        LargeImageText = "原神"
                                    },
                                    Timestamps = Timestamps.Now,
                                    State = "Teyvat continent"
                                });

                                Debug.Print($"Set RichPresence to {proc.ProcessName}");
                            }
                            else
                            {
                                Debug.Print($"Keep RichPresence to {proc.ProcessName}");
                            }
                        }
                        else
                        {
                            playing = false;
                            client.ClearPresence();
                            Debug.Print($"Clear RichPresence");
                        }
                    }
                    catch (Exception e)
                    {
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
            var autoButton = new MenuItem("AtuoStart" + "    " + (AutoStart.Check() ? "√" : "✘"));
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

                autoButton.Text = "AtuoStart" + "    " + (AutoStart.Check() ? "√" : "✘");
            };

            
            Application.Run();
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
