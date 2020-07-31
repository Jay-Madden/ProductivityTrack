using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ProductivityTrack.Models
{
    public class CurrentProcessWithIconModel
    {
        public BitmapSource Icon { get; set; }

        public string Name { get; set; }

        public CurrentProcessWithIconModel(BitmapSource image, string n)
        {
            Icon = image;
            Name = n;

        }
    }
}
