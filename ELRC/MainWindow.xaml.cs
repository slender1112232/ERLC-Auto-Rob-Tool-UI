using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using ELRCRobTool.Robberies;
using System.Windows.Threading;
using System.Media;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ELRCRobTool
{
    public partial class MainWindow : Window
    {
        private class CooldownInfo
        {
            public DispatcherTimer Timer { get; set; } = null!;
            public int RemainingSeconds { get; set; }
            public TextBlock Display { get; set; } = null!;
            public string InitialText { get; set; } = null!; // Lưu văn bản ban đầu
        }

        private readonly Dictionary<string, CooldownInfo> _cooldowns = new Dictionary<string, CooldownInfo>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CheckRoblox();
            SetupCooldowns();

            // Thêm sự kiện KeyDown để xử lý hotkey
            KeyDown += MainWindow_KeyDown;

            // Đăng ký event từ Logger để cập nhật LogTextBox
            Logger.OnLogMessage += message =>
            {
                Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText(message + "\n");
                    LogTextBox.ScrollToEnd();
                });
            };
        }

        public string SystemScaleMultiplier => Screen.SystemScaleMultiplier.ToString("F2");

        private void CheckRoblox()
        {
            if (!Roblox.IsRobloxRunning())
            {
                AppendLog("Waiting for Roblox to open...");
                Task.Run(async () =>
                {
                    while (!Roblox.IsRobloxRunning())
                    {
                        await Task.Delay(500);
                    }
                    Dispatcher.Invoke(() => AppendLog("Roblox is running!"));
                });
            }
        }

        private void SetupCooldowns()
        {
            _cooldowns["AutoATM"] = new CooldownInfo { Display = AutoATMCooldown };
            _cooldowns["RobBank"] = new CooldownInfo { Display = RobBankCooldown };
            _cooldowns["GlassCutting"] = new CooldownInfo { Display = GlassCuttingCooldown };
            _cooldowns["LockPick"] = new CooldownInfo { Display = LockPickCooldown };


            foreach (var cooldown in _cooldowns.Values)
            {
                cooldown.Timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                cooldown.Timer.Tick += (s, e) =>
                {
                    if (cooldown.RemainingSeconds > 0)
                    {
                        cooldown.RemainingSeconds--;
                        Dispatcher.Invoke(() =>
                        {
                            cooldown.Display.Text = $"Wait {cooldown.RemainingSeconds}s";
                            cooldown.Display.Background = new SolidColorBrush(Colors.Red);
                            cooldown.Display.Foreground = new SolidColorBrush(Colors.White);
                        });
                    }
                    else
                    {
                        cooldown.Timer.Stop();
                        Dispatcher.Invoke(() =>
                        {
                            cooldown.Display.Text = cooldown.InitialText; // Khôi phục văn bản ban đầu
                            cooldown.Display.Background = new SolidColorBrush(Colors.LightGreen);
                            cooldown.Display.Foreground = new SolidColorBrush(Colors.Black);
                            string actionName = _cooldowns.FirstOrDefault(x => x.Value == cooldown).Key;
                            PlaySound(actionName);
                        });
                    }
                };
            }

            Dispatcher.Invoke(() =>
            {
                foreach (var cooldown in _cooldowns)
                {
                    // Lưu văn bản ban đầu và hiển thị nó
                    cooldown.Value.InitialText = $"{cooldown.Key}: Ready";
                    cooldown.Value.Display.Text = cooldown.Value.InitialText;
                    cooldown.Value.Display.Background = new SolidColorBrush(Colors.LightGreen);
                    cooldown.Value.Display.Foreground = new SolidColorBrush(Colors.Black);
                }
            });
        }

        private void StartAction(Action action, string actionName, int cooldownSeconds = 0, TextBlock? cooldownDisplay = null)
        {
            if (cooldownDisplay != null && _cooldowns.TryGetValue(actionName, out var cooldown) && cooldown.Timer.IsEnabled)
            {
                Dispatcher.Invoke(() =>
                {
                    cooldownDisplay.Text = $"Wait {cooldown.RemainingSeconds}s";
                    cooldownDisplay.Background = new SolidColorBrush(Colors.Red);
                    cooldownDisplay.Foreground = new SolidColorBrush(Colors.White);
                });
                return;
            }

            if (actionName != "GlassCutting")
            {
                Program.SetStopAction(false);
            }

            Task.Run(() =>
            {
                try
                {
                    action();
                    Dispatcher.Invoke(() =>
                    {
                        AppendLog($"{actionName} completed.");
                        if (cooldownSeconds > 0 && cooldownDisplay != null)
                        {
                            StartCooldown(actionName, cooldownSeconds, cooldownDisplay);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"Error: {ex.Message}"));
                }
            });
        }

        private void AppendLog(string message)
        {
            Logger.WriteLine(message);
        }

        private void LockPick_Click(object sender, RoutedEventArgs e)
        {
            StartAction(() => LockPicking.StartProcess(), "LockPick", 240, LockPickCooldown);

        }

        private void GlassCutting_Click(object sender, RoutedEventArgs e)
        {
            StartAction(() => GlassCutting.StartProcess(), "GlassCutting", 15, GlassCuttingCooldown);
        }

        private void AutoATM_Click(object sender, RoutedEventArgs e)
        {
            StartAction(() => ATM.StartProcess(), "AutoATM", 360, AutoATMCooldown);
        }

        private void Crowbar_Click(object sender, RoutedEventArgs e)
        {
            StartAction(() => Crowbar.StartProcess(), "Crowbar");
        }

        private void RobBank_Click(object sender, RoutedEventArgs e)
        {
            StartAction(() =>
            {
                Dispatcher.Invoke(() => AppendLog("Robbing Bank (simulated action)..."));
            }, "RobBank", 240, RobBankCooldown);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Program.SetStopAction(true);
            AppendLog("Stopping current action...");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void StartCooldown(string actionName, int seconds, TextBlock cooldownDisplay)
        {
            if (_cooldowns.TryGetValue(actionName, out var cooldown))
            {
                cooldown.RemainingSeconds = seconds;
                Dispatcher.Invoke(() =>
                {
                    cooldownDisplay.Text = $"Wait {cooldown.RemainingSeconds}s";
                    cooldownDisplay.Background = new SolidColorBrush(Colors.Red);
                    cooldownDisplay.Foreground = new SolidColorBrush(Colors.White);
                });
                if (cooldown.RemainingSeconds > 0)
                {
                    cooldown.Timer.Start();
                }
            }
        }

        private void PlaySound(string actionName)
        {
            string soundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", actionName switch
            {
                "RobBank" => "Banks.wav",
                "AutoATM" => "ATM.wav",
                "GlassCutting" => "GlassCutting.wav",
                "LockPick" => "LockPick.wav",
                _ => "Default.wav"
            });
            try
            {
                using (var player = new SoundPlayer(soundFile))
                {
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => AppendLog($"Error playing {soundFile}: {ex.Message}"));
            }
        }

        private void ResetCooldown_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var cooldown in _cooldowns)
                {
                    if (cooldown.Value.Timer.IsEnabled)
                    {
                        cooldown.Value.Timer.Stop();
                    }
                    cooldown.Value.RemainingSeconds = 0;
                    cooldown.Value.Display.Text = cooldown.Value.InitialText;
                    cooldown.Value.Display.Background = new SolidColorBrush(Colors.LightGreen);
                    cooldown.Value.Display.Foreground = new SolidColorBrush(Colors.Black);
                    AppendLog($"Reset cooldown for {cooldown.Key}.");
                }
            });
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Gán phím số 1-5 cho các hành động
            if (e.Key == System.Windows.Input.Key.D1)
            {
                LockPick_Click(sender, new RoutedEventArgs());
            }
            else if (e.Key == System.Windows.Input.Key.D2)
            {
                GlassCutting_Click(sender, new RoutedEventArgs());
            }
            else if (e.Key == System.Windows.Input.Key.D3)
            {
                AutoATM_Click(sender, new RoutedEventArgs());
            }
            else if (e.Key == System.Windows.Input.Key.D4)
            {
                Crowbar_Click(sender, new RoutedEventArgs());
            }
            else if (e.Key == System.Windows.Input.Key.D5)
            {
                RobBank_Click(sender, new RoutedEventArgs());
            }
            // Gán phím Esc cho Exit
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                Exit_Click(sender, new RoutedEventArgs());
            }
        }
    }
}