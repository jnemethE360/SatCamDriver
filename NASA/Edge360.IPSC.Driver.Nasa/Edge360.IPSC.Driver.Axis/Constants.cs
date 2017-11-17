using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    internal class Constants
    {
        private Constants() { }

        public const string ResourcePath = ResourcePathRoot + ".Strings";
        private const string ResourcePathImages = ResourcePathRoot + ".Images";
        private const string ResourcePathRoot = "CNL.IPSecurityCenter.Driver.Axis";

        internal static class Network
        {
            public const int DefaultPort = 80;
        }

        internal static class Manufacturer
        {
            public const string Name = "Axis";
            public const string Description = "Axis is an IT company offering network video solutions for professional installations.";
            public const string SupportUrl = "http://www.axis.com/techsup/index.htm";
            public const string Url = "http://www.axis.com";
            public const string Image = ResourcePathImages + ".manufacturerlogo.jpg";
            public const string ImageCaption = "Axis";
        }

        internal class Camera
        {
            private Camera() { }

            public const string Name = "Axis Communications Video Encoder Camera";
            public const string Description = "Camera for the Axis Video Encoder devices.";
            public const string Url = "http://www.axis.com";
            public const string Image = ResourcePathImages + ".camera128.png";
            public const string ImageCaption = "Axis Camera";
        }

        internal static class Server
        {
            public const string Name = "Axis Communications Video Encoder";
            public const string Description = "Device for the entire range of Axis video encoders and cameras.";
            public const string Url = "http://www.axis.com/products/video/video_server/index.htm";
            public const string Image = ResourcePathImages + ".server128.png";
            public const string ImageCaption = "Axis Video Encoder";
        }

        internal static class Images
        {
            public const string Camera16x16 = ResourcePathImages + ".camera16.png";
            public const string Camera24x24 = ResourcePathImages + ".camera24.png";
            public const string Camera32x32 = ResourcePathImages + ".camera32.png";
            public const string Camera64x64 = ResourcePathImages + ".camera64.png";
            public const string Server16x16 = ResourcePathImages + ".server16.png";
            public const string Server24x24 = ResourcePathImages + ".server24.png";
            public const string Server32x32 = ResourcePathImages + ".server32.png";
            public const string Server64x64 = ResourcePathImages + ".server64.png";
        }

        internal static class Encoding
        {
            private const string Prefix = "Encoding";

            public const string MotionJpeg = Prefix + "MotionJpeg";
            public const string Mpeg2Unicast = Prefix + "Mpeg2Unicast";
            public const string Mpeg2Multicast = Prefix + "Mpeg2Multicast";
            public const string Mpeg4Unicast = Prefix + "Mpeg4Unicast";
            public const string Mpeg4Multicast = Prefix + "Mpeg4Multicast";
            public const string H264Unicast = Prefix + "H264Unicast";
            public const string H264Multicast = Prefix + "H264Multicast";
            public const string GenericMulticast = Prefix + "GenericMulticast";
        }

        internal static class ErrorMessage
        {
            public const string Prefix = "ErrorMessage";

            public const string NET_PROBLEMS = Prefix + "NET_PROBLEMS";
            public const string NO_MORE_STREAM = Prefix + "NO_MORE_STREAM";
            public const string ConnectedNO_MORE_STREAM = Prefix + "ConnectedNO_MORE_STREAM";
            public const string NO_UDP_VIDEO_STREAM_STARTED = Prefix + "NO_UDP_VIDEO_STREAM_STARTED";
            public const string HTTP_NOT_FOUND = Prefix + "HTTP_NOT_FOUND";
            public const string CONNECT_SOURCE = Prefix + "CONNECT_SOURCE";
            public const string CONNECT_SOURCE_ALL = Prefix + "CONNECT_SOURCE_ALL";
            public const string DECODER_MISSING = Prefix + "DECODER_MISSING";
            public const string AXIS_E_MEDIA_FAIL = Prefix + "AXIS_E_MEDIA_FAIL";
            public const string NO_PTZ_CONTROL_URL = Prefix + "NO_PTZ_CONTROL_URL";
            public const string PTZ_FAIL = Prefix + "PTZ_FAIL";
            public const string AACDMO_NOT_FOUND = Prefix + "AACDMO_NOT_FOUND";
            public const string INIT_UNEXPECTED = Prefix + "INIT_UNEXPECTED";
            public const string INIT_ACCESSDENIED = Prefix + "INIT_ACCESSDENIED";
            public const string INIT_COMPONENT_MISSING = Prefix + "INIT_COMPONENT_MISSING";
            public const string INIT_COMPONENT_OLD = Prefix + "INIT_COMPONENT_OLD";
            public const string MISSING_SNAPSHOT_FOLDER = Prefix + "MISSING_SNAPSHOT_FOLDER";
            public const string MISSING_RECORDING_FOLDER = Prefix + "MISSING_RECORDING_FOLDER";
            public const string WRONG_MEDIA_TYPE = Prefix + "WRONG_MEDIA_TYPE";
            public const string NO_PROTOCOL = Prefix + "NO_PROTOCOL";
            public const string DIRECTX_VERSION = Prefix + "DIRECTX_VERSION";
            public const string AUDIO_NET_PROBLEMS = Prefix + "AUDIO_NET_PROBLEMS";
            public const string AUDIO_MAX_CLIENTS = Prefix + "AUDIO_MAX_CLIENTS";
            public const string AUDIO_CLOSE_RECEIVE = Prefix + "AUDIO_CLOSE_RECEIVE";
            public const string AUDIO_CLOSE_TRANSMIT = Prefix + "AUDIO_CLOSE_TRANSMIT";
            public const string AUDIO_CLIENT = Prefix + "AUDIO_CLIENT";
            public const string AUDIO_PROXY = Prefix + "AUDIO_PROXY";
            public const string AUDIO_MAX_REC_CLIENTS = Prefix + "AUDIO_MAX_REC_CLIENTS";
            public const string AUDIO_TRANSMIT = Prefix + "AUDIO_TRANSMIT";
            public const string AUDIO_FILE = Prefix + "AUDIO_FILE";
            public const string AUDIO_NO_MORE_STREAM = Prefix + "AUDIO_NO_MORE_STREAM";
            public const string RECORDING_COMPONENT_MISSING = Prefix + "RECORDING_COMPONENT_MISSING";
            public const string RECORDING_NO_STREAM = Prefix + "RECORDING_NO_STREAM";
            public const string AFW_E_WMF_MISSING = Prefix + "AFW_E_WMF_MISSING";
            public const string AFW_E_ACCESS_DENIED = Prefix + "AFW_E_ACCESS_DENIED";
            public const string AFW_E_WRITER_FAILED = Prefix + "AFW_E_WRITER_FAILED";
            public const string AFW_E_OLD_VERSION = Prefix + "AFW_E_OLD_VERSION";
        }

        internal static class ErrorAdditionalInfo
        {
            public const string Prefix = "ErrorAdditionalInfo";

            public const string NET_PROBLEMS = Prefix + "NET_PROBLEMS";
            public const string NO_MORE_STREAM = Prefix + "NO_MORE_STREAM";
            public const string ConnectedNO_MORE_STREAM = Prefix + "ConnectedNO_MORE_STREAM";
            public const string NO_UDP_VIDEO_STREAM_STARTED = Prefix + "NO_UDP_VIDEO_STREAM_STARTED";
            public const string HTTP_NOT_FOUND = Prefix + "HTTP_NOT_FOUND";
            public const string CONNECT_SOURCE = Prefix + "CONNECT_SOURCE";
            public const string CONNECT_SOURCE_ALL = Prefix + "CONNECT_SOURCE_ALL";
            public const string DECODER_MISSING = Prefix + "DECODER_MISSING";
            public const string AXIS_E_MEDIA_FAIL = Prefix + "AXIS_E_MEDIA_FAIL";
            public const string NO_PTZ_CONTROL_URL = Prefix + "NO_PTZ_CONTROL_URL";
            public const string PTZ_FAIL = Prefix + "PTZ_FAIL";
            public const string AACDMO_NOT_FOUND = Prefix + "AACDMO_NOT_FOUND";
            public const string INIT_UNEXPECTED = Prefix + "INIT_UNEXPECTED";
            public const string INIT_ACCESSDENIED = Prefix + "INIT_ACCESSDENIED";
            public const string INIT_COMPONENT_MISSING = Prefix + "INIT_COMPONENT_MISSING";
            public const string INIT_COMPONENT_OLD = Prefix + "INIT_COMPONENT_OLD";
            public const string MISSING_SNAPSHOT_FOLDER = Prefix + "MISSING_SNAPSHOT_FOLDER";
            public const string MISSING_RECORDING_FOLDER = Prefix + "MISSING_RECORDING_FOLDER";
            public const string WRONG_MEDIA_TYPE = Prefix + "WRONG_MEDIA_TYPE";
            public const string NO_PROTOCOL = Prefix + "NO_PROTOCOL";
            public const string DIRECTX_VERSION = Prefix + "DIRECTX_VERSION";
            public const string AUDIO_NET_PROBLEMS = Prefix + "AUDIO_NET_PROBLEMS";
            public const string AUDIO_MAX_CLIENTS = Prefix + "AUDIO_MAX_CLIENTS";
            public const string AUDIO_CLOSE_RECEIVE = Prefix + "AUDIO_CLOSE_RECEIVE";
            public const string AUDIO_CLOSE_TRANSMIT = Prefix + "AUDIO_CLOSE_TRANSMIT";
            public const string AUDIO_CLIENT = Prefix + "AUDIO_CLIENT";
            public const string AUDIO_PROXY = Prefix + "AUDIO_PROXY";
            public const string AUDIO_MAX_REC_CLIENTS = Prefix + "AUDIO_MAX_REC_CLIENTS";
            public const string AUDIO_TRANSMIT = Prefix + "AUDIO_TRANSMIT";
            public const string AUDIO_FILE = Prefix + "AUDIO_FILE";
            public const string AUDIO_NO_MORE_STREAM = Prefix + "AUDIO_NO_MORE_STREAM";
            public const string RECORDING_COMPONENT_MISSING = Prefix + "RECORDING_COMPONENT_MISSING";
            public const string RECORDING_NO_STREAM = Prefix + "RECORDING_NO_STREAM";
            public const string AFW_E_WMF_MISSING = Prefix + "AFW_E_WMF_MISSING";
            public const string AFW_E_ACCESS_DENIED = Prefix + "AFW_E_ACCESS_DENIED";
            public const string AFW_E_WRITER_FAILED = Prefix + "AFW_E_WRITER_FAILED";
            public const string AFW_E_OLD_VERSION = Prefix + "AFW_E_OLD_VERSION";
        }

        internal static class DisplayNames
        {
            private const string Prefix = "DisplayName";

            public const string Channel = Prefix + "Channel";
            public const string Encoding = Prefix + "Encoding";
            public const string FirmwareVersion = Prefix + "FirmwareVersion";

            public const string Restart = Prefix + "Restart";
            public const string SetTime = Prefix + "SetTime";
            public const string Time = Prefix + "Time";

        }

        internal static class Descriptions
        {
            private const string Prefix = "Description";

            public const string Channel = Prefix + "Channel";
            public const string Encoding = Prefix + "Encoding";
            public const string FirmwareVersion = Prefix + "FirmwareVersion";
            public const string Firmware4xx = Prefix + "Firmware4xx";
            public const string Firmware5xx = Prefix + "Firmware5xx";

            public const string Restart = Prefix + "Restart";
            public const string SetTime = Prefix + "SetTime";
            public const string Time = Prefix + "Time";
        }
    }
}
