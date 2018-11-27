using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ConfigData
{
    class IPConfigure
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
          string val, string filePath);

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        public IPConfigure()
        {
            foreach (string Device in Section)
            {
                IPList IP = new IPList();
                IP.DeviceName = Device;               
                IP.Port = Convert.ToInt16(ReadIniData(Device, "Port", "", IPConfigPath));
                IP.IP = IPAddress.Parse(ReadIniData(Device, "IP", "", IPConfigPath));
                IP.ATPPort = Convert.ToInt16(ReadIniData(Device, "ATPPort", "", IPConfigPath));
                IP.ATPIP = IPAddress.Parse(ReadIniData(Device, "ATPIP", "", IPConfigPath));
                IP.sendID = Convert.ToInt16(ReadIniData(Device, "sendID", "", IPConfigPath));
                IP.trainID = Convert.ToInt16(ReadIniData(Device, "trainID", "", IPConfigPath));
                IPList.Add(IP);
            }
        }

        public static IPList FindIpList(string deviceName)
        {
            return IPList.Find(item => { return item.DeviceName == deviceName; });
        }

        public static List<IPList> IPList = new List<IPList>();
        StringBuilder temp = new StringBuilder(1024);
        string IPConfigPath = System.AppDomain.CurrentDomain.BaseDirectory + "IP-PortList.ini";
        string[] Section = { "ZC", "DMI", "DC", "Fault", "ATS", "desATS", "desATS", "ATPCurve"};
        string[] Key = { "Port", "IP", "ATPIP", "ATPPort", "sendID", "trainID" };

        #region 读Ini文件
        public string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);

                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion
    }
    
    class IPList
    {
        public string DeviceName;
        public int Port;
        public IPAddress IP;
        public int ATPPort;
        public IPAddress ATPIP;
        public int sendID;
        public int trainID;
    }
}
