using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTC
{
    class EB
    {

        public void Set_EB(bool isEB, string reason)
        {
            isEB = true;
            WriteLog(reason);
        }

        public static void WriteLog(string strLog)
        {
            string sFilePath = ".\\" + DateTime.Now.ToString("yyyyMM")+"\\"+"log.txt";

            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFilePath))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFilePath, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFilePath, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "   ---   " + strLog);
            sw.Close();
            fs.Close();
        }

    }
}
