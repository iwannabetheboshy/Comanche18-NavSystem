using NaviSystem.Data;
using NaviSystem.DeviceControl;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO.Ports;

namespace NaviSystem.DeviceCommunication.Test
{
    [TestFixture]
    class DeviceControlTest
    {
        IDeviceControl _diviceController;

        //[SetUp]
        //public void Init()
        //{
        //    _diviceController = null;
        //}

        [Test]
        public void GetSerialPortsTest()
        {

            Assert.That(_diviceController.GetSerialPorts() == SerialPort.GetPortNames());
        }

        [Test]
        public void StartDevicesTest()
        {
            MySerialData s_date = new MySerialData()
            {
                DeviceName = "DVL",
                BaudRate = 9600,
                DataBits = 8,
                PortName = "COM1"
            };

            _diviceController = new DeviceControl.DeviceControl();

            List<MySerialData> s_list = new List<MySerialData>();
            s_list.Add(s_date);

            Assert.That(_diviceController.StartDevices(s_list)[0] == "DVL");
        }

        [Test]
        public void CancelDevicesTest()
        {
            MySerialData s_date = new MySerialData()
            {
                DeviceName = "DVL",
                BaudRate = 9600,
                DataBits = 8,
                PortName = "COM1"
            };

            _diviceController = new DeviceControl.DeviceControl();

            List<MySerialData> s_list = new List<MySerialData>();
            s_list.Add(s_date);

            _diviceController.StartDevices(s_list);

            Assert.That(_diviceController.CancelDevices(s_list)[0] == "DVL");
        }

        [Test]
        public void IsActiveTest()
        {
            MySerialData s_date = new MySerialData()
            {
                DeviceName = "DVL",
                BaudRate = 9600,
                DataBits = 8,
                PortName = "COM1"
            };

            _diviceController = null;

            List<MySerialData> s_list = new List<MySerialData>();
            s_list.Add(s_date);

            _diviceController.StartDevices(s_list);

            Assert.That(_diviceController.IsActive(s_date));

        }
    }
}
