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
using static AppConfig;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MapControlTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BasicGeoposition carPosCurrent;
        private MapIcon carIconCurrent;
        private int updateCount;
        private DispatcherTimer updateTimer;
        private bool trackingEnabled;
        private bool loggingEnabled;
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
        private RandomAccessStreamReference img_stopped;
        private RandomAccessStreamReference img_start;
        private RandomAccessStreamReference img_finish;
        private RandomAccessStreamReference img_dot;
        private List<MapIcon> iconSet;
        private List<LocationObject> locationSet;
        private List<MapPolyline> pathOutline;

        public MainPage()
        {
            this.InitializeComponent();
            carPosCurrent = new BasicGeoposition();
            carIconCurrent = new MapIcon();
            iconSet = new List<MapIcon>();
            pathOutline = new List<MapPolyline>();
            trackingEnabled = false;
            StartStopButton.Content = "Start";
        }

        /// <summary>
        /// Initializes most of the members once Map gets loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize center latitude and longitude some location
            carPosCurrent.Latitude = 9.610135;
            carPosCurrent.Longitude = 76.680732;

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
            img_stopped = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/stopped.png"));
            img_start = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/start.png"));
            img_finish = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/finish.png"));
            img_dot = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/dot.png"));
            myMap.Center = new Geopoint(carPosCurrent);
            myMap.ZoomLevel = 15;
            carIconCurrent.Image = img_stopped;
            carIconCurrent.NormalizedAnchorPoint = new Point(0.5, 0.5);
            carIconCurrent.Title = "";
            carIconCurrent.Visible = false;
            myMap.MapElements.Add(carIconCurrent);

            // Check current status of logging
            try
            {
                string logStatus = await LoggingStatus();
                if (logStatus != "Error")
                {
                    if (logStatus == "Enabled")
                    {
                        loggingEnabled = true;
                        LoggingButton.Content = "Disable";
                    }
                    else if (logStatus == "Disabled")
                    {
                        loggingEnabled = false;
                        LoggingButton.Content = "Enable";
                    }
                    LoggingButton.IsEnabled = true;
                }
            }
            catch(Exception ex)
            {
                errorBox.Text = "Exception: " + ex.Message;
            }

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
        /// This will trigger at regular intervals (5 sec).
        /// Updating is enabled/disabled by Start/Stop button
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
                LocationObject carLocation = await GetLocationData();

                // Update the location in Map
                UpdateLocation(carLocation.gps_data);
                updateCount += 1;
                errorBox.Text = string.Format("Updated {0} times", updateCount);
                if(carLocation.status.error > 0)
                {
                    errorBox.Text += "\nError: " + ((ErrorStatus)carLocation.status.error).ToString();
                }

                // Start timer again
                updateTimer.Interval = new TimeSpan(0, 0, 5);
                updateTimer.Start();
            }
            catch(Exception ex)
            {
                errorBox.Text = "Exception: " + ex.Message;
            }

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
            carPosCurrent.Latitude = gpsData.lat;
            carPosCurrent.Longitude = gpsData.lon;
            SetMapIcon(carIconCurrent, gpsData);
            myMap.Center = new Geopoint(carPosCurrent);
            
            if (gpsData.speed >= 5)
            {
                label += string.Format("\n{0} kph", gpsData.speed);
            }
            carIconCurrent.Title = label;
            carIconCurrent.Visible = true;
        }

        /// <summary>
        /// Set properties for a MapIcon, specifically its location and image
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="gpsData"></param>
        private void SetMapIcon(MapIcon icon, GpsData gpsData)
        {
            if (gpsData.speed < 5)
            {
                icon.Image = img_stopped;
                icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
            }
            else
            {
                // Change icon image based on heading
                if ((gpsData.dir > 348) && (gpsData.dir <= 12))
                {
                    icon.Image = img_0;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.66);
                }
                else if ((gpsData.dir > 12) && (gpsData.dir <= 36))
                {
                    icon.Image = img_24;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.62);
                }
                else if ((gpsData.dir > 36) && (gpsData.dir <= 60))
                {
                    icon.Image = img_48;
                    icon.NormalizedAnchorPoint = new Point(0.4, 0.55);
                }
                else if ((gpsData.dir > 60) && (gpsData.dir <= 84))
                {
                    icon.Image = img_72;
                    icon.NormalizedAnchorPoint = new Point(0.34, 0.5);
                }
                else if ((gpsData.dir > 84) && (gpsData.dir <= 108))
                {
                    icon.Image = img_96;
                    icon.NormalizedAnchorPoint = new Point(0.33, 0.5);
                }
                else if ((gpsData.dir > 108) && (gpsData.dir <= 132))
                {
                    icon.Image = img_120;
                    icon.NormalizedAnchorPoint = new Point(0.37, 0.5);
                }
                else if ((gpsData.dir > 132) && (gpsData.dir <= 156))
                {
                    icon.Image = img_144;
                    icon.NormalizedAnchorPoint = new Point(0.47, 0.38);
                }
                else if ((gpsData.dir > 156) && (gpsData.dir <= 180))
                {
                    icon.Image = img_168;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.33);
                }
                else if ((gpsData.dir > 180) && (gpsData.dir <= 204))
                {
                    icon.Image = img_192;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.33);
                }
                else if ((gpsData.dir > 204) && (gpsData.dir <= 228))
                {
                    icon.Image = img_216;
                    icon.NormalizedAnchorPoint = new Point(0.58, 0.38);
                }
                else if ((gpsData.dir > 228) && (gpsData.dir <= 252))
                {
                    icon.Image = img_240;
                    icon.NormalizedAnchorPoint = new Point(0.65, 0.5);
                }
                else if ((gpsData.dir > 252) && (gpsData.dir <= 276))
                {
                    icon.Image = img_264;
                    icon.NormalizedAnchorPoint = new Point(0.66, 0.5);
                }
                else if ((gpsData.dir > 276) && (gpsData.dir <= 300))
                {
                    icon.Image = img_288;
                    icon.NormalizedAnchorPoint = new Point(0.65, 0.5);
                }
                else if ((gpsData.dir > 300) && (gpsData.dir <= 324))
                {
                    icon.Image = img_312;
                    icon.NormalizedAnchorPoint = new Point(0.6, 0.6);
                }
                else // ((gpsData.dir > 324) && (gpsData.dir <= 348))
                {
                    icon.Image = img_336;
                    icon.NormalizedAnchorPoint = new Point(0.5, 0.66);
                }
            }
            icon.Location =
                new Geopoint(new BasicGeoposition()
                {
                    Latitude = gpsData.lat,
                    Longitude = gpsData.lon,
                });
        }

        // Get the location data from server, parse and return the object containing the data
        private async Task<LocationObject> GetLocationData()
        {
            try
            {
                string jsonText = await GetWebPageStringAsync(locationApiUrl);
                var data = DeserializeJson<LocationObject>(jsonText);
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

        // Convert a date-time string to DateTime object
        private DateTime StringToDate(string date, string time)
        {
            if((date != "") && (time != ""))
            {
                return DateTime.ParseExact(date + "T" + time, "s", CultureInfo.InvariantCulture);
            }

            return new DateTime(0, 0, 0, 0, 0, 0);
        }

        // Handler for Start/Stop button - starts or stops fetching updates
        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (trackingEnabled == false) {
                // Start tracking
                trackingEnabled = true;
                StartStopButton.Content = "Stop";
                errorBox.Text = "Started updating...";
            }
            else
            {
                // Stop tracking
                trackingEnabled = false;
                StartStopButton.Content = "Start";
                errorBox.Text = "Stopped updating...";
            }
        }

        // Handler for Logging Enable/Disable button - disables logging in the server if enabled and vice versa
        private async void LoggingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggingEnabled == true)
                {
                    // Currently logging is enabled - Disable it
                    string logStatus = await LoggingStatus(false);
                    if (logStatus == "Disabled")
                    {
                        loggingEnabled = false;
                        LoggingButton.Content = "Enable";
                    }
                    else
                    {
                        errorBox.Text += "\nLogging could not be disabled";
                    }
                }
                else
                {
                    // Currently logging is disabled - enable it
                    string logStatus = await LoggingStatus(true);
                    if (logStatus == "Enabled")
                    {
                        loggingEnabled = true;
                        LoggingButton.Content = "Disable";
                    }
                    else
                    {
                        errorBox.Text += "Logging could not be enabled";
                    }
                }
            }
            catch (Exception log_ex)
            {
                errorBox.Text = "Exception: " + log_ex.Message;
            }
        }

        // Gets current status of logging from server
        private async Task<string> LoggingStatus()
        {
            try
            {
                string logStatus = await GetWebPageStringAsync(setLogStatusUrl);
                if((logStatus == "Enabled") || (logStatus == "Disabled"))
                {
                    return logStatus;
                }
                else
                {
                    return "Error";
                }
            }
            catch(Exception e)
            {
                throw new Exception("Could not get logging status:", e);
            }
        }

        // Enable/Disable logging in the server
        private async Task<string> LoggingStatus(bool enable)
        {
            try
            {
                string logStatus = await GetWebPageStringAsync(AppConfig.setLogStatusUrl + "?enable=" + enable.ToString());
                if ((logStatus == "Enabled") || (logStatus == "Disabled"))
                {
                    return logStatus;
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not get logging status:", e);
            }
        }

        // Get a set of location samples corresponding to the query provided
        private async Task<List<LocationObject>> GetLocationSet(int lastCount)
        {
            if(lastCount <= 0)
            {
                lastCount = 1;
            }
            string jsonText = await GetWebPageStringAsync(locationSetApiUrl + string.Format("?last={0}", lastCount));
            //errorBox.Text = jsonText;
            var obj = DeserializeJson<List<LocationObject>>(jsonText);
            return obj;
        }

        // Deserialize a JSON string to corresponding object type
        private T DeserializeJson<T> (string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }
         
        private async void TestPrev_Click(object sender, RoutedEventArgs e)
        {
            int prevCount;
            if (PrevCount.Text != null)
            {
                prevCount = Convert.ToInt32(PrevCount.Text);
                if (prevCount > 100)
                    prevCount = 100;
            }
            else
            {
                prevCount = 3;
            }

            locationSet = await GetLocationSet(prevCount);
            DrawIconSet(locationSet);
            DrawPath(locationSet);
            myMap.Center = iconSet[0].Location;
        }

        private void DrawPath(List<LocationObject> locationSet)
        {
            int count = 0;
            if (pathOutline.Count > 0)
            {
                foreach(MapPolyline line in pathOutline)
                {
                    myMap.MapElements.Remove(line);
                }
                pathOutline.Clear();
            }
            count = locationSet.Count;
            while(count > 1)
            {
                MapPolyline line = new MapPolyline();
                line.Path = new Geopath(new List<BasicGeoposition>()
                {
                    new BasicGeoposition() { Latitude = locationSet[count-1].gps_data.lat, Longitude = locationSet[count-1].gps_data.lon },
                    new BasicGeoposition() { Latitude = locationSet[count-2].gps_data.lat, Longitude = locationSet[count-2].gps_data.lon }
                });
                line.StrokeDashed = true;
                line.StrokeThickness = 3;
                line.StrokeColor = Windows.UI.Colors.Red;
                line.ZIndex = 1;
                pathOutline.Add(line);
                myMap.MapElements.Add(line);
                count--;
            }
        }

        private void DrawIconSet(List<LocationObject> locationSet)
        {
            if (iconSet.Count > 0)
            {
                foreach (MapIcon icon in iconSet)
                {
                    myMap.MapElements.Remove(icon);
                }
                iconSet.Clear();
            }
            foreach (LocationObject location in locationSet)
            {
                //errorBox.Text += string.Format("{0}\n", location.gps_data.time);
                MapIcon icon = new MapIcon();
                //SetMapIcon(icon, location.gps_data);  // shows the detailed map icon

                SetPathIcon(icon, location);
                
                iconSet.Add(icon);
                myMap.MapElements.Add(icon);
            }
        }

        /// <summary>
        /// Set image and location properties for a MapIcon to be included in a path
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="location"></param>
        private void SetPathIcon(MapIcon icon, LocationObject location)
        {
            if (locationSet.IndexOf(location) == 0) // starting location
            {
                icon.Image = img_finish;
                icon.ZIndex = 3;
            }
            else if (locationSet.IndexOf(location) == (locationSet.Count - 1))  // ending location
            {
                icon.Image = img_start;
                icon.ZIndex = 3;
            }
            else
            {
                // just show a dot icon
                icon.Image = img_dot;
                icon.ZIndex = 2;
            }
            icon.Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = location.gps_data.lat,
                Longitude = location.gps_data.lon
            });
            icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
        }

        private void myMap_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            MapIcon clicked_icon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
            int index = iconSet.IndexOf(clicked_icon);

            if (index >= 0)
            {

                if (clicked_icon.Title == "")   // Show detailed MapIcon
                {
                    GpsData gpsData = locationSet[index].gps_data;
                    SetMapIcon(clicked_icon, gpsData);
                    DateTime time = StringToDate(gpsData.date, gpsData.time);
                    string title = "";
                    if (clicked_icon.Image != img_stopped)
                    {
                         title = string.Format("{0} kph", gpsData.speed);
                    }
                    title += string.Format("\n{0:T}", time);
                    /*
                    if (time.Year == DateTime.Now.Year)
                    {
                        title += string.Format("\n{0:ddd, MMM d}", time);
                    }
                    else
                    {
                        title += string.Format("\n{0:D}", time);
                    }
                    */
                    clicked_icon.Title = title;
                    clicked_icon.ZIndex = 3;
                }
                else  // Show normal path icon
                {
                    clicked_icon.Title = "";
                    SetPathIcon(clicked_icon, locationSet[index]);
                }
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
    public class LocationObject
    {
        [DataMember]
        public GpsData gps_data { get; set; }
        [DataMember]
        public Status status { get; set; }
    }

    public enum ErrorStatus
    {
        NoError = 0,
        SqlQueryError = 1,
        GpsNoCommunication = 2,
        GpsNoProgress = 3
    }


}
