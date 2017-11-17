using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.ServiceModel;
using CNL.IPSecurityCenter.Driver.Exceptions;
using CNL.IPSecurityCenter.Driver.Ptz;
using CNL.IPSecurityCenter.Driver.ServiceLocation;
using CNL.IPSecurityCenter.Driver.Video;
using CNL.IPSecurityCenter.Driver.Video.DeviceConnection;
using log4net;
using System.Collections.Generic;
using System.Globalization;
using CNL.IPSecurityCenter.Driver.Video.Playback;
using System.Xml.Linq;
using System.Net;



using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using CNL.IPSecurityCenter.Driver;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using System.Text.RegularExpressions;
using AXISMEDIACONTROLLib;
using System.Reflection;
using System.Net.Configuration;
using System.IO;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// Video control for the video server.
    /// </summary>
    public partial class VideoServerVideoControl
    {
        // -- constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoServerVideoControl"/> class.
        /// </summary>
        public VideoServerVideoControl()
        {
            InitializeComponent();

            log = LogManager.GetLogger(typeof(VideoServerVideoControl));
        }

        // -- fields

        private IContainer components = null;
        private IVideoCamera camera;
        private IVideoServer server;
        private DeviceConnectionInformation connectionInformation;

        private Context context;
        private VideoMode mode;
        private DateTime seekTime;

        private ILog log;
        private AxAXISMEDIACONTROLLib.AxAxisMediaControl amc;
        private string initializationError;

        [NonSerialized]
        private System.Threading.Timer timerFramePoll;

        //[NonSerialized]
        //private System.Timers.Timer currentTimePlayback;

        [NonSerialized]
        private Channel fakeStorageChannel = new Channel(30);

        [NonSerialized]
        private Channel realStorageChannel = new Channel(90);

        [NonSerialized]
        private bool paused;

        [NonSerialized]
        List<RecordingItem> listRecordings;

        [NonSerialized]
        DateTime currentPlaybackTime;


        [NonSerialized]
        RecordingItem recordDataMaster;

        [NonSerialized]
        private bool compatibilityMode = false;

        [NonSerialized]
        private DateTime lastEnd;

        // -- public methods

        /// <summary>
        /// Initializes the output control prior to the output being displayed on it.
        /// </summary>
        /// <param name="deviceIdentifier">The identifier for the device. Use the provided <paramref name="deviceDescriptorFactory"/>  for obtaining connection information for this device.</param>
        /// <param name="deviceDescriptorFactory">The factory for obtaining the <see cref="DeviceDescriptor"/> instance for a device identifier.</param>
        /// <param name="deviceRepository">Repository from which device instances can be obtained.</param>
        /// <remarks>
        /// This method does not initiate the display of video.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Initialize(Guid deviceIdentifier, IDeviceDescriptorFactory deviceDescriptorFactory, IDeviceRepository deviceRepository)
        {
            if (deviceDescriptorFactory == null)
                throw new ArgumentNullException("deviceDescriptorFactory");

            if (deviceRepository == null)
                throw new ArgumentNullException("deviceRepository");

            StopVideo();
            var cameraDescriptor = deviceDescriptorFactory.Create(deviceIdentifier);
            connectionInformation = cameraDescriptor.SimpleConnectionInformation.GetByParentType(typeof(VideoServer));

            try
            {
                server = deviceRepository.Read<IVideoServer>(connectionInformation.ParentIdentifier);
                camera = deviceRepository.Read<IVideoCamera>(cameraDescriptor.Identifier);
                initializationError = null;
                InitControl();

                //TODO: Reset any class level fields at this point
            }
            catch (EndpointNotFoundException ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, ErrorMessages.ConnectionManagerUnreachable, ex);
                server = null;
                camera = null;
                initializationError = ErrorMessages.ConnectionManagerUnreachable;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                server = null;
                camera = null;
                initializationError = ErrorMessages.VideoFeedNotAvailable;
            }
        }

        /// <summary>
        /// Begins the display of live video.
        /// </summary>
        /// <remarks>
        /// This method is called every time live video should be displayed.
        /// </remarks>
        public void ShowLiveVideo()
        {
            const string urlFormat = "{0}://{1}:{2}/{3}";
            try
            {
                context = Context.LiveVideo;
                ValidateDevices();

                StopVideo();
                mode = VideoMode.Live;

                // TODO display live video for the camera.
                //   The connection details can be found in the server, camera, and videoInput fields.
                //   Make sure the connection is cached if supported by the SDK, as the user could open up 16 cameras at once

                // TODO tell the host that video is connected and ready to be viewed

                //var port = server.Port > 0 ? server.Port : Constants.Network.DefaultPort;
                //var encoding = GetEnumAttribute<AxisEncodingAttribute>(server.Encoding);
                //var suffix = string.Format(CultureInfo.CurrentCulture, encoding.Suffix, server.Channel);
                //var scheme = GetEnumAttribute<System.ComponentModel.DescriptionAttribute>(encoding.Scheme).Description;
                var url = string.Format(CultureInfo.InvariantCulture, urlFormat, "http", server.IP, server.Port, "axis-cgi/mjpg/video.cgi?camera=1");

                amc.MediaURL = url;

                //Start live video
                amc.Play();


                OnStateChanged(VideoControlState.Connected);
                //throw new NotImplementedException();
            }
            catch (FatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(VideoControlState.Disconnected, ex.Message, ex.MoreInformation);
            }
            catch (NonFatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(VideoControlState.Disconnected, ex.Message, ex.MoreInformation);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, ErrorMessages.LiveVideoFeedNotAvailable, ex);
                OnStateChanged(VideoControlState.Disconnected, ErrorMessages.LiveVideoFeedNotAvailable);
                throw new FatalDriverException(ErrorMessages.LiveVideoFeedNotAvailable, ex);
            }
        }

        /// <summary>
        /// Initializes the control prior to video playback being started.
        /// This should not seek to a specific time, or start playback yet, 
        /// just make the control ready to recieve a seek command.
        /// </summary>
        public void ShowPlaybackVideo()
        {
            try
            {
                context = Context.PlaybackVideo;
                ValidateDevices();

                StopVideo();
                OnStateChanged(VideoControlState.Connecting, string.Empty, string.Empty);
                //amc.OnStatusChange += axisMediaControl_OnStatusChange;
                amc.Refresh();

                DownloadRecordingList();
                // We set properties here for clarity
                InitControl();

                // We set properties here for clarity
                amc.BackgroundColor = 0; // black
                amc.MaintainAspectRatio = true;
                amc.StretchToFit = true;
                //amc.Popups &= ~((int)AMC_POPUPS.AMC_POPUPS_NO_VIDEO); // Hide "Video stopped" message
                amc.ToolbarConfiguration = "default -pixcount -settings";

                // VMR9 is the recommended video renderer but VMR7 works better when using the
                // work-around provided to support seeking for older firmware versions (< 5.50).
               // amc.VideoRenderer = compatibilityMode ? (int)AMC_VIDEO_RENDERER.AMC_VIDEO_RENDERER_VMR7 : // VMR7
              //                                                                                  (int)AMC_VIDEO_RENDERER.AMC_VIDEO_RENDERER_VMR9;  // VMR9

               // amc.VideoRenderer = 0;
               // amc.PlaybackMode = 2;
                amc.EnableOverlays = false;

                // Optimize for playback of recording (not live)
               // amc.PlaybackMode = (int)AMC_PLAYBACK_MODE.AMC_PM_RECORDING;
                fakeStorageChannel.Add(new Chunk(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow));
                lastEnd = DateTime.UtcNow;
                OnChannelInformationChanged(new ChannelEventArgs(fakeStorageChannel));


                OnStateChanged(VideoControlState.Connected, null, null);
            }
            catch (FatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(VideoControlState.Disconnected, ex.Message, ex.MoreInformation);
            }
            catch (NonFatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(VideoControlState.Disconnected, ex.Message, ex.MoreInformation);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, ErrorMessages.RecordedVideoFeedNotAvailable, ex);
                OnStateChanged(VideoControlState.Disconnected, ErrorMessages.RecordedVideoFeedNotAvailable);
                throw new FatalDriverException(ErrorMessages.RecordedVideoFeedNotAvailable, ex);
            }
        }

        /// <summary>
        /// Seeks to the specified time..
        /// </summary>
        /// <param name="dateTimeUtc">The date time to go to.</param>
        public void Seek(DateTime dateTimeUtc)
        {
            seekTime = dateTimeUtc.ToLocalTime();

            if (mode == VideoMode.Playback)
            {
                //   If the requested time is not available, not error should occur but playback should instead be frozen
                Pause();
                amc.Stop();
                StopFramePollTimer();
                RecordingItem recordData = getCurrent(seekTime, true);
                recordDataMaster = recordData;

                if (recordData != null)
                {
                    //amc.MediaType = "mjpeg";
                    //InitControl();
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "axrtsphttp://";
                    uriBuilder.Host = server.IP;
                    uriBuilder.Port = server.Port;
                    uriBuilder.Path = "/axis-media/media.amp";
                    uriBuilder.Query = "recordingid=" + recordData.RecordingId;
                    loadWindow(uriBuilder.ToString(), server.Username, server.Password);

                    if (seekTime < recordData.StopTime && seekTime > recordData.StartTime)
                    {
                        TimeSpan span = seekTime - recordData.StartTime;
                        ulong ms = (ulong)span.TotalMilliseconds;
                        SetMediaPosition(ms, false);

                        currentPlaybackTime = seekTime;
                    }
                    else
                    {
                        currentPlaybackTime = recordData.StartTime;

                    }
    
                    amc.Play();
                    Play();

                }
            }
            else
            {
                RecordingItem recordData = getCurrent(seekTime, true);
                recordDataMaster = recordData;

                if (recordData != null)
                {

                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "axrtsphttp://";
                    uriBuilder.Host = server.IP;
                    uriBuilder.Port = server.Port;
                    uriBuilder.Path = "/axis-media/media.amp";
                    uriBuilder.Query = "recordingid=" + recordData.RecordingId;
                    loadWindow(uriBuilder.ToString(), server.Username, server.Password);
                    currentPlaybackTime = recordData.StartTime;

                    Play();
                }

            }
        }

        /// <summary>
        /// Starts video playback.
        /// </summary>
        /// <remarks>
        /// If there is no recorded video at the current playback time then playback starts at the nearest time to that specified.
        /// </remarks>
        public void Play()
        {
            paused = false;
            StartFramePollTimer();
            if (mode == VideoMode.Playback)
            {
                // TODO resume video from a paused state
                amc.Play();

            }
            else
            {
                amc.Play();
                mode = VideoMode.Playback;
                OnStateChanged(VideoControlState.Connected);
            }
        }

        /// <summary>
        /// Pauses the video playback.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if pause is not supported.</exception>
        public void Pause()
        {
            paused = true;
            amc.TogglePause();
        }

        /// <summary>
        /// Changes the playback speed i.e. rewinds and fast forwards the video.
        /// </summary>
        /// <param name="speed">The percentage of the normal playback speed to play at.</param>
        /// <exception cref="NotSupportedException">Thrown if the playback speed specified is not supported.</exception>
        public void ChangeSpeed(float speed)
        {
            // TODO implement if the video control supports
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Captures the the current video frame.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Drawing.Image"/> containing the current video frame.
        /// </returns>
        public Image CaptureImage()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var imagePath = Path.Combine(localAppData, "CNL\\IPSecurityCenter\\Drivers\\ImageCapture\\");
            var filename = Guid.NewGuid() + ".jpg";
            var fullPath = Path.Combine(imagePath, filename);
            bool saved = false;
            int attempts = 0;
            Image capturedImage;
            Image copy = null;

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            amc.SaveCurrentImage(0, fullPath);

            // HACK: give the control time to save the image. Max 2 second.
            do
            {
                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            capturedImage = Image.FromStream(fs);
                            copy = new Bitmap(Width, Height);

                            using (var g = Graphics.FromImage(copy))
                            {
                                g.DrawImage(capturedImage, new Rectangle(0, 0, Width, Height));
                            }
                        }

                        saved = true;
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            } while (!saved && attempts < 20);

            File.Delete(fullPath);

            return copy;
        }

        /// <summary>
        /// Pan Tilt and Zoom.
        /// </summary>
        /// <param name="pan"></param>
        /// <param name="tilt"></param>
        /// <param name="zoom"></param>
        public void PanTiltZoom(int pan, int tilt, int zoom)
        {
            // TODO implement pan tilt zoom, the values are from -100 to 100 depending on the speed and direction. 0,0,0 means stop.
            var ptzURL = InitUri();
            ptzURL.Scheme = "http://";
            ptzURL.Path = "/axis-cgi/com/ptz.cgi";
            int zoomspeed = server.zoomSpeed;
            if (zoomspeed < 1)
            {
                zoomspeed = 200;
            }
            ptzURL.Query = string.Format(CultureInfo.InvariantCulture, "continuouspantiltmove={0},{1}&rzoom={2}", pan, tilt, zoom * zoomspeed);

            //  var ptzURL = "ptz.cgi?";
            log.ErrorFormat(CultureInfo.InvariantCulture, ptzURL.ToString());
            WebClientGet(ptzURL.ToString());
        }

        /// <summary>
        /// Removes the specified preset.
        /// </summary>
        /// <param name="preset">The preset to remove.</param>
        public void Remove(Preset preset)
        {
            // TODO delete the preset
            //throw new NotImplementedException();
            updatePTZPreset("removeserverpresetname=" + preset.Label);
        }

        /// <summary>
        /// Selects the specified preset for the displayed camera.
        /// </summary>
        /// <param name="number">The preset number.</param>
        public void SelectPreset(int number)
        {
            // TODO goto the preset
            //throw new NotImplementedException();
            var presets = camera.GetPresets();
            var preset = presets.First(i => i.Number == number);
            updatePTZPreset("gotoserverpresetname=" + preset.Label);
        }

        /// <summary>
        /// Updates the specified preset.
        /// </summary>
        /// <param name="preset">The preset to update.</param>
        public void Update(Preset preset)
        {
            // TODO save the preset
            //throw new NotImplementedException();
            updatePTZPreset("setserverpresetname=" + preset.Label);
        }
        private void updatePTZPreset(string extensionURL)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "http://";
                uriBuilder.Host = server.IP;

                uriBuilder.Port = server.Port;
                uriBuilder.Path = "/axis-cgi/com/ptz.cgi";
                uriBuilder.Query = extensionURL;
                WebClient myWebClient = new WebClient();
                if (!String.IsNullOrEmpty(server.Username))
                {
                    myWebClient.Credentials = new NetworkCredential(server.Username, server.Password);
                }
                // Download home page data. 
                log.DebugFormat(CultureInfo.InvariantCulture, "Accessing {0} ...", uriBuilder.ToString());
                // Open a stream to point to the data stream coming from the Web resource.
                Stream myStream = myWebClient.OpenRead(uriBuilder.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "update Prezet: {0}", ex.Message);
            }


        }
        // -- protected methods

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            StopVideo();

            if (disposing && (components != null))
            {
                if (timerFramePoll != null)
                {
                    timerFramePoll.Dispose();
                }

                timerFramePoll = null;
                ////currentTimePlayback = null;
                listRecordings = null;

                listRecordings = null;


                this.amc.Dispose();

                components.Dispose();
            }

            base.Dispose(disposing);
        }

        // -- private methods

        /// <summary>
        /// Stops any video currently being displayed.
        /// </summary>
        private void StopVideo()
        {
            amc.Stop();
            if (mode != VideoMode.None)
            {
                amc.Stop();
                ////if (currentTimePlayback != null)
                ////{
                ////    currentTimePlayback.Elapsed -= Timer_Tick_Playback;

                ////    currentTimePlayback.Stop();
                ////    currentTimePlayback.Enabled = false;
                ////    currentTimePlayback.Dispose();
                ////}
                StopFramePollTimer();
            }

            mode = VideoMode.None;
        }

        /// <summary>
        /// Validates the devices have been loaded and connected correctly.
        /// </summary>
        private void ValidateDevices()
        {
            if (connectionInformation == null)
                throw new FatalDriverException(GetVideoServerIncompatibleMessage());

            if (!string.IsNullOrEmpty(initializationError))
            {
                throw new FatalDriverException(GetCameraNotConnectedMessage(), initializationError);
            }

            if (server == null || camera == null)
                throw new FatalDriverException(GetCameraNotConnectedMessage());
        }

        /// <summary>
        /// Returns the contextual video server incompatible message.
        /// </summary>
        /// <returns></returns>
        private string GetVideoServerIncompatibleMessage()
        {
            switch (context)
            {
                case Context.LiveVideo: return ErrorMessages.LiveVideoServerIncompatible;
                case Context.PlaybackVideo: return ErrorMessages.RecordedVideoServerIncompatble;
                default: throw new InvalidOperationException(ErrorMessages.ErrorNotFound);
            }
        }

        /// <summary>
        /// Returns the contextual camera not connected message.
        /// </summary>
        /// <returns></returns>
        private string GetCameraNotConnectedMessage()
        {
            switch (context)
            {
                case Context.LiveVideo:
                    return ErrorMessages.LiveVideoCameraNotConnected;
                case Context.PlaybackVideo:
                    return ErrorMessages.RecordedVideoCameraNotConnected;
                default:
                    throw new InvalidOperationException(ErrorMessages.ErrorNotFound);
            }
        }

        private string InitControl()
        {
            amc.MaintainAspectRatio = true;
            amc.StretchToFit = true;
            amc.MediaUsername = server.Username;
            amc.MediaPassword = server.Password;
            //amc.Popups = 0;
            amc.BackgroundColor = 0; // black

            //amc.MaintainAspectRatio = true;
            //amc.StretchToFit = true;
            amc.Popups &= 4; // Hide "Video stopped" message
            amc.ToolbarConfiguration = "default -pixcount -settings";

           // amc.VideoRenderer = 0;  // VMR9
            amc.EnableOverlays = false;

            // Optimize for playback of recording (not live)
          //  amc.PlaybackMode = 2;

            amc.Visible = true;
            amc.StretchToFit = true;
            return "";

        }

        private Boolean DownloadRecordingList()
        {
            try
            {
                // Comprehensive information about the Edge Storage API can be found on the partner pages:
                // http://www.axis.com/partner_pages/vapix3.php
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "http://";
                uriBuilder.Host = server.IP;
                uriBuilder.Path = "/axis-cgi/record/list.cgi";
                uriBuilder.Query = "recordingid=all";
                WebClient webClient = new WebClient();
                if (!String.IsNullOrEmpty(server.Username))
                {
                    webClient.Credentials = new NetworkCredential(server.Username, server.Password);
                }
                string result = webClient.DownloadString(uriBuilder.ToString());

                XDocument xmlDoc = XDocument.Load(new System.IO.StringReader(result));

                String frameRateFilter = xmlDoc.Descendants("recording").FirstOrDefault().Descendants("video").FirstOrDefault().Attribute("framerate").Value;
                IEnumerable<RecordingItem> querytemp = from r in xmlDoc.Descendants("recording")
                                                       orderby r.Attribute("starttime").Value
                                                       where r.Descendants("video").FirstOrDefault().Attribute("framerate").Value == frameRateFilter
                                                       select new RecordingItem()
                                                       {
                                                           RecordingId = (string)r.Attribute("recordingid").Value,
                                                           StartTime = IntParseDateTime(r.Attribute("starttime").Value),
                                                           StopTime = IntParseDateTime(r.Attribute("stoptime").Value),
                                                           RecordingStatus = (string)r.Attribute("recordingstatus").Value,
                                                           MimeType = (string)r.Descendants("video").FirstOrDefault().Attribute("mimetype").Value,
                                                           FrameRate = (string)r.Descendants("video").FirstOrDefault().Attribute("framerate").Value,
                                                           Audio = (string)((r.Descendants("audio").Count() > 0) ? "yes" : "no")
                                                       };

                listRecordings = new List<RecordingItem>();
                int indexTemp = 0;
                foreach (var item in querytemp)
                {

                    RecordingItem tempRecord = new RecordingItem()
                    {
                        RecordingId = item.RecordingId,
                        StartTime = item.StartTime,
                        StopTime = item.StopTime,
                        RecordingStatus = item.RecordingStatus,
                        MimeType = item.MimeType,
                        FrameRate = item.FrameRate,
                        Audio = item.Audio,
                        indexPos = indexTemp
                    };
                    if (tempRecord != null)
                        if (tempRecord.RecordingStatus == "completed")
                        {
                            realStorageChannel.Add(new Chunk(tempRecord.StartTime, tempRecord.StopTime));//.AddMonths()));



                            listRecordings.Add(tempRecord);
                            indexTemp++;

                        }

                }
                OnChannelInformationChanged(new ChannelEventArgs(realStorageChannel));
                return true;
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat("Failed to download recording list. " + ex.Message +
                    "\n\n(Note that the Edge Storage API is supported from firmware 5.40)");
                return false;
            }
        }
        private DateTime IntParseDateTime(string dateTimeString)
        {
            DateTime parsedDateTime;
            if (DateTime.TryParse(dateTimeString, out parsedDateTime))
            {
                return parsedDateTime;
            }
            else
            {
                return new DateTime();
            }
        }

        /// <summary>
        ///
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:ShortTimers")]
        private void StartFramePollTimer()
        {
            StopFramePollTimer();
            //Set up a timer to simulate playback position information from a device SDK
            timerFramePoll = new System.Threading.Timer(FramePollTimer_Tick, null, 500, 500);

        }


        /// <summary>
        ///
        /// </summary>
        private void StopFramePollTimer()
        {
            if (timerFramePoll != null)
            {
                using (var mre = new ManualResetEvent(false))
                {
                    timerFramePoll.Dispose(mre);
                    mre.WaitOne();
                    timerFramePoll = null;
                }
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="state"></param>
        private void FramePollTimer_Tick(object state)
        {
            //const
            TimeSpan epsilon = new TimeSpan(0, 1, 0);
            //_log.DebugFormat(CultureInfo.InvariantCulture, "FramePollTimer_Tick message {0}:{1}", refetched, paused);
            //Set the UI to reflect the current playback position.
            try
            {
                if (mode == VideoMode.Playback)
                {
                    DateTime dtg;
                    if (paused == true)
                    {
                        log.DebugFormat(CultureInfo.InvariantCulture, "Paused");
                    }
                    if (recordDataMaster != null)
                    {
                        dtg = recordDataMaster.StartTime.AddMilliseconds(amc.CurrentPosition64);
                    }

                    else
                    {
                        dtg = DateTime.Now;
                    }
                    if (dtg != DateTime.MinValue)
                    {
                        //var utcDtg = dtg.ToUniversalTime();
                        var utcDtg = dtg;

                        OnFrameDateChanged(new FrameDateChangedEventArgs(utcDtg));


                        if (DateTime.UtcNow - lastEnd > epsilon)
                        {
                            fakeStorageChannel.Add(new Chunk(lastEnd, DateTime.UtcNow));
                            lastEnd = DateTime.UtcNow;
                            OnChannelInformationChanged(new ChannelEventArgs(fakeStorageChannel));
                        }
                        /*   if (DateTime.UtcNow - utcDtg <= epsilon)
                        {
                            fakeStorageChannel.Add(new Chunk(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow));//.AddMonths()));

                            OnChannelInformationChanged(new ChannelEventArgs(fakeStorageChannel));
                        }*/
                    }

                }
            }
            catch (Exception ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message);
            }

        }

        private UriBuilder InitUri()
        {

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = server.IP;
            uriBuilder.Port = server.Port > 0 ? server.Port : Constants.Network.DefaultPort;
            InitControl();
            return uriBuilder;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private string WebClientGet(string uri)
        {
            using (var webClient = new WebClient())
            {

                webClient.Credentials = new NetworkCredential(server.Username, server.Password);
                return webClient.DownloadString(uri.ToString());
            }
        }

        #region playback
        private RecordingItem getCurrent(DateTime currentTime, Boolean firstTry)
        {
            try
            {
                if (listRecordings != null)
                {
                    RecordingItem itemFound2 = listRecordings.FirstOrDefault(xmlItems => xmlItems.StartTime <= currentTime && xmlItems.StopTime > currentTime);
                    RecordingItem itemFound = listRecordings.Find(xmlItems => xmlItems.StartTime <= currentTime && xmlItems.StopTime > currentTime);

                    if (itemFound != null)
                    {
                        RecordingItem prevItem = listRecordings.Find(xmlItem => xmlItem.indexPos == itemFound.indexPos - 1);
                        RecordingItem prevItem2 = listRecordings.Find(xmlItem => xmlItem.indexPos == itemFound.indexPos - 2);
                        RecordingItem nextItem = listRecordings.Find(xmlItem => xmlItem.indexPos == itemFound.indexPos + 1);

                        if (itemFound.RecordingStatus == "completed")
                        {
                            if (currentTime.AddSeconds(5) > itemFound.StopTime)
                            {
                                if (nextItem.RecordingStatus == "completed")
                                {

                                    return nextItem;
                                }
                                return null;
                            }
                            else
                            {
                                return itemFound;
                            }
                        }
                        else
                        {
                            if (prevItem != null)
                            {
                                if (prevItem.RecordingStatus == "completed")
                                {
                                    return prevItem;
                                }
                                if (prevItem2 != null)
                                {
                                    if (prevItem2.RecordingStatus == "completed")
                                    {
                                        return prevItem2;
                                    }
                                }

                            }

                            return null;
                        }
                    }
                    else
                    {
                        if (firstTry)
                        {
                            DownloadRecordingList();
                            return getCurrent(currentTime, false);
                        }
                        else
                        {
                            RecordingItem lastItem = listRecordings[listRecordings.Count - 1];

                            return lastItem;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, err.Message);
                if (!firstTry)
                {
                    if (listRecordings.Count > 0)
                    {
                        RecordingItem lastItem = listRecordings[listRecordings.Count - 1];

                        return lastItem;
                    }
                    return null;
                }

                return null;
            }
        }

        public void loadWindow(string recordingUrl, string userName, string password)
        {

            amc.MediaURL = recordingUrl;
            if (!String.IsNullOrEmpty(userName))
            {
                amc.MediaUsername = userName;
                amc.MediaPassword = password;
            }
        }

        private void SetMediaPosition(ulong newPosMilliSec, bool forceSeek)
        {
            //if (MediaDuration <= 0)
            //{
            //    return;
            //}
            amc.Play();
            if (compatibilityMode)
            {
                amc.CurrentPosition64 = newPosMilliSec;
          
            }
            else
            {
                amc.CurrentPosition64 = newPosMilliSec;
            }
            amc.TogglePause();
        }

        #endregion



        #region Axis Event Handler
        /// <summary>
        /// Handles the OnError event of the axisMediaControl control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void axisMediaControl_OnError(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnErrorEvent e)
        {


            StopVideo();
        }

        /// <summary>
        /// Handles the OnNewImage event of the axisMediaControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void axisMediaControl_OnNewImage(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Handles the OnStatusChange event of the axisMediaControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void axisMediaControl_OnStatusChange(object sender, AxAXISMEDIACONTROLLib._IAxisMediaControlEvents_OnStatusChangeEvent e)
        {

            //if (e.theNewStatus == 1)
            //{
            //    Pause();
            //}
        }

        #endregion

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoServerVideoControl));
            this.amc = new AxAXISMEDIACONTROLLib.AxAxisMediaControl();
            ((System.ComponentModel.ISupportInitialize)(this.amc)).BeginInit();
            this.SuspendLayout();
            // 
            // amc
            // 
            this.amc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.amc.Enabled = true;
            this.amc.Location = new System.Drawing.Point(0, 0);
            this.amc.Margin = new System.Windows.Forms.Padding(0);
            this.amc.Name = "amc";
            this.amc.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("amc.OcxState")));
            this.amc.Size = new System.Drawing.Size(363, 262);
            this.amc.TabIndex = 0;
            // 
            // VideoServerVideoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.amc);
            this.Name = "VideoServerVideoControl";
            this.Size = new System.Drawing.Size(363, 262);
            ((System.ComponentModel.ISupportInitialize)(this.amc)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
