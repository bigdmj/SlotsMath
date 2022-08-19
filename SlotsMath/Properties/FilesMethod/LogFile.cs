using System.Globalization;
using System.IO;

namespace SlotsMath.Properties.FileMethod
{
    public static class LogFile
    {
        /// <summary>
        /// 清空log
        /// </summary>
        /// <param name="logName">日志文件名称</param>
        public static void ClearLog(string logName)
        {
            string savePath = Program.logPath + logName;
            if (File.Exists(savePath))
            {
                FileStream stream = File.Open(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                stream.Close();
            }
        }

        public static void SaveLog(string logName,string logTxt)
        {
            string savePath = Program.logPath + logName;
            if (File.Exists(savePath))
            {
                
                StreamWriter sw = File.AppendText(savePath);
                sw.Write("\n"+logTxt);
                //开始写
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                sw.Dispose();
            }
            else
            {
                FileStream fs = new FileStream(savePath,FileMode.Create);
                StreamWriter sw = File.AppendText(savePath);
                sw.Write(logTxt);
                //开始写
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                sw.Dispose();
                fs.Close();
            }
            
        }
    }
}