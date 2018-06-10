using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class SocketMessage
    {
        public static bool isEB;
        public void SaveLogs(string content)    //打印日志
        {    
              string path = "Log\\" + DateTime.Now.Year + " " + DateTime.Now.Month + "." + DateTime.Now.Day + ".txt";
              File.AppendAllText(path, content);
        }

        public void IsEB(string EBReason)     //利用这个方法EB
        {
            isEB = true;
            SaveLogs(EBReason);
        }


    }
}
