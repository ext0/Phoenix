using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;
using PhoenixLib.Datastore.CommunicationClasses;

namespace PhoenixLib.Webcam
{
    public static class Webcam
    {
        static FilterInfoCollection webcams;
        static VideoCaptureDevice device;
        public static Bitmap mostRecent;
        public static WebcamWrapper[] getDevices()
        {
            webcams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            List<FilterInfo> filterInfo = new List<FilterInfo>();
            for (int i = 0; i < webcams.Count; i++)
            {
                filterInfo.Add(webcams[i]);
            }
            return filterInfo.Select((x) => (new WebcamWrapper { name = x.MonikerString, niceName = x.Name })).ToArray();
        }
        public static void init(String deviceString)
        {
            device = new VideoCaptureDevice(deviceString);
            device.NewFrame += Device_NewFrame;
        }

        private static void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            mostRecent = (Bitmap)eventArgs.Frame.Clone();
            GC.Collect();
        }

        public static void start()
        {
            if (!device.IsRunning)
            {
                device.Start();
            }
        }
        public static void stop()
        {
            device.SignalToStop();
            if (device.IsRunning)
            {
                device.Stop();
            }
        }
    }
}
