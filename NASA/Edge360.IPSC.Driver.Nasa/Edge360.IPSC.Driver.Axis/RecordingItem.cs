using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge360.IPSC.Driver.Axis
{
    class RecordingItem
    {
        public int indexPos { get; set; }
        //   [CategoryPropertiesAttribute]
        //    [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorreaderID", typeof(RecordingListEventArgs))]
        //     [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorreaderID", typeof(RecordingListEventArgs))]
        public string RecordingId { get; set; }




        //    [CategoryPropertiesAttribute]
        //   [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorname", typeof(RecordingListEventArgs))]
        //    [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorname", typeof(RecordingListEventArgs))]
        public DateTime StartTime { get; set; }




        //    [CategoryPropertiesAttribute]
        //  [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErroronline", typeof(RecordingListEventArgs))]
        //    [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErroronline", typeof(RecordingListEventArgs))]
        public bool online { get; set; }


        //    [CategoryPropertiesAttribute]
        //     [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorDoorId", typeof(RecordingListEventArgs))]
        //     [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorDoorId", typeof(RecordingListEventArgs))]
        public DateTime StopTime { get; set; }



        //    [CategoryPropertiesAttribute]
        //   [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorisOnline", typeof(RecordingListEventArgs))]
        //   [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorisOnline", typeof(RecordingListEventArgs))]
        public string RecordingStatus { get; set; }



        //     [CategoryPropertiesAttribute]
        //   [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorisCommError", typeof(RecordingListEventArgs))]
        //    [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorisCommError", typeof(RecordingListEventArgs))]
        public string MimeType { get; set; }


        //  [CategoryPropertiesAttribute]
        //  [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorisTamper", typeof(RecordingListEventArgs))]
        //     [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorisTamper", typeof(RecordingListEventArgs))]

        public string FrameRate { get; set; }


        //  [CategoryPropertiesAttribute]
        //  [DisplayName("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DisplayNameReaderCommunicationErrorisPINRequired", typeof(RecordingListEventArgs))]
        //    [Description("Edge360.IPSC.SH.CCURE.Design.driverdesign.Strings", "DescriptionReaderCommunicationErrorisPINRequired", typeof(RecordingListEventArgs))]
        public string Audio { get; set; }
    }
}
