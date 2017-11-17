using CNL.IPSecurityCenter.Driver.Attributes;

namespace Edge360.IPSC.Driver.Axis
{
    public enum AxisEncoding : int
    {
        [Description(Constants.Encoding.MotionJpeg, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Http, "mjpeg", "axis-cgi/mjpg/video.cgi?camera={0}")]
        MJpeg = 0,

        [Description(Constants.Encoding.Mpeg2Unicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Http, "mpeg2-unicast", "axis-cgi/mpeg2/{0}/video.cgi")]
        Mpeg2 = 1,

        [Description(Constants.Encoding.Mpeg2Multicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Axsdp, "mpeg2-multicast", "mpeg2-multicast/{0}")]
        Mpeg2Multicast = 2,

        [Description(Constants.Encoding.Mpeg4Unicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Axrtsphttp, "mpeg4", "mpeg4/{0}/media.amp")]
        Mpeg4 = 3,

        [Description(Constants.Encoding.Mpeg4Multicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Axrtpm, "mpeg4", "axis-media/media.amp")]
        Mpeg4Multicast = 4,

        [Description(Constants.Encoding.H264Unicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Rtsp, "h264", "axis-media/media.amp?videocodec=h264&camera={0}")]
        H264 = 5,

        [Description(Constants.Encoding.H264Multicast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Axrtpm, "h264", "axis-media/media.amp")]
        H264Multicast = 6,

        [Description(Constants.Encoding.GenericMulticast, typeof(IVideoServer))]
        [AxisEncoding(AxisEncodingScheme.Axrtpm, "", "axis-media/media.amp")]
        GenericMulticast = 7,
    }
}
