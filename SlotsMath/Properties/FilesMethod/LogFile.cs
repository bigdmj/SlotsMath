/*
 * 日志相关
 */
using System.Globalization;
using System.IO;

namespace SlotsMath.Properties.FileMethod
{
    public static class LogFile
    {
        /// <summary>
        /// 清空log文件
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

        /// <summary>
        /// 将logTxt保存到日志文件中(后续插入，如果要清空请手动clear)
        /// </summary>
        /// <param name="logName">日志文件名称</param>
        /// <param name="logTxt">文本</param>
        public static void SaveLog(string logName,string logTxt)
        {
            string savePath = Program.logPath + logName;
            if (File.Exists(savePath))
            {
                StreamWriter sw = File.AppendText(savePath);
                sw.Write(logTxt); //开始写
                sw.Flush(); //清空缓冲区
                sw.Close(); //关闭流
                sw.Dispose();
            }
            else
            {
                FileStream fs = new FileStream(savePath,FileMode.Create);
                StreamWriter sw = File.AppendText(savePath);
                sw.Write(logTxt); //开始写
                sw.Flush(); //清空缓冲区
                sw.Close(); //关闭流
                sw.Dispose();
                fs.Close();
            }
            
        }
    }
}