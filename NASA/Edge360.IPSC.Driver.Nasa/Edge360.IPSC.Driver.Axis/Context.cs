using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// Defines the caller context.
    /// </summary>
    internal enum Context
    {
        None,
        ConnectionManager,
        VideoExport,
        LiveVideo,
        PlaybackVideo
    }
}
