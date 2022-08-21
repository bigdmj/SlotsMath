/*
 * 用到的一些工具脚本,比如说用窗口抛出异常信息，获取列表中特定元素的数量，把dic和list转化为string方便打印
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SlotsMath.Properties
{
    /// <summary>
    /// 公共方法，包括抛出错误，json文件转Jobject
    /// </summary>
    public static class SlotsTools
    {
        /// <summary>
        /// json文件转Jobject
        /// </summary>
        /// <param name="jsonPath">json文件目录</param>
        /// <returns>转后的Jobject对象</returns>
        /// <exception cref="TempIsZeroException"></exception>
        public static JObject ReadJson(string jsonPath)
        {
            try
            {
                StreamReader file = File.OpenText(jsonPath);
                JsonTextReader reader = new JsonTextReader(file);
                JObject jsonObject = (JObject) JToken.ReadFrom(reader);
                file.Close();
                return jsonObject;
            }
            catch
            {
                throw (new TempIsZeroException("读取Json失败"));
            }
        }
        
        /// <summary>
        /// 抛出异常(弹窗)
        /// </summary>
        public class TempIsZeroException : ApplicationException
        {
            public TempIsZeroException(string message) : base(message)
            {
            }
        }

        /// <summary>
        /// 把字典全部转化为string
        /// </summary>
        /// <param name="dict">字典的key和value必须可以转化为string</param>
        /// <returns></returns>
        public static string DictToString<T>(Dictionary<T, T> dict)
        {
            string outString = "";
            foreach (var key in dict.Keys)
            {
                outString += "key:" + key.ToString()+",Value:"+dict[key].ToString()+";\n";
            }
            return outString;
        }

        /// <summary>
        /// 把list转化为string
        /// </summary>
        /// <param name="list">list中的元素必须能转化为string</param>
        /// <returns></returns>
        public static string ListToString<T> (List<T> list)
        {
            string outString = "[";
            foreach (var VARIABLE in list)
            {
                outString += VARIABLE.ToString() + ",";
            }
            outString = outString.Substring(0, outString.Length - 1);
            outString += "]";
            return outString;
        }

        /// <summary>
        /// 获取列表中特定元素的数量
        /// </summary>
        /// <param name="inValueList">查询对象列表</param>
        /// <param name="t">查询值</param>
        /// <typeparam name="T">任何类型</typeparam>
        /// <returns></returns>
        public static int GetSymbolCountInList<T>(List<T> inValueList, T t)
        {
            int elementCount = 0;
            foreach (T value in inValueList)
            {
                if (value.Equals(t))
                {
                    elementCount++;
                }
            }
            return elementCount;
        }

        /// <summary>
        /// 从元素矩阵中获取特定元素的数量
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="symbolId">元素id</param>
        /// <returns>int</returns>
        public static int GetSymbolCountByArray(List<List<int>> symbolArray, int symbolId)
        {
            int outInt = 0;
            List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量
            //获取每一列特定元素数量
            foreach (List<int> i in symbolArray)
            {
                int tempElementCount = SlotsTools.GetSymbolCountInList(i, symbolId);
                symbolCountInArray.Add(tempElementCount);
            }

            foreach (int i in symbolCountInArray)
            {
                outInt += i;
            }

            return outInt;
        }
        
        /// <summary>
        /// 从元素矩阵中获取特定多个元素的数量
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="symbolId">元素id</param>
        /// <returns>int</returns>
        public static int GetSymbolsCountByArray(List<List<int>> symbolArray, List<int> symbolIds)
        {
            int outInt = 0;
            List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量
            //获取每一列特定元素数量
            foreach (int symbolId in symbolIds)
            {
                symbolCountInArray.Clear();
                foreach (List<int> i in symbolArray)
                {
                    int tempElementCount = GetSymbolCountInList(i, symbolId);
                    symbolCountInArray.Add(tempElementCount);
                }//将特定元素在数组中的数量记录到列表中（逐列）
                foreach (int i in symbolCountInArray)
                {
                    outInt += i;
                } //将值累加到输出值中
            }
            return outInt;
        }

        
        /// <summary>
        /// 输出中奖线上的元素列表
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="lineList">一条中奖线配置</param>
        /// <returns></returns>
        /// <exception cref="TempIsZeroException"></exception>
        public static List<int> GetSymbolListWithLine(List<List<int>> symbolArray,List<int> lineList)
        {
            List<int> outList = new List<int>();
            if (symbolArray.Count == lineList.Count)
            {
                for (int i = 0; i < symbolArray.Count; i++)
                {
                    outList.Add(symbolArray[i][lineList[i]]);
                }
            }
            else
            {
                throw (new TempIsZeroException("the symbolArray count is not like lineList count"));
            }

            return outList;
        }

        /// <summary>
        /// 根据权重随机，未完成
        /// todo
        /// </summary>
        /// <param name="randomObjectList">需要随机的对象</param>
        /// <param name="randomWeight">权重列表</param>
        /// <typeparam name="T">任何类型</typeparam>
        /// <returns></returns>
        public static T Random<T>(List<T> randomObjectList,List<double> randomWeightList)
        {
            return randomObjectList[0];
        }
        
        /// <summary>
        /// 深克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T DepthClone<T>(T t)
        {
            T clone = default(T);
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, t);
                    stream.Seek(0, SeekOrigin.Begin);
                    clone = (T) formatter.Deserialize(stream);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
            }

            return clone;
        }
    }
    
    /// <summary>
    /// 判断文件状态
    /// </summary>
    public class FileStaues
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite); //IntPtr相当于long类型

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int OF_WRITE = 2;

        private const int OF_SHARE_DENY_NONE = 0x40;

        private static readonly IntPtr HFILE_ERROR = new IntPtr(964);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileFullName">文件完整路径</param>
        /// <returns>文件不存在返回-1，文件已打开返回1，文件未打开返回0</returns>
        public static int FileIsOpen(string fileFullName)
        {
            if (!File.Exists(fileFullName))
            {
                //不存在文件
                return -1;
            }

            //handle的值始终不是-1，这样就无法返回1，也就不能判定文件已经打开，就是这个地方有问题
            IntPtr handle = _lopen(fileFullName, OF_WRITE | OF_SHARE_DENY_NONE);

            if (handle == HFILE_ERROR)
            {
                //文件已经打开
                return 1;
            }

            CloseHandle(handle);
            return 0;
        }
    }
    
    /// <summary>
    /// EXCEL处理类
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// Excel 转换为 Datatable
        /// </summary>
        /// <param name="sheetNum">工作表索引</param>
        /// <param name="isFirstRowColumn">首行为列</param>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable ExcelToDataTable(int sheetNum, bool isFirstRowColumn, string fileName)
        {
            IWorkbook workbook = null;
            ISheet sheet = null;
            DataTable myTable = new DataTable();
//                try
//                {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (fileName.IndexOf(".xlsx") > 0)
                workbook = new XSSFWorkbook(fs);
            else if (fileName.IndexOf(".xls") > 0)
                workbook = new HSSFWorkbook(fs);
            sheet = workbook.GetSheetAt(sheetNum);

            //工作表不能为空
            if (sheet == null)
            {
                string str = "";
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    str += workbook.GetSheetAt(i).SheetName + ",";
                }

                str = workbook.NumberOfSheets + str;
                throw new Exception($"sheet不能为空！参数：{sheetNum} 工作簿信息：{str}");
            }

            /*Excel最大列数(这是获取整个表最大列数的方法，我只要第一行列数作为table列数)
            int MaxColumnNum = 0;
            for (int i = 0; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row.LastCellNum > MaxColumnNum)
                {
                    MaxColumnNum = row.LastCellNum;
               }
            }*/
            //Excel行数
            int MaxRowNum = sheet.LastRowNum;
            //Excel列数
            int MaxColumnNum = sheet.GetRow(0).LastCellNum;

            //table新增列
            for (int i = 0; i < MaxColumnNum; ++i)
            {
                //首行为列
                if (isFirstRowColumn)
                {
                    bool addEmptyCell = true; //是否添加空列
                    ICell cell = sheet.GetRow(1).GetCell(i);
                    if (cell != null)
                    {
                        //table列赋值
                        string cellValue = ""; //列名
                        if (cell.CellType == CellType.Numeric)
                        {
                            cellValue = cell.NumericCellValue.ToString();
                        }
                        else
                        {
                            cellValue = cell.StringCellValue;
                        }

                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            //列数据为Excel的数据
                            addEmptyCell = false;
                            myTable.Columns.Add(new DataColumn(cellValue));
                        }
                    }

                    if (addEmptyCell)
                    {
                        myTable.Columns.Add(new DataColumn("")); //列数据为空
                    }
                }
                else
                {
                    myTable.Columns.Add(new DataColumn(i + ""));
                }
            }

            //起始行
            int startRow = 0;
            if (isFirstRowColumn)
            {
                startRow = 1;
            }

            //DataTable赋值
            for (int i = startRow; i <= MaxRowNum; ++i)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                DataRow NewRow = myTable.NewRow();
                for (int j = row.FirstCellNum; j < row.LastCellNum; ++j)
                {
                    ICell cell = row.GetCell(j);
                    if (cell != null)
                    {
                        //table行赋值 
                        //CellType.Numeric 数值型
                        //CellType.String 字符串型
                        //CellType.Formula 公式型
                        //CellType.Blank 空值
                        //CellType.Boolean 布尔型
                        //CellType.Error 错误类型
                        if (cell.CellType == CellType.Numeric)
                        {
                            NewRow[j] = cell.NumericCellValue;
                        }
                        else
                        {
                            row.GetCell(j).SetCellType(CellType.String);
                            NewRow[j] = cell.StringCellValue;
                        }
                    }
                }

                myTable.Rows.Add(NewRow);
            }

            return myTable;
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
        }

        /// <summary>
        /// 获取excel的DataTable
        /// </summary>
        /// <param name="filePath">excel文件路径</param>
        /// <param name="sheetName">分页名</param>
        /// <returns></returns>
        public static DataTable GetExcelContent(String filePath, string sheetName)
        {
            if (sheetName == "_xlnm#_FilterDatabase")
                return null;
            DataSet dateSet = new DataSet();
            String connectionString =
                String.Format(
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=NO;IMEX=1;'",
                    filePath);
            String commandString = string.Format("SELECT * FROM [{0}$]", sheetName);
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand(commandString, connection))
                {
                    OleDbCommand objCmd = new OleDbCommand(commandString, connection);
                    OleDbDataAdapter myData = new OleDbDataAdapter(commandString, connection);
                    myData.Fill(dateSet, sheetName);
                    DataTable table = dateSet.Tables[sheetName];

                    for (int i = 0; i < table.Rows[0].ItemArray.Length; i++)
                    {
                        var cloumnName = table.Rows[0].ItemArray[i].ToString();
                        if (!string.IsNullOrEmpty(cloumnName))
                            table.Columns[i].ColumnName = cloumnName;
                    }

                    table.Rows.RemoveAt(0);
                    return table;
                }
            }
        }

        /// <summary>
        /// 获取excel文件里面的所有的工作表名称
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<string> GetExcelSheetNames(string filePath)
        {
            OleDbConnection connection = null;
            System.Data.DataTable dt = null;
            try
            {
                String connectionString =
                    String.Format(
                        "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES;IMEX=1;'",
                        filePath);
                connection = new OleDbConnection(connectionString);
                connection.Open();
                dt = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return new List<string>();
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString().Split('$')[0];
                    i++;
                }

                return excelSheets.Distinct().ToList();
            }
            catch (Exception ex)
            {
                // LogHelper.Logger.Error(ex);
                return new List<string>();
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }

                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        /// <summary>
        /// excel转换为json
        /// </summary>
        /// <param name="filePath"></param>
        public static void ExcelToJson(string filePath)
        {
            List<string> tableNames = ExcelHelper.GetExcelSheetNames(filePath);
            var json = new JObject();
            tableNames.ForEach(tableName =>
            {
                var table = new JArray() as dynamic;
                DataTable dataTable = ExcelHelper.GetExcelContent(filePath, tableName);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    dynamic row = new JObject();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        row.Add(column.ColumnName, dataRow[column.ColumnName].ToString());
                    }

                    table.Add(row);
                }

                json.Add(tableName, table);
            });
            Console.WriteLine(json.ToString());
            Console.WriteLine(json.ToString(Formatting.None));
        }
    }
    
}
        
