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
            public string InitialText { get; set; } = null!;
        }

        private readonly Dictionary<string, CooldownInfo> _cooldowns = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CheckRoblox();
            SetupCooldowns();

            KeyDown += MainWindow_KeyDown;

            Logger.OnLogMessage += m => Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(m + "\n");
                LogTextBox.ScrollToEnd();
            });
        }

        /* ─────────────────────────── Utility ─────────────────────────── */
        public string SystemScaleMultiplier => Screen.SystemScaleMultiplier.ToString("F2");

        private void AppendLog(string msg) => Logger.WriteLine(msg);

        private void CheckRoblox()
        {
            if (Roblox.IsRobloxRunning()) return;
            AppendLog("Waiting for Roblox to open...");
            Task.Run(async () =>
            {
                while (!Roblox.IsRobloxRunning()) await Task.Delay(500);
                Dispatcher.Invoke(() => AppendLog("Roblox is running!"));
            });
        }

        /* ─────────────────────────── Cooldowns ─────────────────────────── */
        private void SetupCooldowns()
        {
            _cooldowns["AutoATM"] = new() { Display = AutoATMCooldown };
            _cooldowns["RobBank"] = new() { Display = RobBankCooldown };
            _cooldowns["GlassCutting"] = new() { Display = GlassCuttingCooldown };
            _cooldowns["LockPick"] = new() { Display = LockPickCooldown };

            foreach (var cd in _cooldowns.Values)
            {
                cd.Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                cd.Timer.Tick += (_, _) => UpdateCooldownTick(cd);
            }

            foreach (var kv in _cooldowns)
            {
                kv.Value.InitialText = $"{kv.Key}: Ready";
                ResetDisplay(kv.Value);
            }
        }

        private void UpdateCooldownTick(CooldownInfo cd)
        {
            if (cd.RemainingSeconds > 0)
            {
                cd.RemainingSeconds--;
                cd.Display.Text = $"Wait {cd.RemainingSeconds}s";
                cd.Display.Background = Brushes.Red;
                cd.Display.Foreground = Brushes.White;
            }
            else
            {
                cd.Timer.Stop();
                ResetDisplay(cd);
                string name = _cooldowns.FirstOrDefault(x => x.Value == cd).Key;
                PlaySound(name);
            }
        }

        private static void ResetDisplay(CooldownInfo cd)
        {
            cd.Display.Text = cd.InitialText;
            cd.Display.Background = Brushes.LightGreen;
            cd.Display.Foreground = Brushes.Black;
        }

        private void StartCooldown(string name, int seconds, TextBlock display)
        {
            if (!_cooldowns.TryGetValue(name, out var cd)) return;
            cd.Timer.Stop();
            cd.RemainingSeconds = seconds;
            display.Text = $"Wait {cd.RemainingSeconds}s";
            display.Background = Brushes.Red;
            display.Foreground = Brushes.White;
            cd.Timer.Start();
        }

        /* ─────────────────────────── StartAction core ─────────────────────────── */
        private void StartAction(Action action, string name, int cdSeconds = 0, TextBlock? cdDisplay = null)
        {
            // ➜ Không block khi cooldown chạy, nhưng reset countdown về lại sau khi action kết thúc.
            if (cdDisplay != null && _cooldowns.TryGetValue(name, out var cdRunning) && cdRunning.Timer.IsEnabled)
            {
                // thông báo nhưng vẫn cho chạy tiếp
                cdRunning.Timer.Stop(); // dừng đếm hiện tại để reset sau
                cdRunning.RemainingSeconds = 0;
                cdDisplay.Text = "Running...";
                cdDisplay.Background = Brushes.Orange;
                cdDisplay.Foreground = Brushes.Black;
            }

            if (name != "GlassCutting") Program.SetStopAction(false);

            Task.Run(() =>
            {
                try
                {
                    if (!Roblox.IsRobloxRunning())
                    {
                        Dispatcher.Invoke(() => AppendLog("! ~ Roblox is not running."));
                        return;
                    }
                    action();
                    Dispatcher.Invoke(() => AppendLog($"{name} completed."));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"Error: {ex.Message}"));
                }
                finally
                {
                    Program.SetStopAction(false);
                    if (cdSeconds > 0 && cdDisplay != null)
                        Dispatcher.Invoke(() => StartCooldown(name, cdSeconds, cdDisplay));
                }
            });
        }

        /* ─────────────────────────── Button handlers ─────────────────────────── */
        private void LockPick_Click(object s, RoutedEventArgs e) => StartAction(() => LockPicking.StartProcess(), "LockPick", 240, LockPickCooldown);
        private void GlassCutting_Click(object s, RoutedEventArgs e) => StartAction(() => GlassCutting.StartProcess(), "GlassCutting", 15, GlassCuttingCooldown);
        private void AutoATM_Click(object s, RoutedEventArgs e) => StartAction(() => ATM.StartProcess(), "AutoATM", 360, AutoATMCooldown);
        private void Crowbar_Click(object s, RoutedEventArgs e) => StartAction(() => Crowbar.StartProcess(), "Crowbar");
        private void RobBank_Click(object s, RoutedEventArgs e) => StartAction(() => AppendLog("Robbing Bank (simulated)..."), "RobBank", 360, RobBankCooldown);

        /* ─────────────────────────── Misc handlers ─────────────────────────── */
        private void Stop_Click(object s, RoutedEventArgs e) { Program.SetStopAction(true); AppendLog("Stopping current action..."); }
        private void Exit_Click(object s, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Hyperlink_RequestNavigate(object s, RequestNavigateEventArgs e)
        { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); e.Handled = true; }

        private void ResetCooldown_Click(object s, RoutedEventArgs e)
        {
            foreach (var kv in _cooldowns)
            {
                kv.Value.Timer.Stop();
                kv.Value.RemainingSeconds = 0;
                ResetDisplay(kv.Value);
            }
            AppendLog("All cooldowns reset.");
        }

        /* ─────────────────────────── Key bindings ─────────────────────────── */
        private void MainWindow_KeyDown(object s, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.D1: LockPick_Click(s, new()); break;
                case System.Windows.Input.Key.D2: GlassCutting_Click(s, new()); break;
                case System.Windows.Input.Key.D3: AutoATM_Click(s, new()); break;
                case System.Windows.Input.Key.D4: Crowbar_Click(s, new()); break;
                case System.Windows.Input.Key.D5: RobBank_Click(s, new()); break;
                case System.Windows.Input.Key.Escape: Exit_Click(s, new()); break;
            }
        }

        /* ─────────────────────────── Sound ─────────────────────────── */
        private void PlaySound(string action)
        {
            string path = action switch
            {
                "RobBank" => "Banks.wav",
                "AutoATM" => "ATM.wav",
                "GlassCutting" => "GlassCutting.wav",
                "LockPick" => "LockPick.wav",
                _ => "Default.wav"
            };
            if (string.IsNullOrEmpty(path)) return;
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", path);
            if (!File.Exists(path)) return;
            try { using var p = new SoundPlayer(path); p.Play(); }
            catch (Exception ex) { AppendLog($"Error playing sound: {ex.Message}"); }
        }
    }
}
