using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                IP.sendID = Convert.ToInt16(ReadIniData(Device, "sendID", "", IPConfigPath));
                IP.trainID = Convert.ToInt16(ReadIniData(Device, "trainID", "", IPConfigPath));
                IPList.Add(IP);
            }
        }

        public static List<IPList> IPList = new List<IPList>();
        StringBuilder temp = new StringBuilder(1024);
        string IPConfigPath = System.AppDomain.CurrentDomain.BaseDirectory + "IP-PortList.ini";
        string[] Section = { "ATP", "ZC", "DMI", "DC", "Fault", "ATS", "desATS", "ATPCurve"};
        string[] Key = { "Port", "IP", "sendID", "trainID" };

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
        public int sendID;
        public int trainID;
    }
}
