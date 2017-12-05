using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using NetworkIt;

namespace HyggeKinectV
{
    /// <summary>
    /// Interaction logic for GesturesPage.xaml
    /// </summary>
    public partial class MainWindow : Page
    {

        Client client = new Client("edel", "http://localhost", 8000);

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        GestureController _gestureController;
        int messageCount;

        public MainWindow()
        {
            // NetworkIt
            client.MessageReceived += Client_MessageReceived;
            
            client.Connected += Client_Connected;
            client.StartConnection();

            messageCount = 0;

            // Kinect
            
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _gestureController = new GestureController();
                _gestureController.GestureRecognized += GestureController_GestureRecognized;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Body body = frame.Bodies().Closest();

                    if (body != null)
                    {
                        _gestureController.Update(body);
                    }
                }
            }
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            string gesture = e.GestureType.ToString();
            Trace.WriteLine(gesture);

            Message msg = new Message(gesture);
            msg.DeliverToSelf = true;
            msg.AddField("count", "" + messageCount++);

            client.SendMessage(msg);
        }

        // NetworkIt

        private void Client_Connected(object sender, System.EventArgs e)
        {
            Trace.WriteLine("Connection Successful");
        }

        private void Client_MessageReceived(object sender, NetworkItMessageEventArgs e)
        {
            Trace.WriteLine(e.ReceivedMessage.ToString());
        }

        // Send message

    }
}
