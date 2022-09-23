using System;
using System.Data;
using System.Text;
using Newtonsoft.Json;

namespace SlotsMath.Properties.FileMethod
{
    public static class DataTableMethod
    {
        /// <summary>
        /// Datatable生成Excel表格并返回路径
        /// </summary>
        /// <param name="m_DataTable">Datatable</param>
        /// <param name="s_FileName">文件名</param>
        /// <returns></returns>
        public static string DataToExcel(System.Data.DataTable m_DataTable, string s_FileName)
        {
            string FileName = Program.ExcelSavePath + s_FileName + ".xlsx";  //文件存放路径
            if (System.IO.File.Exists(FileName))                                //存在则删除
            {
                System.IO.File.Delete(FileName);
            }
            System.IO.FileStream objFileStream;
            System.IO.StreamWriter objStreamWriter;
            string strLine = "";
            objFileStream = new System.IO.FileStream(FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            objStreamWriter = new System.IO.StreamWriter(objFileStream, Encoding.Unicode);
            for (int i = 0; i < m_DataTable.Columns.Count; i++)
            {
                strLine = strLine + m_DataTable.Columns[i].Caption.ToString() + Convert.ToChar(9);      //写列标题
            }
            objStreamWriter.WriteLine(strLine);
            strLine = "";
            for (int i = 0; i < m_DataTable.Rows.Count; i++)
            {
                for (int j = 0; j < m_DataTable.Columns.Count; j++)
                {
                    if (m_DataTable.Rows[i].ItemArray[j] == null)
                        strLine = strLine + " " + Convert.ToChar(9);                                    //写内容
                    else
                    {
                        string rowstr = "";
                        rowstr = m_DataTable.Rows[i].ItemArray[j].ToString();
                        if (rowstr.IndexOf("\r\n") > 0)
                            rowstr = rowstr.Replace("\r\n", " ");
                        if (rowstr.IndexOf("\t") > 0)
                            rowstr = rowstr.Replace("\t", " ");
                        strLine = strLine + rowstr + Convert.ToChar(9);
                    }
                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";
            }
            objStreamWriter.Close();
            objFileStream.Close();
            return FileName;        //返回生成文件的绝对路径
        }

        
        /// <summary>
        /// 打印dataTable
        /// </summary>
        /// <param name="dataTable"></param>
        public static void PrintDataTable(DataTable dataTable)
        {
            foreach (var tableHeard in dataTable.Columns)
            {
                Console.Write(tableHeard.ToString());
                Console.Write(" ");
            }
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Console.Write("\n");
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    Console.Write(dataTable.Rows[i][j]);
                    Console.Write(" ");
                } 
            }
            Console.Write("\n");
            Console.Write("输出完毕 \n");
        }
        
        /// <summary>
        /// dataTable 转化为json格式的字符串
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// todo 这里转出来的格式不是想要的格式，需要单独写
        public static string DataTableToJsonWithJsonNet(DataTable table)
        { 
            string JsonString=string.Empty; 
            JsonString = JsonConvert.SerializeObject(table); 
            return JsonString; 
        }
        
    }
    
    
}