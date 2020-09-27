using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NPOI.SS.Formula.Functions;

namespace SlotsMath.Properties
{
    public enum SymbolType
    {
        Normal,//普通元素
        Wild,//万能元素
        Scatter,//特殊机制元素
        Bonus,//大奖元素
        Collect,//收集元素
        Custom//自定义元素

    }

    public enum ReelType
    {
        Base,
        Free,
        Custom
    }

    /// <summary>
    /// 元素类，维护元素的赔率，中奖次数等信息
    /// </summary>
    public class SlotsSymbol
    {
        public int SymbolId;                    //元素ID
        public SymbolType SymbolType;                  //元素类型
        public List<double> SymbolPay;            //赔付金额列表
        public List<int> SymbolWinCount;            //中奖次数记录列表
        
        public SlotsSymbol(int symbolId,string symbolType)
        {
            SymbolId = symbolId;
            //设置slotsSymbol类型
            switch (symbolType)
            {
                case "normal":
                    SymbolType = Properties.SymbolType.Normal;
                    break;
                case "wild":
                    SymbolType = Properties.SymbolType.Wild;
                    break;
                case "scatter":
                    SymbolType = Properties.SymbolType.Scatter;
                    break;
                case "bonus":
                    SymbolType = Properties.SymbolType.Bonus;
                    break;
                case "collect":
                    SymbolType = Properties.SymbolType.Collect;
                    break;
                case "custom":
                    SymbolType = Properties.SymbolType.Custom;
                    break;
            }
            
        }
        
        /// <summary>
        /// 设置元素赔率（构建时可不设置赔率，单独使用次方法设置赔率）
        /// </summary>
        public void SetSlotsSymbolPay(List<double> symbolPaylist)
        {
            SymbolPay = new List<double>(symbolPaylist);
        }

        /// <summary>
        /// 获取元素赔率
        /// </summary>
        /// <param name="symbolCount">元素数量</param>
        /// <returns>中奖赔率</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double GetSymbolPay(int symbolCount)
        {
            if (symbolCount>SymbolPay.Count-1)
            {
                throw new  ArgumentOutOfRangeException(symbolCount+"个"+SymbolId+"元素的赔率在赔率表中不存在");
            }
            return SymbolPay[symbolCount];
        }

        /// <summary>
        /// 获取元素中奖次数
        /// </summary>
        /// <param name="symbolCount">元素数量</param>
        /// <returns>中奖次数</returns>
        public int GetSymbolWinCount(int symbolCount)
        {
            return SymbolWinCount[symbolCount];
        }

        /// <summary>
        /// 元素中奖次数增加（默认增加1）
        /// </summary>
        /// <param name="symbolCount"></param>
        /// <param name="winCount"></param>
        public void AddSymbolWinCount(int symbolCount,int winCount =1)
        {
            SymbolWinCount[symbolCount] += winCount;
        }
    }
    
    
    /// <summary>
    /// 元素总类
    /// </summary>
    public class SlotsSymbols
    {
        public Dictionary<int, SlotsSymbol> SlotsSymbolsDic;

        /// <summary>
        /// 从元素表，生产元素字典对象（不会设置赔率，需要单独指定元素的赔率）
        /// </summary>
        /// <param name="symbolDataTable">元素表</param>
        public SlotsSymbols(DataTable symbolDataTable,DataTable payDataTable)
        {
            int normalSymbolCount = 0;
            int specialSymbolCount = 0;
            foreach (DataRow dataRow in symbolDataTable.Rows)
            {
                int symbolId = Convert.ToInt32(dataRow[0].ToString());
                SlotsSymbol tempSlotsSymbol = new SlotsSymbol(symbolId,dataRow["type"].ToString());
                SlotsSymbolsDic[symbolId] = tempSlotsSymbol;
                if (dataRow["type"].ToString() == "normal")
                {
                    normalSymbolCount++;
                }
                else
                {
                    specialSymbolCount++;
                }
            }
            SetSymbolsPay(payDataTable);
        }

        /// <summary>
        /// 读赔率表，设置元素赔率
        /// </summary>
        /// <param name="payDataTable"></param>
        public void SetSymbolsPay(DataTable payDataTable)
        {
            foreach (DataRow dataRow in payDataTable.Rows)
            {
                try
                {
                    int symbolid = Convert.ToInt32(dataRow["id"].ToString());
                    List<double> tempSymbolPayList = new List<double>{0,0,0,0,0,0,0,0};
                    tempSymbolPayList[2] = Convert.ToDouble(dataRow["2symbolPay"].ToString());
                    tempSymbolPayList[3] = Convert.ToDouble(dataRow["3symbolPay"].ToString());
                    tempSymbolPayList[4] = Convert.ToDouble(dataRow["4symbolPay"].ToString());
                    tempSymbolPayList[5] = Convert.ToDouble(dataRow["5symbolPay"].ToString());
                    tempSymbolPayList[6] = Convert.ToDouble(dataRow["6symbolPay"].ToString());
                    SlotsSymbolsDic[symbolid].SetSlotsSymbolPay(tempSymbolPayList);
                }
                catch (Exception e)
                {
                    Console.WriteLine("设置元素赔率时报错,错误元素id："+dataRow["id"].ToString());
                    throw;
                }
            }
        }
    }


