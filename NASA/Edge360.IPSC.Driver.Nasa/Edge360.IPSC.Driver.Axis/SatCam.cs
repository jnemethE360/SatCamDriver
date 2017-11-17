using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CNL.IPSecurityCenter.Driver;
using CNL.IPSecurityCenter.Driver.Attributes;
using CNL.IPSecurityCenter.Driver.ServiceLocation;
using CNL.IPSecurityCenter.Driver.Video;
using CNL.IPSecurityCenter.Driver.Video.DeviceConnection;
using CNL.IPSecurityCenter.Driver.Video.Playback;
using CNL.IPSecurityCenter.Driver.Video.Export;
using CNL.IPSecurityCenter.Driver.Ptz;
using VideoDeviceInterfaceDescriptor = CNL.IPSecurityCenter.Driver.Video.DeviceConnection.DeviceInterfaceDescriptor;

namespace Edge360.IPSC.Driver.Axis
{

    /// <summary>
    /// A SatCam is a position over the Earth. It's a lat/long representation that can contain multiple images of a spot across time.
    /// </summary>
    public class SatCam : Device
    {
        private List<SatImage> _ImageSpan;
        private double _latitude;
        private double _longitude;
        private int _currentIndex;

        private DateTime _RangeStart;
        private DateTime _RangeEnd;

        public SatCam(double lat, double lon)
        {
            _latitude = lat;
            _longitude = lon;
            //set defaults for start and end of time scale range
            _RangeStart = new DateTime(2015, 1, 1);
            _RangeEnd = new DateTime(2017, 1, 1);

            BuildTimeLapse(_RangeStart, _RangeEnd);
            _currentIndex = _ImageSpan.Count - 1;
            Label = "Satellite_Cam_" + lat.ToString() + "-" + lon.ToString();

            Interfaces.Add(new DeviceInterface(DeviceInterfaceType.Video, "Video Output"));
            return;
        }

        public SatCam(double lat, double lon, DateTime TimeRangeStart, DateTime TimeRangeEnd)
        {
            _latitude = lat;
            _longitude = lon;
            _RangeStart = TimeRangeStart;
            _RangeEnd = TimeRangeEnd;

            BuildTimeLapse(_RangeStart, _RangeEnd);
            _currentIndex = _ImageSpan.Count - 1;
            Label = "Satellite_Cam_" + lat.ToString() + "-" + lon.ToString();

            Interfaces.Add(new DeviceInterface(DeviceInterfaceType.Video, "Video Output"));
            return;
        }

        public double CamLongitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                _longitude = value;
            }
        }

        public double CamLatitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                _latitude = value;
            }
        }

        /// <summary>
        /// Get Datetimes for the start and stop parameters of the cam's time lapse range.
        /// </summary>
        /// <returns></returns>
        public List<DateTime> GetTimeRange()
        {
            List<DateTime> timeRange = new List<DateTime>();
            timeRange.Add(_RangeStart);
            timeRange.Add(_RangeEnd);
            return timeRange;
        }

        /// <summary>
        /// Helper function for outside class to specify a new time range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void SetTimeRange(DateTime start, DateTime end)
        {
            BuildTimeLapse(start, end);
        }

        /// <summary>
        /// Compile a list of image metadata for the different times at this position.
        /// </summary>
        private void BuildTimeLapse(DateTime start, DateTime end)
        {
            _RangeStart = start;
            _RangeEnd = end;
            _ImageSpan = new List<SatImage>();
            DateTime currentDate = start;
            string inputDate = "";
            string returnedDate = "";

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https://";
            uriBuilder.Host = "api.nasa.gov";
            uriBuilder.Path = @"/planetary/earth/imagery";
            WebClient webClient = new WebClient();
            WebClient imgWebClient = new WebClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            while (currentDate <= end)
            {
                inputDate = currentDate.Date.ToString("yyyy-mm-dd");

                uriBuilder.Query = string.Format("lon={0}&lat={1}&date={2}&api_key=3j9axhTtE4nROpHHLY3jBQJ7H3Y0rjrrgDKbwTxj", _longitude, _latitude, inputDate);
                
                string result = webClient.DownloadString(uriBuilder.ToString());
                if (returnedDate.Equals(result.Substring(10, 10)))
                {   //If the date is the same, the date-step was not enough to get the next sat image. skip this image to avoid duplicates
                    currentDate.AddDays(10);
                    continue;
                }

                returnedDate = result.Substring(10, 10);
                //Step 10 days
                currentDate.AddDays(10);

                SatImage newImage = new SatImage();
                newImage.ImgURL = result.Substring(40, 124); //substring for URL path in query return. This should be changed to an xml node to be more versatile.
                newImage.Latitude = _latitude;
                newImage.Longitude = _longitude;
                _ImageSpan.Add(newImage);
            }
        }

        /// <summary>
        /// Gets the URL for the image in the cam's timespan equivalent to the seek-bar's placement.
        /// Note: I'm not sure if the interface's seek bar actually uses percentage as its param; if it doesn't I'll have to adjust the seek accordingly.
        /// </summary>
        /// <param name="seekPercentage">The seek-bar's placement represented by percentage (left-most == 0, right-most == 100)</param>
        /// <returns>The image's URL</returns>
        public string Seek(double seekPercentage)
        {
            int imgCount = _ImageSpan.Count();
            //take percentage of the count of the image list, then round to the nearest int.  Adjust for starting index of 0
            int index = Convert.ToInt32(imgCount * (seekPercentage / 100)) - 1;

            //catch edges just in case
            if (index > imgCount)
                index = imgCount - 1;
            if (index < 0)
                index = 0;

            return _ImageSpan[index].ImgURL;
        }

        public string GetCurrentImage()
        {
            return _ImageSpan[_currentIndex].ImgURL;
        }

        public SatImage GetNextImage()
        {
            //make sure you're not already on the last image in the span
            if(_ImageSpan.Count-1 > _currentIndex)
                _currentIndex++;
            return _ImageSpan[_currentIndex];
        }

        public SatImage GetPreviousImage()
        {
            //make sure you're not already on the first image in the span
            if (0 < _currentIndex)
                _currentIndex--;
            return _ImageSpan[_currentIndex];
        }
        /// <summary>
        /// Get the current image's date
        /// </summary>
        /// <returns></returns>
        public DateTime GetImageDate()
        {
            return _ImageSpan[_currentIndex].Date;
        }


    }
}
