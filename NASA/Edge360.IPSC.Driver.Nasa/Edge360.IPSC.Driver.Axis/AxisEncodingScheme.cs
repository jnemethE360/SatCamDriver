using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Edge360.IPSC.Driver.Axis
{
    public enum AxisEncodingScheme : int
    {
        [Description("http")]
        Http = 0,
        [Description("axsdp")]
        Axsdp = 1,
        [Description("axrtsphttp")]
        Axrtsphttp = 2,
        [Description("axrtpm")]
        Axrtpm = 3,
        [Description("rtsp")]
        Rtsp = 4
    }
}