    /// <summary>
    /// 单个卷轴类,包含卷轴的截取方法    
    /// </summary>
    public class SlotsReel
    {
        public List<List<int>> Reel;            //扩展前的卷轴
        public List<List<int>> ExpandReel;    //扩展后的卷轴
        
        
        public readonly int ColumnsCount;     //列数       
        public readonly List<int> RowsCount;        //行数列表
        
        /// <summary>
        /// 卷轴构造函数
        /// </summary>
        /// <param name="reel">卷轴列表</param>
        public SlotsReel(DataTable dataTable, ReelType reelType = ReelType.Base)
        {
            //todo 这里需要改成从dataTable生成
//            Reel = reel;
//            ColumnsCount = reel.Count;
//            RowsCount = new List<int>();
//            ExpandReel = new List<List<int>>();
//            for (int i = 0; i < ColumnsCount; i++)
//            {
//                ExpandReel[i].AddRange(reel[i]);
//                ExpandReel[i].AddRange(reel[i]);
//                RowsCount.Add(reel[i].Count);
//            }
        }

        /// <summary>
        /// 输入卷轴位置号，和截取长度，输出元素矩阵
        /// </summary>
        /// <param name="position">位置号</param>
        /// <param name="interceptRowsCount">截取行数</param>
        /// <returns></returns>
        public List<List<int>> GetArray(List<int> position,int interceptRowsCount = 3)
        {
            List<List<int>> symbolArray = new List<List<int>>();
            for (int i = 0; i < position.Count; i++)
            {
                List<int> tempList = ExpandReel[i].GetRange(position[i], interceptRowsCount);
                symbolArray.Add(tempList);
            }
            return symbolArray;
        }

    }

    /// <summary>
    /// 所有的卷轴整合到一起，包含卷轴字典，卷轴输入位置号，获取矩阵
    /// </summary>
    public class SlotsReels
    {
        public Dictionary<string, SlotsReel> SlotsReelsDictionary;

        public SlotsReels()
        {
            SlotsReelsDictionary = new Dictionary<string, SlotsReel>();
        }

        /// <summary>
        /// 将SlotsReel添加到SLotsReelsDictionary中
        /// </summary>
        /// <param name="reelDataTable">卷轴数据源</param>
        /// <param name="reelName">卷轴名称</param>
        /// <param name="reelType">卷轴类型</param>
        public void AddReelsInReelsDic(DataTable reelDataTable,string reelName,string reelType = "base")
        {
            ReelType tempReelType = ReelType.Custom;
            if (reelType == "free")
            {
                tempReelType = ReelType.Free;
            }
            if (reelType == "custom")
            {
                tempReelType = ReelType.Custom;
            }
            SlotsReelsDictionary[reelName] = new SlotsReel(reelDataTable,tempReelType);
        }

        /// <summary>
        /// 使用卷轴名称，卷轴的位置号，卷轴列高度，输出元素矩阵
        /// </summary>
        /// <param name="reelName">卷轴名称</param>
        /// <param name="position">位置号列表</param>
        /// <param name="interceptRowsCount">截取长度</param>
        /// <returns></returns>
        public List<List<int>> GetArray(string reelName, List<int> position, int interceptRowsCount = 3)
        {
            return SlotsReelsDictionary[reelName].GetArray(position, interceptRowsCount);
        }

    }

    /// <summary>
    /// 中奖线类
    /// </summary>
    public class PayLine
    {
        public List<List<int>> payLineList;

        public PayLine(DataTable winLineDataTable)
        {
            
        }
    }


    /// <summary>
    /// 矩阵类，包含元素矩阵的计算中奖方法
    /// </summary>
    public class SlotsArray
    {
        public List<List<int>> SymbolArray;                                    //中奖区域元素列表（每1个元素代表1列）
        public bool IsWithLine;
        private Dictionary<int, SlotsSymbol> SlotsSymbolDictionary;
        public List<List<int>> LineConfig;
        
