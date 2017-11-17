using CNL.IPSecurityCenter.Driver;
using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using System.Linq;
using CNL.IPSecurityCenter.Driver.Exceptions;
using CNL.IPSecurityCenter.Driver.Ptz;
using log4net;
using CNL.IPSecurityCenter.Driver.Video.Export;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Net.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;

namespace Edge360.IPSC.Driver.Axis
{
    /// <summary>
    /// Represents the VideoServer class, implementing the IVideoServer contract.
    /// </summary>
    [Serializable]
    [ServiceBehaviorAttribute(InstanceContextMode = InstanceContextMode.Single)]
    public class VideoServer : VideoServerBase, IDeserializationCallback, IVideoServer
    {
        // -- constructor

        public VideoServer()
            : base()
        {
            OnDeserialization(null);
        }

        // -- fields

        [NonSerialized]
        private object lockInstance;

        [NonSerialized]
        private ILog log;

        [NonSerialized]
        private bool disposed;

        [NonSerializedAttribute]
        WebClient client;

        public AxisEncoding Encoding { get; set; }

        public int Channel { get; set; }

        [NonSerialized]
        private Timer m_timer;

        // -- public methods

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {
            InitializeFields();
        }

        /// <summary>
        /// Connects to the physical device.
        /// </summary>
        public override void Connect()
        {
            const string connecting = "Connecting to the server: {0}@{1}:{2}";
            const string connected = "Connected successfully to the server: {0}@{1}:{2}";

            try
            {
                CheckDisposed();

                var username = DeviceDefaults.DefaultUsername(this);
                var password = DeviceDefaults.DefaultPassword(this);
                var port = DeviceDefaults.DefaultPort(this);

                log.InfoFormat(CultureInfo.CurrentCulture, connecting, username, IP, port);

                lock (lockInstance)
                {
                    Disconnect();

                    //PopulateCameras();
                    PopulateSatCam();

                    VerifyPing();
                    VerifyHost();

                    m_timer = new Timer(new TimerCallback(Connection_Tick), null, 0, this.Timeout.Milliseconds);

                    //TODO connect to the physical device. 
                    //Throw the DeviceException with standard error messages from ErrorMessages.resx if there is a specific failure.

                    
                    OnStateChanged(DeviceState.Online);
                }

                log.InfoFormat(CultureInfo.CurrentCulture, connected, username, IP, port);
            }
            catch (FatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(DeviceState.Failed, ex.Message);
            }
            catch (NonFatalDriverException ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, ex.Message, ex);
                OnStateChanged(DeviceState.Failed, ex.Message);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, ErrorMessages.DeviceConnectionFailed, ex);
                OnStateChanged(DeviceState.Failed, ErrorMessages.DeviceConnectionFailed);
                throw new FatalDriverException(ErrorMessages.DeviceConnectionFailed, ex);
            }


        }

        /// <summary>
        /// Disconnects from the physical device.
        /// </summary>
        public override void Disconnect()
        {
            const string disconnecting = "Disconnecting from the server: {0}@{1}:{2}";
            const string disconnected = "Disconnected from the server: {0}@{1}:{2}";

            CheckDisposed();

            log.InfoFormat(CultureInfo.CurrentCulture, disconnecting, Username, IP, Port);

            lock (lockInstance)
            {
                // TODO disconnect from the physical device.
                if (m_timer != null)
                {
                    m_timer.Dispose();
                }
            }

            log.InfoFormat(CultureInfo.CurrentCulture, disconnected, Username, IP, Port);

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the camera connected to the specified interface identifier to a preset position.
        /// </summary>
        /// <param name="preset">The preset to move the camera to.</param>
        /// <param name="interfaceIdentifier">The identifier of the interface the camera is connected to.</param>
        public override void SelectPreset(Preset preset, Guid interfaceIdentifier)
        {
            //TODO: Add support for going to preset
            //throw new NotImplementedException();

            /*
            CheckDisposed();
             
            if (preset == null)
                throw new ArgumentNullException("preset");

            if (!Interfaces.Contains(interfaceIdentifier))
                return;

            var deviceInterface = Interfaces[interfaceIdentifier];
            var customIdentifier = deviceInterface.CustomIdentifier;

            //TODO: Find the SDK camera from the custom identifier and move to the requested preset
            var sdkCamera = listOfCamerasFromSdk[customIdentifier];
            sdkCamera.Presets[preset.Number].MoveTo();
            
            */
        }

        /// <summary>
        /// Starts video export with the provided task.
        /// </summary>
        /// <param name="task"></param>
        public override void Start(VideoExportTask task)
        {

            try
            {
                //Get all the variables out of the task
                var startTimeUTC = task.Parameters[VideoExportStaticParameter.StartTime].CastValue<DateTime>();
                var endTimeUTC = task.Parameters[VideoExportStaticParameter.EndTime].CastValue<DateTime>();
                var fileName = task.Parameters[VideoExportStaticParameter.FileName].CastValue<string>();
                //var cameraId = Convert.ToInt32(this.Interfaces[task.VideoServerInterfaceIdentifier].CustomIdentifier);
                OnProgress(new VideoExportProgressEventArgs(task, 0));
                DateTime startTime = startTimeUTC.ToLocalTime();
                DateTime endTime = endTimeUTC.ToLocalTime();

                double latitude = 100.75;
                double longitude = 1.5;
                string date = "2017-11-15";

                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "http://";
                uriBuilder.Host = "api.nasa.gov";
                uriBuilder.Path = "/planetary/earth/imagery?";
                uriBuilder.Query = string.Format("lat={0}&lon={1}&date={2}&key=3j9axhTtE4nROpHHLY3jBQJ7H3Y0rjrrgDKbwTxj",latitude, longitude, date);
                WebClient webClient = new WebClient();
                if (!String.IsNullOrEmpty(Username))
                {
                    webClient.Credentials = new NetworkCredential(Username, Password);
                }
                string result = webClient.DownloadString(uriBuilder.ToString());
                XDocument xmlDoc = XDocument.Load(new System.IO.StringReader(result));
                OnProgress(new VideoExportProgressEventArgs(task, 1));
                //List<RecordingItem> matchedItems = new List<RecordingItem>();
                List<RecordingItem> matchedItems = parseCameraList(xmlDoc.Descendants("recording"), startTime, endTime);
   
                List<RecordingItem> SortedList = matchedItems.OrderBy(o => o.StartTime).ToList();
                String keyResults = null;
                int tempIndex = 0;
                foreach (var item in SortedList)
                {
                    tempIndex += 1;
                    if (tempIndex < 100)
                    {
                        if (tempIndex == SortedList.Count)
                        {
                            keyResults = keyResults + item.RecordingId;
                        }
                        else
                        {
                            keyResults = keyResults + item.RecordingId + ",";
                        }
                    }
                    else
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "out of bounds recording pull.");
                    }
                }

                if (keyResults != null)
                {
                    if (keyResults.Length > 5)
                    {
                        //test(fileName, "record/download", selectedItem.RecordingId, task);
                        var taskDownloadFile = DownloadFile(@fileName, "record/download", keyResults, SortedList.Count, task);
                        //OnProgress(new VideoExportProgressEventArgs(task, 0));

                        taskDownloadFile.ContinueWith((antecedent) =>
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "COMPLETED Download");
                            task.OutputFiles.Add(new VideoExportOutputFile(fileName + ".zip"));
                            OnCompleted(new VideoExportCompletedEventArgs(task));
                            //Progress = null;
                            //Completed = null;
                        });
                    }
                }
                else
                {

                    OnCompleted(new VideoExportCompletedEventArgs(task));
                }

            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ERROR On Download: " + ex);
                OnCompleted(new VideoExportCompletedEventArgs(task));

            }
        }
        /// <summary>
        /// create Camera T List from SML Doc
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private List<RecordingItem> parseCameraList(IEnumerable<XElement> enumerable, DateTime startTime, DateTime endTime )
        {
            List<RecordingItem> matchedItems = new List<RecordingItem>();
            foreach (var item in enumerable)
            {
                DateTime startItem = IntParseDateTime(item.Attribute("starttime").Value);
                DateTime stopItem = IntParseDateTime(item.Attribute("stoptime").Value);
                RecordingItem tempRecordedItem = null;
                if (startItem <= startTime && stopItem >= endTime)
                {
                    tempRecordedItem = new RecordingItem()
                    {
                        RecordingId = (string)item.Attribute("recordingid").Value,
                        StartTime = stopItem,
                        StopTime = startItem,
                        RecordingStatus = (string)item.Attribute("recordingstatus").Value,
                        MimeType = (string)item.Descendants("video").FirstOrDefault().Attribute("mimetype").Value,
                        FrameRate = (string)item.Descendants("video").FirstOrDefault().Attribute("framerate").Value,
                        Audio = (string)((item.Descendants("audio").Count() > 0) ? "yes" : "no")
                    };
                }
                else if (startItem >= startTime && startItem <= endTime)
                {
                    if (tempRecordedItem == null)
                    {
                        tempRecordedItem = new RecordingItem()
                        {
                            RecordingId = (string)item.Attribute("recordingid").Value,
                            StartTime = stopItem,
                            StopTime = startItem,
                            RecordingStatus = (string)item.Attribute("recordingstatus").Value,
                            MimeType = (string)item.Descendants("video").FirstOrDefault().Attribute("mimetype").Value,
                            FrameRate = (string)item.Descendants("video").FirstOrDefault().Attribute("framerate").Value,
                            Audio = (string)((item.Descendants("audio").Count() > 0) ? "yes" : "no")
                        };
                    }
                }
                else if (stopItem >= startTime && stopItem <= endTime)
                {
                    if (tempRecordedItem == null)
                    {
                        tempRecordedItem = new RecordingItem()
                        {
                            RecordingId = (string)item.Attribute("recordingid").Value,
                            StartTime = stopItem,
                            StopTime = startItem,
                            RecordingStatus = (string)item.Attribute("recordingstatus").Value,
                            MimeType = (string)item.Descendants("video").FirstOrDefault().Attribute("mimetype").Value,
                            FrameRate = (string)item.Descendants("video").FirstOrDefault().Attribute("framerate").Value,
                            Audio = (string)((item.Descendants("audio").Count() > 0) ? "yes" : "no")
                        };
                    }
                }
                if (tempRecordedItem != null)
                    if (tempRecordedItem.RecordingStatus == "completed")
                    {
                        log.DebugFormat(CultureInfo.InvariantCulture, "added recorded stamp: " + tempRecordedItem);
                        matchedItems.Add(tempRecordedItem);
                    }
            }
            return matchedItems;
        }

        /// <summary>
        /// Pauses the video export with the provided task.
        /// </summary>
        /// <param name="task"></param>
        public override void Pause(VideoExportTask task)
        {
            //TODO: Support pausing export, if supported by SDK.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels the video export with the provided task.
        /// </summary>
        /// <param name="task"></param>
        public override void Cancel(VideoExportTask task)
        {
            //TODO: Support cancelling export, if supported by SDK.
            if (task == null)
                throw new ArgumentNullException("task");

            if (client != null)
            {
                //var handle = exportTaskHandle[task];
                //sdkSession.StopDownload(handle);
                //exportTaskHandle.Remove(task);
                this.client.CancelAsync();

            }
        }

        /// <summary>
        /// Called when the object is to be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposed)
            {
                if (disposing)
                {
                    // TODO dispose any disposable components
                    Disconnect();
                }

                if (m_timer != null)
                    m_timer.Dispose();

                disposed = true;
            }
        }

        #region Download Files
        /// <summary>
        /// Task fired on file download
        /// </summary>
        /// <param name="targetFolder"></param>
        /// <param name="service"></param>
        /// <param name="videoID"></param>
        /// <param name="fileNum"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task DownloadFile(string targetFolder, string service, string videoID, int fileNum, VideoExportTask task)
        {
            try
            {
                int filesize = fileNum * 1000000;//getting an average number to the file download
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "http://";
                uriBuilder.Host = this.IP;// ipBox.Text;
                uriBuilder.Path = string.Format("/axis-cgi/{0}.cgi", service);
                const string q = "recordingid={0}&duration=20&videocodec=mjpeg&compression=80&clock=1&date=1";
                uriBuilder.Query = string.Format(q, videoID);

                client = new WebClient();
                //so use THIS instead to send credenatials RIGHT AWAY
                string credentials = Convert.ToBase64String(
                     System.Text.Encoding.ASCII.GetBytes(Username + ":" + Password));
                client.Headers[HttpRequestHeader.Authorization] = string.Format(
                    "Basic {0}", credentials);
                client.DownloadProgressChanged += client_DownloadProgressChanged(task, filesize);
                client.DownloadFileCompleted += client_DownloadFileCompleted(task);
                await client.DownloadFileTaskAsync(uriBuilder.Uri, targetFolder + ".zip");
            }
            catch (Exception ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, "Failed to download File: " + videoID);
                log.ErrorFormat(CultureInfo.InvariantCulture, ex.Message);
            }
        }
        /// <summary>
        /// vidoe download progress tick
        /// </summary>
        /// <param name="task"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        public DownloadProgressChangedEventHandler client_DownloadProgressChanged(VideoExportTask task, int totalSize)
        {
            Action<object, DownloadProgressChangedEventArgs> action = (sender, e) =>
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = totalSize;
                double percentage = bytesIn / totalBytes * 98;
                if (percentage > 98)
                {
                    percentage = 98;
                }
                percentage = percentage + 1;

                OnProgress(new VideoExportProgressEventArgs(task, (int)Math.Round(percentage, 1)));

            };
            return new DownloadProgressChangedEventHandler(action);
        }

        /// <summary>
        /// Event Handler on Video download complete
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public AsyncCompletedEventHandler client_DownloadFileCompleted(VideoExportTask task)
        {
            Action<object, AsyncCompletedEventArgs> action = (sender, e) =>
            {
                try
                {
                    if (e.Error != null)
                    {
                        throw new Exception("file_error", e.Error);
                    }
                    if (e.Cancelled)
                    {
                        throw new Exception("file_cancelled");
                    }

                }
                catch (Exception err)
                {
                    if (err.Message == "file_error")
                    {
                        log.WarnFormat(CultureInfo.InvariantCulture, axisMessages.FileError);

                    }
                    else if (err.Message == "file_cancelled")
                    {
                        log.WarnFormat(CultureInfo.InvariantCulture, axisMessages.ExportCancelled);

                    }
                    else
                    {
                        log.WarnFormat(CultureInfo.InvariantCulture, axisMessages.ExportError);

                    }

                }
            };
            return new AsyncCompletedEventHandler(action);
        }
        #endregion

        // -- private methods

        /// <summary>
        /// Initializes the fields used by the instance.
        /// </summary>
        /// <remarks>
        /// NonSerialized fields should be initialized in this method otherwise
        /// they will only be initialized the first time the device is instantiated
        /// and will not initialize when a device instance is deserialized;
        /// </remarks>
        private void InitializeFields()
        {
            lockInstance = new object();
            log = LogManager.GetLogger(typeof(VideoServer));

            // TODO reset fields to their default values
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Populates the cameras connected to the video server.
        /// </summary>
        private void PopulateCameras()
        {
            // TODO update the cameras connected to the video server. 
            // Loop through the cameras provided by the SDK and copy their properties into the DDK camera class.

            const string addingCamera = "Adding new camera, Identifier='{0}', Label='{1}'";

            //foreach (var sdkCamera in listOfCamerasFromSdk)
            //{
            var customIdentifier = IP;
            if (!this.Interfaces.Contains(customIdentifier))
            {
                //if (this.Interfaces.Contains(customIdentifier))
                //    continue;

                log.InfoFormat(CultureInfo.CurrentCulture, addingCamera, customIdentifier, "AXIS Camera");

                //Create the DDK camera from the SDK camera
                var ddkCamera = new VideoCamera();
                ddkCamera.Label = "Axis Camera";
                ddkCamera.PtzSupported = true;
                ddkCamera.PresetsSupported = true;

                foreach (var testItem in GetPresets())
                {

                    log.DebugFormat(CultureInfo.InvariantCulture, testItem.Key + ", " + testItem.Value.Name + "||" + testItem.Value.Data);

                    string resultString = Regex.Match(testItem.Key, @"\d+").Value;

                    Preset ddkPreset = new Preset(Int32.Parse(resultString), testItem.Value.Name);
                   
                    ddkCamera.Update(ddkPreset);

                }

                //Creating the device interface and connecting it to the server interface
                var serverInput = new DeviceInterface(DeviceInterfaceType.Video, ddkCamera.Label, customIdentifier);
                this.Interfaces.Add(serverInput);

                var cameraOutput = ddkCamera.Interfaces.First();
                serverInput.Connect(cameraOutput);
                //}  
            }

        }

        /// <summary>
        /// Create a satalite camera position
        /// </summary>
        private void PopulateSatCam()
        {
            double longitude = 100.75;
            double latitude = 1.055;

            SatCam cam = new SatCam(latitude, longitude);

            //Creating the device interface and connecting it to the server interface
            var serverInput = new DeviceInterface(DeviceInterfaceType.Video, cam.Label, cam.Label);
            this.Interfaces.Add(serverInput);

            //need to look into this still. It seems unlikely this portion actually works since the SatCam object is so different from a VideoCamera
            var cameraOutput = cam.Interfaces.First();
            serverInput.Connect(cameraOutput);
        }

        public void DownloadImage(SatCam cam)
        {
            WebClient WebClient = new WebClient();
            WebClient.DownloadFile(new Uri(cam.GetCurrentImage()), @"C:\development\experimental\NASA\sat_pics\sat" + cam.CamLatitude.ToString() + "_" + cam.CamLongitude.ToString() + "_" + cam.GetImageDate().ToString() + ".png");
        }

        /// <summary>
        /// Gets the next picture in time of the lat long
        /// </summary>
        public SatImage GetNextImage(SatCam cam)
        {
            return cam.GetNextImage();
        }

        /// <summary>
        /// Gets the previous picture in time of the lat long
        /// </summary>
        public SatImage GetPreviousImage(SatCam cam)
        {
            return cam.GetPreviousImage();
        }

        /// <summary>
        /// Move to a new camera position based on the user's mouse click on the interface.
        /// Note: center of screen is 0.0; top-left is -100,100.
        /// </summary>
        /// <param name="x">x mouse coordinate</param>
        /// <param name="y">y mouse coordinate</param>
        /// <param name="cam">current cam position being referenced</param>
        /// <returns></returns>
        public SatCam MoveSatCam(SatCam cam, double x, double y)
        {
            //I'm not sure how big these jumps really are.  They may have to be rescaled to make sense.
            double lat = cam.CamLatitude;
            double lon = cam.CamLongitude;
            List<DateTime> timeRange = cam.GetTimeRange();

            if ((x > -50 && x < 50) && (y > -50 && y < 50))
                return null;

            //magnitudes for increase latitude
            if (x > 50)
                lat += 0.01;
            if (x > 70)
                lat += 0.01;
            if (x > 90)
                lat += 0.01;
            //decrease latitude
            if (x < -50)
                lat -= 0.01;
            if (x < -70)
                lat -= 0.01;
            if (x < -90)
                lat -= 0.01;
            //increase longitude
            if (y > 50)
                lon += 0.01;
            if (y > 70)
                lon += 0.01;
            if (y > 90)
                lon += 0.01;
            //decrease longitude
            if (y < -50)
                lon -= 0.01;
            if (y < -70)
                lon -= 0.01;
            if (y < -90)
                lon -= 0.01;

            //instantiate new camera with same time range and new coordinates
            SatCam newCam = new SatCam(lat, lon, timeRange[0], timeRange[1]);
            return newCam;
        }

        /// <summary>
        /// Updates the specified preset.
        /// </summary>
        /// <param name="preset">The preset to update.</param>
        public Dictionary<string, AxisPreset> GetPresets()
        {
            // TODO save the preset
            //this.media.SetPreset((Int16)preset.Number);
            try
            {
                PresetCollection returnPresets = new PresetCollection();
                ToggleAllowUnsafeHeaderParsing(true);
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "http://";
                uriBuilder.Host = this.IP;
                uriBuilder.Port = this.Port;
                uriBuilder.Path = "/axis-cgi/param.cgi";
                uriBuilder.Query = "action=list&group=PTZ.Preset";

                WebClient myWebClient = new WebClient();
                if (!String.IsNullOrEmpty(Username))
                {
                    myWebClient.Credentials = new NetworkCredential(this.Username, this.Password);
                }
                // Download home page data. 
                // Open a stream to point to the data stream coming from the Web resource.
                Stream myStream = myWebClient.OpenRead(uriBuilder.ToString());

                Dictionary<string, AxisPreset> dictionary = new Dictionary<string, AxisPreset>();
                //StreamReader sr = new StreamReader(myStream);
                using (StreamReader sr = new StreamReader(myStream))
                {
                    while (sr.Peek() >= 0)
                    {

                        string filteredData = sr.ReadLine();


                        if (filteredData != null)
                        {
                            var gottenData = filteredData.Split(new char[] { '=' }, 2);
                            string keyData = gottenData[0];
                            var tempKeys = keyData.Split('.');
                            if (tempKeys.Length > 3)
                            {
                                string tempkey = tempKeys[tempKeys.Length - 2];
                                if (tempkey != null)
                                {
                                    if (dictionary.ContainsKey(tempkey))
                                    {
                                        if (dictionary[tempkey].Name == null || dictionary[tempkey].Name == "")
                                        {
                                            dictionary[tempkey].Name = gottenData[1];
                                        }
                                        if (dictionary[tempkey].Data == null || dictionary[tempkey].Data == "")
                                            dictionary[tempkey].Data = gottenData[1];
                                    }
                                    else
                                    {
                                        AxisPreset tempPreset = new AxisPreset();
                                        if (tempKeys[tempKeys.Length - 1] == "Name")
                                        {
                                            tempPreset.Name = gottenData[1];
                                        }
                                        else
                                        {
                                            tempPreset.Data = gottenData[1];
                                        }
                                        dictionary.Add(tempkey, tempPreset);
                                    }

                                }
                            }

                        }
                    }
                }
                ToggleAllowUnsafeHeaderParsing(false);

                return dictionary;


            }
            catch (Exception ex)
            {

                log.ErrorFormat(CultureInfo.InvariantCulture, "Get Presets: ", ex.Message);

                return null;
            }

        }
        /// <summary>
        /// Toggle to allow Unsafe Header Parsing TO receive Video Recording
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Verifies the server is pingable.
        /// </summary>
        private void VerifyPing()
        {
            using (var ping = new Ping())
            {
                try
                {
                    if (ping.Send(IP).Status != IPStatus.Success)
                    {
                        throw new FatalDriverException(ErrorMessages.DeviceUnreachable);
                    }
                }
                catch (PingException)
                {
                    throw new FatalDriverException(ErrorMessages.DeviceUnreachable);
                }
            }
        }

        /// <summary>
        /// Verify Host
        /// </summary>
        /// <exception cref="FatalDriverException">Thrown when the device address cannot be reached.</exception>
        private void VerifyHost()
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    tcpClient.Connect(IP, DeviceDefaults.DefaultPort(this));
                }
            }
            catch (SocketException ex)
            {
                throw new FatalDriverException(ErrorMessages.DeviceUnavailable, ex);
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the instance has been disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().FullName);
        }

        /// <summary>
        /// Heart Beat on Camera connection
        /// </summary>
        /// <param name="state"></param>
        public void Connection_Tick(object state)
        {


            //JObject infoSession = session.getInfo();
            //const string pingUrl = "http://{0}:{1}/cameras?accept=application/json";
            const string pingUrl = "http://{0}:{1}/axis-cgi/admin/date.cgi?action=get";
            string urlPing = String.Format(pingUrl, IP, Port);
            log.InfoFormat(urlPing);
            CookieContainer myContainer = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(urlPing));
            request.ContentType = "application/json";
            request.Method = "GET";
            request.Credentials = new NetworkCredential(Username, Password);
            request.CookieContainer = myContainer;
            request.PreAuthenticate = true;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    OnStateChanged(DeviceState.Failed, ErrorMessages.LiveVideoFeedLost);

                }

            }
            catch (System.Net.WebException webEx)
            {
                log.InfoFormat(webEx.ToString());
                OnStateChanged(DeviceState.Failed, ErrorMessages.LiveVideoFeedLost);

            }
        }
        /// <summary>
        /// Converting Strings to DateFormat Return current Date if date cast error
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        private DateTime IntParseDateTime(string dateTimeString)
        {
            try
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
            catch (Exception ex)
            {
                log.WarnFormat(CultureInfo.InvariantCulture, axisMessages.ErrorParsingDate + ex.Message);
                return DateTime.Now;
            }
        }
    }
    public class AxisPreset
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }
}
