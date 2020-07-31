using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ProductivityTrack.Models
{
    public class ProcessMetricsModel
    {
        public double OpenTime { get; set; }
        public string OpenTimeStr { get => $"Up Time: {OpenTime}"; } 

        public double FocusedTime { get; set; }
        public string FocusedTimeStr { get => $"Focused Time: {FocusedTime}"; }

        public double ActiveMouseTime { get; set; }
        public string ActiveMouseTimeStr { get => $"Active Mouse Time: {ActiveMouseTime}";  } 

        public List<int> WpmKeyPresses { get; set; } = new List<int> { 0 };

        public Process Process { get; set; }

        public BitmapSource Icon { get; set; }

        public string Name { get; set; }


        public ProcessMetricsModel(Process p, BitmapSource ico)
        {
            Process = p;
            Icon = ico;
            Name = p.ProcessName;
        }
    }
}
