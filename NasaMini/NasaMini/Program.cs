using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace NasaMini
{
    class Program
    {
        public static void Main(string[] args)
        {
            int modeSwitch = 0;
            switch (modeSwitch)
            {
                case 0:
                    Sat sat = new Sat();
                    sat.GetSatImage();
                    break;
                case 1:
                    WebCam webCam = new WebCam();
                    webCam.GetWebCam();
                    break;
                default:
                    break;
            }


        }
    }

    class Sat
    {
        public void GetSatImage()
        {
            double longitude = 100.75;
            double latitude = 1.055;
            string date = "2017-01-15";

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https://";
            uriBuilder.Host = "api.nasa.gov";
            uriBuilder.Path = @"/planetary/earth/imagery";
            uriBuilder.Query = string.Format("lon={0}&lat={1}&date={2}&api_key=3j9axhTtE4nROpHHLY3jBQJ7H3Y0rjrrgDKbwTxj", longitude, latitude, date);
            WebClient webClient = new WebClient();
            WebClient imgWebClient = new WebClient();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


            string result = webClient.DownloadString(uriBuilder.ToString());
            //XDocument xmlDoc = XDocument.Load(new System.IO.StringReader(result));
            //List<string> matchedItems = xmlDoc.Nodes();

            string imageURL = result.Substring(40, 124); //substring for URL path in query return. This should be changed to an xml node to be more versatile
            string returnedDate = result.Substring(10, 10);
            //var obj = JObject.Parse(json);
            //var url = (string)obj.SelectToken("url");
            imgWebClient.DownloadFile(new Uri(imageURL), @"C:\development\experimental\NASA\sat_pics\sat" + latitude + "_" + longitude + "_" + date + ".png");
        }


    }

    class WebCam
    { 
        public void GetWebCam()
        {
            const string WEBSERVICE_URL = "https://webcamstravel.p.mashape.com/webcams/list/continent=EU?lang=en&show=webcams:player";
            try
            {
                var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 20000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("X-Mashape-Key", "KxOo1HIQfPmshtCvdx1RNSkgQauGp1KRyVxjsnFRbmV7RLdKx0");
                    using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            var jsonResponse = sr.ReadToEnd();
                            Console.WriteLine(String.Format("Response: {0}", jsonResponse));
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //format for webcam player:  "https://api.lookr.com/embed/player/1169738193/live"
            //note for timelapse, the player takes two images and gradiants from one to the next, then picks the next jpg and repeats. This happens live in the html
            //format for server link (the player's source) http://stream.webcams.travel/1169738193
            //format for the actual source (stream's video source)  http://dlmainstreetwebcam.gondtc.com/axis-cgi/mjpg/video.cgi?resolution=704x480
            //which to use? how to programmatically get the true source?


        }
    }
}
