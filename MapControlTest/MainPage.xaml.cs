using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Globalization;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MapControlTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BasicGeoposition carPos;
        private MapIcon icon;
        private int updateCount;
        private DispatcherTimer updateTimer;
        private bool trackingEnabled;
        private RandomAccessStreamReference img_0;
        private RandomAccessStreamReference img_24;
        private RandomAccessStreamReference img_48;
        private RandomAccessStreamReference img_72;
        private RandomAccessStreamReference img_96;
        private RandomAccessStreamReference img_120;
        private RandomAccessStreamReference img_144;
        private RandomAccessStreamReference img_168;
        private RandomAccessStreamReference img_192;
        private RandomAccessStreamReference img_216;
        private RandomAccessStreamReference img_240;
        private RandomAccessStreamReference img_264;
        private RandomAccessStreamReference img_288;
        private RandomAccessStreamReference img_312;
        private RandomAccessStreamReference img_336;
        private RandomAccessStreamReference img_stop;

        public MainPage()
        {
            this.InitializeComponent();
            carPos = new BasicGeoposition();
            icon = new MapIcon();
            trackingEnabled = false;
            StartStopButton.Content = "Start";
        }

        /// <summary>
        /// Initializes most members on once Map gets loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize center latitude and longitude some location
            carPos.Latitude = 9.610135;
            carPos.Longitude = 76.680732;

            // Load the image resources
            img_0 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg0.png"));
            img_24 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg24.png"));
            img_48 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg48.png"));
            img_72 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg72.png"));
            img_96 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg96.png"));
            img_120 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg120.png"));
            img_144 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg144.png"));
            img_168 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg168.png"));
            img_192 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg192.png"));
            img_216 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg216.png"));
            img_240 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg240.png"));
            img_264 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg264.png"));
            img_288 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg288.png"));
            img_312 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg312.png"));
            img_336 = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/hdg336.png"));
            img_stop = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/stopped.png"));

            myMap.Center = new Geopoint(carPos);
            myMap.ZoomLevel = 15;

            icon.Image = img_stop;
            icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
            icon.Title = "";
            icon.Visible = false;
            myMap.MapElements.Add(icon);

            // Start the timer
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 1);
            updateTimer.Tick += UpdateTimer_Tick;
            updateCount = 0;
            errorBox.Text = "Initialized";
            updateTimer.Start();
        }

        /// <summary>
        /// Timer event handler which updates the location details by fetching data from server
        /// This will trigger at regular intervals (5 sec)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateTimer_Tick(object sender, object e)
        {
            // Dont retrieve data from server if tracking disabled
            if(trackingEnabled == false)
            {
                return;
            }

            try
            {
                // Stop timer for now
                updateTimer.Stop();

                // Get the location data from server
                RootObject carLocation = await GetLocationData();

                // Update the location in Map
                UpdateLocation(carLocation.gps_data);
                updateCount += 1;
                errorBox.Text = string.Format("Updated {0} times", updateCount);
            }
            catch(Exception ex)
            {
                errorBox.Text = "Exception: " + ex.Message;
            }
            // Start timer again
            updateTimer.Interval = new TimeSpan(0, 0, 5);
            updateTimer.Start();

        }
        
        // Update the Map Icon to match with the given gps location data 
        private void UpdateLocation(GpsData gpsData)
        {
            DateTime currentTime = DateTime.Now;
            DateTime updatedTime = StringToDate(gpsData.date, gpsData.time);
            TimeSpan timeDifference = currentTime - updatedTime;
            string label = "";
            if (updatedTime.Year == 0)
            {
                label = "Unknown time";
            }
            else
            {
                if(timeDifference.Days > 0)
                {
                    label = string.Format("{0}d ago", timeDifference.Days);
                }
                else if(timeDifference.Hours > 0)
                {
                    label = string.Format("{0}h ago", timeDifference.Hours);
                }
                else if(timeDifference.Minutes > 0)
                {
                    label = string.Format("{0}m ago", timeDifference.Minutes);
                }
                else if(timeDifference.Seconds >= 0)
                {
                    label = string.Format("{0}s ago", timeDifference.Seconds);
                }
                else
                {
                    // timeDifference is negative value (can't be negative?)
                    label = "Now";
                }
            }
            carPos.Latitude = gpsData.lat;
            carPos.Longitude = gpsData.lon;
            myMap.Center = new Geopoint(carPos);
            icon.Location = myMap.Center;
            if (gpsData.speed < 5)
            {
                icon.Image = img_stop;
                icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
            }
            else 
            {
                label +=
                string.Format("\n{0}kph", gpsData.speed);
                //string.Format("\nHeading: {0}°", gpsData.dir);

                // Change icon image based on heading
                if ((gpsData.dir > 348) && (gpsData.dir <= 12))
                {
                    icon.Image = img_0;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.106);
                }
                else if ((gpsData.dir > 12) && (gpsData.dir <= 36))
                {
                    icon.Image = img_24;
                    icon.NormalizedAnchorPoint = new Point(0.865, 0.1);
                }
                else if ((gpsData.dir > 36) && (gpsData.dir <= 60))
                {
                    icon.Image = img_48;
                    icon.NormalizedAnchorPoint = new Point(0.893, 0.11);
                }
                else if ((gpsData.dir > 60) && (gpsData.dir <= 84))
                {
                    icon.Image = img_72;
                    icon.NormalizedAnchorPoint = new Point(0.9, 0.147);
                }
                else if ((gpsData.dir > 84) && (gpsData.dir <= 108))
                {
                    icon.Image = img_96;
                    icon.NormalizedAnchorPoint = new Point(0.9, 0.7);
                }
                else if ((gpsData.dir > 108) && (gpsData.dir <= 132))
                {
                    icon.Image = img_120;
                    icon.NormalizedAnchorPoint = new Point(0.9, 0.875);
                }
                else if ((gpsData.dir > 132) && (gpsData.dir <= 156))
                {
                    icon.Image = img_144;
                    icon.NormalizedAnchorPoint = new Point(0.883, 0.9);
                }
                else if ((gpsData.dir > 156) && (gpsData.dir <= 180))
                {
                    icon.Image = img_168;
                    icon.NormalizedAnchorPoint = new Point(0.78, 0.9);
                }
                else if ((gpsData.dir > 180) && (gpsData.dir <= 204))
                {
                    icon.Image = img_192;
                    icon.NormalizedAnchorPoint = new Point(0.22, 0.9);
                }
                else if ((gpsData.dir > 204) && (gpsData.dir <= 228))
                {
                    icon.Image = img_216;
                    icon.NormalizedAnchorPoint = new Point(0.117, 0.9);
                }
                else if ((gpsData.dir > 228) && (gpsData.dir <= 252))
                {
                    icon.Image = img_240;
                    icon.NormalizedAnchorPoint = new Point(0.1, 0.875);
                }
                else if ((gpsData.dir > 252) && (gpsData.dir <= 276))
                {
                    icon.Image = img_264;
                    icon.NormalizedAnchorPoint = new Point(0.1, 0.7);
                }
                else if ((gpsData.dir > 276) && (gpsData.dir <= 300))
                {
                    icon.Image = img_288;
                    icon.NormalizedAnchorPoint = new Point(0.1, 0.147);
                }
                else if ((gpsData.dir > 300) && (gpsData.dir <= 324))
                {
                    icon.Image = img_312;
                    icon.NormalizedAnchorPoint = new Point(0.107, 0.11);
                }
                else // ((gpsData.dir > 324) && (gpsData.dir <= 348))
                {
                    icon.Image = img_336;
                    icon.NormalizedAnchorPoint = new Point(0.135, 0.1);
                }
            }
            icon.Title = label;
            icon.Visible = true;
        }

        // Get the location data from server, parse and return the object containing the data
        private async Task<RootObject> GetLocationData()
        {
            try
            {
                //string jsonText = await GetWebPageStringAsync("http://embeddedworld.co.nf/get_last_loc_json.php");
                string jsonText = await GetWebPageStringAsync("http://positioning.hol.es/get_last_loc_json.php");
                var serializer = new DataContractJsonSerializer(typeof(RootObject));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonText));
                var data = (RootObject)serializer.ReadObject(ms);
                return data;
            }
            catch (Exception e)
            {
                throw new Exception("Could not get location data: ", e);
            }
        }

        // Fetch the given Web URL and return the content as string
        private async Task<string> GetWebPageStringAsync(string pageUrl)
        {
            try
            {
                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                HttpClient client = new HttpClient(filter);
                Uri uri = new Uri(pageUrl);

                // disable cache
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

                // GET the URI as string, asynchronously
                string responseString = await client.GetStringAsync(uri);
                responseString = responseString.Replace("<br>", Environment.NewLine);
                return responseString;
            }
            catch (Exception e)
            {
                throw new Exception("Network error", e);
            }

        }

        private DateTime StringToDate(string date, string time)
        {
            if((date != "") && (time != ""))
            {
                return DateTime.ParseExact(date + "T" + time, "s", CultureInfo.InvariantCulture);
            }

            return new DateTime(0, 0, 0, 0, 0, 0);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (trackingEnabled == false) {
                // Start tracking
                trackingEnabled = true;
                StartStopButton.Content = "Stop";
                errorBox.Text = "Started Logging";
            }
            else
            {
                // Stop tracking
                trackingEnabled = false;
                StartStopButton.Content = "Start";
                errorBox.Text = "Stopped Logging";
            }
        }
    }

    [DataContract]
    public class GpsData
    {
        [DataMember]
        public double lat { get; set; }
        [DataMember]
        public double lon { get; set; }
        [DataMember]
        public int speed { get; set; }
        [DataMember]
        public int dir { get; set; }
        [DataMember]
        public int hdop { get; set; }
        [DataMember]
        public string time { get; set; }
        [DataMember]
        public string date { get; set; }
    }

    [DataContract]
    public class Status
    {
        [DataMember]
        public int error { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public GpsData gps_data { get; set; }
        [DataMember]
        public Status status { get; set; }
    }
}
