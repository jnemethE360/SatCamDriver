using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// The modes a video control can be in.
    /// </summary>
    internal enum VideoMode : int
    {
        /// <summary>
        /// No video is being displayed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The video control is in live mode.
        /// </summary>
        Live = 1,

        /// <summary>
        /// The video control is in playback mode.
        /// </summary>
        Playback = 2
    }
}
