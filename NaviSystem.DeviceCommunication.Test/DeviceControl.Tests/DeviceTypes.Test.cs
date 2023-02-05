using NaviSystem.Data;
using NaviSystem.DeviceControl;
using NUnit.Framework;
using System.Collections.Generic;

namespace NaviSystem.DeviceCommunication.Test
{
    [TestFixture]
    class DeviceTypesTest
    {
        public class TestTrackLink : TrackLink
        {
            public TestTrackLink(MySerialData s_date) : base(s_date)
            {
            }
            public BeaconData MyDataProcessing(object ob)
            {
                return base.DataProcessing(ob);
            }
        }

        [Test]
        public void TrackLinkDataProcessingTest()
        {
            MySerialData s_date = new MySerialData()
            {
                DeviceName = "TrackLink",
                BaudRate = 9600,
                DataBits = 8,
                PortName = "COM1"
            };

            TestTrackLink device = new TestTrackLink(s_date);
            string data = "19,05/04/17,14:54:31,  4250.5598,-14718.5044,  4250.5538,-14718.5110,153.7, 198.4\r";

            BeaconData result = device.MyDataProcessing(data);
            BeaconData bData = new BeaconData()
            {
                Depth = 198.4,
                Number = 19,
                Longitude = 147.185110,
                Latitude = 42.505538,
                E = true,
                N = true,
                VeselLatitude = 42.505598,
                VeselLongitude = 147.185044,
                VesselHeading = 153.7
            };


            Assert.AreEqual(bData.ToString(), result.ToString());

        }

        [Test]
        public void TrackLinkSetActionListTest()
        {
            MySerialData s_date = new MySerialData()
            {
                DeviceName = "TrackLink",
                BaudRate = 9600,
                DataBits = 8,
                PortName = "COM1"
            };

            TrackLink device = new TrackLink(s_date);

            ActionInfo ex1 = new ActionInfo("DataUpdate", null);
            ActionInfo ex2 = new ActionInfo("Read", null);
            List<ActionInfo> expectedList = new List<ActionInfo>();
            expectedList.Add(ex1);
            expectedList.Add(ex2);
            Assert.AreEqual(expectedList[0].ToString()+ expectedList[1].ToString(), 
                device.GetActions()[0].ToString()+ device.GetActions()[1].ToString());
        }
    }
}
