using System;
using System.Collections.Generic;
using System.Data;

namespace SlotsMath.Properties
{
    /// <summary>
    /// 元素类，维护元素的赔率，中奖次数等信息
    /// </summary>
    public class SlotsSymbol
    {
        public int SymbolId;
        public List<double> SymbolPay;
        public List<int> SymbolWinCount;

        public SlotsSymbol(int symbolId,List<double> symbolPay)
        {
            SymbolId = symbolId;
            SymbolPay = symbolPay;
        }
        
        public double GetSymbolPay(int symbolCount)
        {
            if (symbolCount>SymbolPay.Count-1)
            {
                throw new  ArgumentOutOfRangeException(symbolCount+"个"+SymbolId+"元素的赔率在赔率表中不存在");
            }
            return SymbolPay[symbolCount];
        }

        public int GetSymbolWinCount(int symbolCount)
        {
            return SymbolWinCount[symbolCount];
        }

        public void AddSymbolWinCount(int symbolCount,int winCount =1)
        {
            SymbolWinCount[symbolCount] += winCount;
        }
    }

    
    /// <summary>
    /// 卷轴类,包含卷轴的截取方法    
    /// </summary>
    public class SlotsReel
    {
        public List<List<int>> Reel;
        public List<List<int>> ExpandReel;
        
        public readonly int RowCount;
        public readonly List<int> ColumnsCount;
        
        public SlotsReel(List<List<int>> reel)
        {
            Reel = reel;
            RowCount = reel.Count;
            ColumnsCount = new List<int>();
            ExpandReel = new List<List<int>>();
            for (int i = 0; i < RowCount; i++)
            {
                ExpandReel[i].AddRange(reel[i]);
                ExpandReel[i].AddRange(reel[i]);
                ColumnsCount.Add(reel[i].Count);
            }
        }

        public SlotsArray GetArray(List<int> position,int interceptRowsCount,int interceptColumnsCount)
        {
            List<List<int>> symbolArray = new List<List<int>>();
            SlotsArray slotsArray = new SlotsArray(symbolArray);
            return slotsArray;
        }

    }

    /// <summary>
    /// 矩阵类，包含元素矩阵的计算中奖方法
    /// </summary>
    public class SlotsArray
    {
        public List<List<int>> SymbolArray;
        
        public SlotsArray(List<List<int>> symbolArray)
        {
            SymbolArray = symbolArray;
        }
        
        public virtual double GetWinValueByArray()
        {
            double outValue = 0;
            return outValue;
        }


    }
    
    /// <summary>
    /// slots父类，后续所有特殊玩法继承此类
    /// </summary>
    public class Slots
    {
        public Dictionary<int,SlotsSymbol> SlotsSymbolDictionary;                            //元素对象字典
        public Dictionary<string, SlotsReel> SlotsReelsDictionary;                            //卷轴对象字典

        public bool IsWithLine;
        public int Row;                                        
        public int Columns;
        public double BaseWin;
        public double FreeWin;
        public double TotalWin;

        /// <summary>
        /// slots构造函数
        /// </summary>
        /// <param name="rowsCount">显示区域行数</param>
        /// <param name="columnsCount">显示区域列数</param>
        public Slots(int rowsCount = 3,int columnsCount = 5)
        {
            SlotsSymbolDictionary = new Dictionary<int, SlotsSymbol>();
            SlotsReelsDictionary = new Dictionary<string, SlotsReel>();
            Row = rowsCount;
            Columns = columnsCount;
        }

        /// <summary>
        /// 从payTable的DataTable中生成SlotsSymbol
        /// </summary>
        /// <param name="dataTable">包含payTable信息的dataTable</param>
        /// <param name="symbolId">元素id</param>
        /// <returns>slotsSymbol对象</returns>
        public SlotsSymbol GetSymbolFromDataTable(DataTable dataTable,int symbolId)
        {
            List<double> symbolPay = new List<double>();
            for (int i = 0; i <= Columns; i++)
            {
                symbolPay.Add(0); 
            }
            for (int i = 1; i < dataTable.Rows.Count; i++)
            {
                symbolPay[i] = 1;
            }
            SlotsSymbol slotsSymbol = new SlotsSymbol(symbolId,symbolPay);
            return slotsSymbol;
        }

        /// <summary>
        /// 从DataTable生成SlotsReel
        /// </summary>
        /// <param name="dataTable">包含reel信息的dataTable</param>
        /// <returns>生成的slotsReel对象</returns>
        public SlotsReel GetReelFromDataTable(DataTable dataTable)
        {
            List<List<int>> reel = new List<List<int>>();
            int rowCount = dataTable.Rows.Count;
            int columnsCount = dataTable.Columns.Count;
            for (int i = 0; i < columnsCount; i++)
            {
                for (int j = 1; j < rowCount; j++)
                {
                    if (dataTable.Rows[j][i].ToString()!="0")
                    {
                        try
                        {
                            reel[i].Add(Convert.ToInt32(dataTable.Rows[j][i].ToString()));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("dataTable转换Reel失败，行数为"+j+",列数为"+i+";");
                            throw;
                        }
                    }
                }
            }
            SlotsReel slotsReel = new SlotsReel(reel);
            return slotsReel;
        }

        /// <summary>
        /// 遍历卷轴，获取所有元素的中奖中中奖金额，在每个symbols中记录中奖次数
        /// </summary>
        /// <param name="reelName">卷轴名称</param>
        public void ErgodicReel(string reelName)
        {
            SlotsReel slotsReel = SlotsReelsDictionary[reelName];
            for (int i = 0; i < slotsReel.RowCount; i++)
            {
                
            }
            //打印中奖次数信息
            foreach (var VARIABLE in SlotsSymbolDictionary)
            {
                
            }
        }
    }
}