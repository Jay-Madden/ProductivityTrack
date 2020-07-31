using Gma.System.MouseKeyHook;
using LiveCharts;
using LiveCharts.Wpf;
using ProductivityTrack.Models;
using ProductivityTrack.Pages.Events;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ProductivityTrack.Pages
{

    public class MetricsViewModel : Stylet.Screen
    {
        public Process[] Processes { get; set; }

        public Dictionary<int, ProcessMetricsModel> Metrics = new Dictionary<int, ProcessMetricsModel>();

        private ObservableCollection<ProcessMetricsModel> _metricsXaml;
        public ObservableCollection<ProcessMetricsModel> MetricsXaml
        {
            get => _metricsXaml;
            set {
                SetAndNotify(ref _metricsXaml, value);
            }
        }

        private Dictionary<int, PieSeries> PieChartSeriesCollection { get; set; } = new Dictionary<int, PieSeries>();

        private Dictionary<int, ColumnSeries> MouseActivitySeriesCollection { get; set; } = new Dictionary<int, ColumnSeries>();

        private Dictionary<int, LineSeries> KeyBoardActivitySeriesCollection { get; set; } = new Dictionary<int, LineSeries>();

        public SeriesCollection FocusedTimeValues { get; set; } = new SeriesCollection();

        public SeriesCollection MouseActivityChart { get; set; } = new SeriesCollection();

        public SeriesCollection KeyboardActivityChart { get; set; } = new SeriesCollection();

        public ObservableCollection<CurrentProcessWithIconModel> CurrentWindows { get; set; } = new ObservableCollection<CurrentProcessWithIconModel>();

        private Point _prevCur = Cursor.Position;

        private IKeyboardMouseEvents m_GlobalHook;

        private IEventAggregator _eventAggregator;

        public MetricsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;


            Processes = Process.GetProcesses();

            foreach (Process p in Processes)
            {
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    using (var ico = Icon.ExtractAssociatedIcon(p.MainModule.FileName))
                    {
                        BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Metrics[p.Id] = new ProcessMetricsModel(p, bitmap);
                    };

                    PieChartSeriesCollection[p.Id] = new PieSeries
                    {
                        Title = p.ProcessName,
                        Values = new ChartValues<double> { 1.0 },
                        DataLabels = true
                    };

                    MouseActivitySeriesCollection[p.Id] = new ColumnSeries
                    {
                        Title = p.ProcessName,
                        Values = new ChartValues<double> { 1.0 },
                        DataLabels = true
                    };

                    KeyBoardActivitySeriesCollection[p.Id] = new LineSeries
                    {
                        Title = p.ProcessName,
                        Values = new ChartValues<int>(Metrics[p.Id].WpmKeyPresses),
                        DataLabels = true,

                    };

                    FocusedTimeValues.Add(PieChartSeriesCollection[p.Id]);

                    MouseActivityChart.Add(MouseActivitySeriesCollection[p.Id]);

                    KeyboardActivityChart.Add(KeyBoardActivitySeriesCollection[p.Id]);

                    using (var ico = Icon.ExtractAssociatedIcon(p.MainModule.FileName))
                    {
                        BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        CurrentWindows.Add(new CurrentProcessWithIconModel(bitmap, p.ProcessName));
                    };

                    MetricsXaml = new ObservableCollection<ProcessMetricsModel>(Metrics.Values);


                }
            }

            StartPollThreads();
            SubscribeToKeyBoardEvents();
        }

        public void SubscribeToKeyBoardEvents()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != ' ')
                return;
            var pid = GetForegroundProcessId();

            if (Metrics.ContainsKey(pid))
            {
                var lastIndex = Metrics[pid].WpmKeyPresses.Count - 1;
                Metrics[pid].WpmKeyPresses[lastIndex]++;
                MetricsXaml = new ObservableCollection<ProcessMetricsModel>(Metrics.Values);
            }
        }

        private void StartPollThreads()
        {
            Thread t = new Thread(PollProcesses);
            t.IsBackground = true;
            t.Start();
            Thread kbt = new Thread(CheckWpm);
            kbt.IsBackground = true;
            kbt.Start();
        }

        private void CheckWpm()
        {
            while (true)
            {
                Thread.Sleep(60000);
                UpdateKeyBoardMetrics();
            }

        }

        private void PollProcesses()
        {
            while (true)
            {
                Thread.Sleep(500);
                UpdateProcessMetrics();
            }
        }

        private void UpdateKeyBoardMetrics()
        {
            foreach (KeyValuePair<int, ProcessMetricsModel> entry in Metrics)
            {
                KeyBoardActivitySeriesCollection[entry.Key].Values.Add(Metrics[entry.Key].WpmKeyPresses.Last());
                entry.Value.WpmKeyPresses.Add(0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private void UpdateProcessMetrics()
        {
            Processes = Process.GetProcesses();
            foreach (Process p in Processes)
            {
                if (!string.IsNullOrEmpty(p.MainWindowTitle) && !Metrics.ContainsKey(p.Id))
                {

                    Execute.OnUIThread(() =>
                        {
                            using (var ico = Icon.ExtractAssociatedIcon(p.MainModule.FileName))
                            {
                                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                Metrics[p.Id] = new ProcessMetricsModel(p, bitmap);
                            };
                            var piChartSeries = new PieSeries
                            {
                                Title = p.ProcessName,
                                Values = new ChartValues<double> { Metrics[p.Id].FocusedTime },
                                DataLabels = true
                            };
                            PieChartSeriesCollection[p.Id] = piChartSeries;
                            FocusedTimeValues.Add(piChartSeries);

                            var mouseActivitySeries = new ColumnSeries
                            {
                                Title = p.ProcessName,
                                Values = new ChartValues<double> { Metrics[p.Id].ActiveMouseTime },
                                DataLabels = true
                            };
                            MouseActivitySeriesCollection[p.Id] = mouseActivitySeries;
                            MouseActivityChart.Add(mouseActivitySeries);

                            var kbSeries = new LineSeries
                            {
                                Title = p.ProcessName,
                                Values = new ChartValues<int>(Metrics[p.Id].WpmKeyPresses),
                                DataLabels = true
                            };
                            KeyBoardActivitySeriesCollection[p.Id] = kbSeries;
                            KeyboardActivityChart.Add(KeyBoardActivitySeriesCollection[p.Id]);

                            MetricsXaml = new ObservableCollection<ProcessMetricsModel>(Metrics.Values);

                        }
                    );
                    continue;
                }
                else if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    Metrics[p.Id].OpenTime += 0.5;
                    Execute.OnUIThread(() =>
                    {
                        PieChartSeriesCollection[p.Id].Values[0] = Metrics[p.Id].FocusedTime;
                    }
                    );
                }
            }

            int pid = GetForegroundProcessId();
            if (Metrics.ContainsKey(pid))
                Metrics[pid].FocusedTime += 0.5;
            Execute.OnUIThread(() =>
                {
                    CurrentWindows.Clear();
                });
            foreach (var p in Processes)
            {
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {

                    Execute.OnUIThread(() =>
                        {
                            try
                            {
                                using (var ico = Icon.ExtractAssociatedIcon(p.MainModule.FileName))
                                {
                                    BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                    CurrentWindows.Add(new CurrentProcessWithIconModel(bitmap, p.ProcessName));
                                };
                            }
                            catch { }
                        });
                }
            }
            CheckMouseActivity();
        }

        private void CheckMouseActivity()
        {
            var currCursor = Cursor.Position;

            if (_prevCur != currCursor)
            {
                int pid = GetForegroundProcessId();
                if (Metrics.ContainsKey(pid))
                {
                    Metrics[pid].ActiveMouseTime += 0.5;
                }

                if (MouseActivitySeriesCollection.ContainsKey(pid))
                {
                    MouseActivitySeriesCollection[pid].Values[0] = Metrics[pid].ActiveMouseTime;
                }
            }

            _prevCur = currCursor;
        }

        private int GetForegroundProcessId()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances,
            // such as when a window is losing activation.
            if (hwnd == null)
                return 0;

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id == pid)
                    return (int)pid;
            }

            return 0;
        }

        public void OnClick_ExitMetrics()
        {
            _eventAggregator.Publish(new ExitMetrics());
        }
    }
}