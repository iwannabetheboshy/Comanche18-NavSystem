using NaviSystem.Date;
using System.Collections.Generic;


namespace NaviSystem.DeviceCommunication
{
    public interface IDeviceCommunication
    {
        List<ISerialPortCom> GetSPortComList(List<DeviceSerialDate> deviceSDate);
    }
    class DeviceCommunication
    {
    }
}
