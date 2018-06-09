using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace CBTC
{
    //配置端口和IP地址
    public class ConfigData
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        private string filePath = Application.StartupPath + "\\IP-Port-List.ini";//获取INI文件路径
        private string sectionATP = "ATP"; //INI文件名  
        private string sectionZC = "ZC"; //INI文件名  
        private string sectionDMI = "DMI"; //INI文件名 
        private string sectionDC = "DC"; //INI文件名 
        private string sectionFault = "Fault";
        private string sectionATS = "ATS";
        private string sectionDesATS = "desATS";

        // 自定义读取INI文件中的内容方法
        private string ContentValue(string Section, string key)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, filePath);
            return temp.ToString();
        }
        public void ReadConfigData()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("未找到配置文件，VOBC1将不能运行！");
                    return;
                }
                else
                {
                    ATP.ATPIP = ContentValue(sectionATP, "IP");
                    ATP.ATPPort = ContentValue(sectionATP, "port");
                    ATP.sendID = ContentValue(sectionATP, "sendID");
                    ATP.trainID = ContentValue(sectionATP, "trainID");
                    ATP.desZCIP = ContentValue(sectionZC, "IP");
                    ATP.desZCPort = ContentValue(sectionZC, "port");
                    ATP.desDMIIP = ContentValue(sectionDMI, "IP");
                    ATP.desDMIPort = ContentValue(sectionDMI, "port");
                    ATP.desDCIP = ContentValue(sectionDC, "IP");
                    ATP.desDCPort = ContentValue(sectionDC, "port");
                    ATP.FalultIP= ContentValue(sectionFault, "IP");
                    ATP.FalultPort= ContentValue(sectionFault, "port");
                    ATP.timeATSIP = ContentValue(sectionATS, "IP");
                    ATP.timeATSPort = ContentValue(sectionATS, "port");
                    ATP.desATSIP = ContentValue(sectionDesATS, "IP");
                    ATP.desATSPort = ContentValue(sectionDesATS, "port");
                }
            }
            catch
            {
                MessageBox.Show("配置文件中有错误，请修改，并重新启动！配置文件路径为：" + filePath);
                System.Environment.Exit(0);
            }
        }
    }
}
