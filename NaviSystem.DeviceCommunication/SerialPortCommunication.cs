using System.IO.Ports;


namespace NaviSystem.DeviceCommunication
{
    public interface ISerialPortCom
    {
        SerialPort GetSerialPort();
        bool StartActions();
        bool StopActions();
        string GetName();
        bool IsInAction();
    }

    class SerialPortCom
    {
    }
}