        /// <summary>
        /// 矩阵对象构造函数
        /// </summary>
        /// <param name="symbolArray">元素矩阵列表</param>
        /// <param name="slotsSymbolDictionary">元素字典</param>
        /// <param name="lineConfig">中奖线配置</param>
        public SlotsArray(List<List<int>> symbolArray, Dictionary<int,SlotsSymbol> slotsSymbolDictionary,List<List<int>> lineConfig = null)
        {
            SymbolArray = symbolArray;                                        //元素矩阵
            SlotsSymbolDictionary = slotsSymbolDictionary;                    //元素字典
            LineConfig = lineConfig;                                          //中奖线配置
        }
        
        /// <summary>
        /// 获取从左向右相同元素的数量(有中奖线的情况)
        /// </summary>
        /// <param name="oneLineSymbolList">一条线上的元素列表</param>
        /// <returns></returns>
        public virtual int GetSameSymbolCountWithLine(List<int> oneLineSymbolList)
        {
            int listCount = oneLineSymbolList.Count;
            int winCount = 0;
            int symbolId = 0;
            if (oneLineSymbolList[0]==21)
            {
                //todo 这里没写
                return 0;
            }
            else
            {
                //当第一列不是21时
                for (int i = 1; i < listCount; i++)
                {
                    if (oneLineSymbolList[i] == 21 || oneLineSymbolList[i] == oneLineSymbolList[0] )
                    {
                        winCount++;
                    }
                    else
                    {
                        return winCount;
                    }
                }
                return winCount;
            }
        }
        
        /// <summary>
        /// 有中奖线类计算中奖金额
        /// </summary>
        /// <param name="isChangeWinCount">如果为true，则会修改slotsSymbolDidctionayr中的中奖次数</param>
        /// <returns></returns>
        public virtual double GetWinValueByArrayWithLine(bool isChangeWinCount = true)
        {
            double totalWin = 0;
            List<int> oneLineSymbolList = new List<int>();
            foreach (var i in LineConfig)
            {
                oneLineSymbolList.Clear();
                int index = 0;
                foreach (var j in i)
                {
                    oneLineSymbolList.Add(SymbolArray[index][j]);
                    index++;
                }
            }
            return totalWin;
        }

        /// <summary>
        /// 获取赢钱数（无中奖线）
        /// </summary>
        /// <returns></returns>
        public virtual double GetWinValueByArrayWithoutLine(bool isChangeWinCount = true)
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
        public SlotsSymbols SlotsSymbols;                        //元素对象字典
        public SlotsReels SlotsReels;                         //卷轴对象字典

        public bool IsWithLine;                    //是否有winLine
        public int Row;                            //显示区域的行数            
        public int Columns;                        //显示区域的列数
        public double BaseWin;                     //baseGame赢数
        public double FreeWin;                     //baseGame赢数
        public double TotalWin;                    //baseGame赢数

        /// <summary>
        /// slots构造函数
        /// </summary>
        /// <param name="rowsCount">显示区域行数</param>
        /// <param name="columnsCount">显示区域列数</param>
        public Slots(SlotsSymbols slotsSymbols,SlotsReels slotsReels, int rowsCount = 3,int columnsCount = 5)
        {
            SlotsSymbols = slotsSymbols;                //元素总类实体
            SlotsReels = slotsReels;                 //卷轴字典
            Row = rowsCount;                            
            Columns = columnsCount;
        }

        /// <summary>
        /// todo 需要移动到SlotsSymbols中
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
        /// todo 需要移动到SlotsReels中
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
            for (int i = 0; i < slotsReel.ColumnsCount; i++)
            {
                
            }
            //打印中奖次数信息
            foreach (var VARIABLE in SlotsSymbolDictionary)
            {
                
            }
        }

        /// <summary>
        /// 获取中奖线完整列表
        /// </summary>
        /// <param name="lineConfigDataTable"></param>
        /// <returns></returns>
        public List<List<int>> GetLineConfig(DataTable lineConfigDataTable)
        {
            List<List<int>> outList = new List<List<int>>();
            int rows = lineConfigDataTable.Rows.Count;
            for (int i = 0; i < rows; i++)
            {
                outList.Add(GetOneLineList(lineConfigDataTable.Rows[i][1].ToString()));
            }
            return outList;
        }

        /// <summary>
        /// 获取单条中奖线列表
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        private List<int> GetOneLineList(string lineString)
        {
            List<int> outList = new List<int>();
            string[] arr = lineString.Split(';');
            foreach (string VARIABLE in arr)
            {
                try
                {
                    outList.Add(Convert.ToInt32(VARIABLE.Split(',')[1]));
                }
                catch (Exception e)
                {
                    Console.WriteLine("中奖线获取失败");
                    throw;
                }
            }
            return outList;
        }
    }
}