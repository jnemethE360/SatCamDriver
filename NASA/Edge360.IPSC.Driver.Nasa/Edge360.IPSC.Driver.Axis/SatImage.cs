using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    public class SatImage
    {

        public SatImage()
        {

        }
        public DateTime Date { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string ImgURL { get; set; }

        public string ImgDir { get; set; }
    }
}
