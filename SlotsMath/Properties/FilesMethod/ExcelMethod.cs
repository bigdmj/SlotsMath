/*
 * excel转dataTable等
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json;

namespace SlotsMath.Properties.FileMethod
{
    public static class ExcelMethod
    {
        /// <summary>  
        /// 将excel中特定名称的表导入到datatable  
        /// </summary>  
        /// <param name="filePath">excel路径</param>  
        /// <param name="firstIsColumnName">第一行是否是列名,否则第二行才是列名</param>
        /// <returns>返回datatable</returns>  
        public static DataTable ExcelToDataTableByName(string filePath, string bookName, bool firstIsColumnName)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = System.IO.File.OpenRead(filePath))
                {
                    // 2007版本  
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本  
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);
 
                    if (workbook != null)
                    {
                        sheet = workbook.GetSheet(bookName);//读取特定名称的分页  
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数  
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行  
                                IRow secondRow = sheet.GetRow(1);//第二行  
                                int cellCount = firstRow.LastCellNum;//列数  
 
                                //构建datatable的列  
                                if (firstIsColumnName)
                                {
                                    startRow = 1;//如果第一行是列名，则从第二行开始读取  
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else//否则第2行才是表头，不管第一行
                                {
                                    startRow = 2;//如果第一行是列名，则从第二行开始读取  
                                    for (int i = secondRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
 
                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;
 
                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                throw ex;
            }
        }

        /// <summary>
        /// 将excel中全部的表导入到dataTable字典中，用excel的分页名做字典的key
        /// </summary>
        /// <param name="filePath">文件名称</param>
        /// <param name="isPassFirstColumn">是否跳过第一行，默认为true</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, DataTable> ExcelToDataTables(string filePath,bool isPassFirstColumn = true)
        {
            Dictionary<string ,DataTable> dictionary = new Dictionary<string, DataTable>();
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = System.IO.File.OpenRead(filePath))
                {
                    // 2007版本  
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本  
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);
 
                    if (workbook != null)
                    {
                        for (int s = 0; s < workbook.NumberOfSheets; s++)
                        {
                            sheet = workbook.GetSheetAt(s);
                            dataTable = new DataTable();
                            if (sheet != null)
                            {
                                int rowCount = sheet.LastRowNum;//总行数  
                                if (rowCount > 0)
                                {
                                    IRow firstRow = sheet.GetRow(0);//第一行  
                                    int cellCount = firstRow.LastCellNum;//列数  
     
                                    //构建datatable的列  
                                    //todo 这里要改成读取我们的配置表
                                    startRow = isPassFirstColumn ? 2:1; //如果要跳过第一行，则从第2行当表头  
                                    if (isPassFirstColumn)
                                    {
                                        startRow = 2;
                                        firstRow = sheet.GetRow(1);
                                    }
                                    else
                                    {
                                        startRow = 1;
                                        firstRow = sheet.GetRow(0);
                                    }
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                    //填充行  
                                    for (int i = startRow; i <= rowCount; ++i)
                                    {
                                        row = sheet.GetRow(i);
                                        if (row == null) continue;
     
                                        dataRow = dataTable.NewRow();
                                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                                        {
                                            cell = row.GetCell(j);
                                            if (cell == null)
                                            {
                                                dataRow[j] = "";
                                            }
                                            else
                                            {
                                                //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                                switch (cell.CellType)
                                                {
                                                    case CellType.Blank:
                                                        dataRow[j] = "";
                                                        break;
                                                    case CellType.Numeric:
                                                        short format = cell.CellStyle.DataFormat;
                                                        //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                                                        if (format == 14 || format == 31 || format == 57 || format == 58)
                                                            dataRow[j] = cell.DateCellValue;
                                                        else
                                                            dataRow[j] = cell.NumericCellValue;
                                                        break;
                                                    case CellType.String:
                                                        dataRow[j] = cell.StringCellValue;
                                                        break;
                                                }
                                            }
                                        }
                                        dataTable.Rows.Add(dataRow);
                                    }
                                }
                            }
                            dictionary.Add(sheet.SheetName,dataTable);
                        }
                        
                    }
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                fs?.Close();
                throw ex;
            }
        }
        
    }
}

        
    
