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
            SymbolPay = new List<double>{0,0,0,0,0,0,0};
            
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
        /// <param name="symbolCount">中奖元素数</param>
        /// <param name="winCount">中奖次数增加数</param>
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
        /// <param name="payDataTable">赔率表</param>
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
        /// <param name="reelDataTable">卷轴的DataTable</param>
        /// <param name="reelType">卷轴类型</param>
        public SlotsReel(DataTable reelDataTable, ReelType reelType = ReelType.Base)
        {
            List<List<int>> reel = new List<List<int>>();
            int rowCount = reelDataTable.Rows.Count;
            int columnsCount = reelDataTable.Columns.Count;
            ColumnsCount = columnsCount;
            for (int i = 0; i < columnsCount; i++)
            {
                for (int j = 1; j < rowCount; j++)
                {
                    if (reelDataTable.Rows[j][i].ToString()!="0")
                    {
                        try
                        {
                            reel[i].Add(Convert.ToInt32(reelDataTable.Rows[j][i].ToString()));
                            RowsCount[j]++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("dataTable转换Reel失败，行数为"+j+",列数为"+i+";");
                            throw;
                        }
                    }
                }
            }
            Reel = reel;
            ExpandReel = GetExpandReel();
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

        /// <summary>
        /// 获取卷轴的扩展列表
        /// </summary>
        /// <returns></returns>
        private List<List<int>> GetExpandReel()
        {
            List<List<int>> expandReel = Clone.DepthClone(Reel);
            for (int i = 0; i < expandReel.Count; i++)
            {
                expandReel[i].AddRange(expandReel[i]);
            }
            return expandReel;
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
        public List<List<int>> payLinesList;

        public PayLine(DataTable winLineDataTable)
        {
            
        }
    }


//    /// <summary>
//    /// 矩阵类，包含元素矩阵的计算中奖方法
//    /// </summary>
//    public class SlotsArray
//    {
//        public List<List<int>> SymbolArray;                                    //中奖区域元素列表（每1个元素代表1列）
//        public bool IsWithLine;
//        private Dictionary<int, SlotsSymbol> SlotsSymbolDictionary;
//        public List<List<int>> LineConfig;
//        
//        /// <summary>
//        /// 矩阵对象构造函数
//        /// </summary>
//        /// <param name="symbolArray">元素矩阵列表</param>
//        /// <param name="slotsSymbolDictionary">元素字典</param>
//        /// <param name="lineConfig">中奖线配置</param>
//        public SlotsArray(List<List<int>> symbolArray, Dictionary<int,SlotsSymbol> slotsSymbolDictionary,List<List<int>> lineConfig = null)
//        {
//            SymbolArray = symbolArray;                                        //元素矩阵
//            SlotsSymbolDictionary = slotsSymbolDictionary;                    //元素字典
//            LineConfig = lineConfig;                                          //中奖线配置
//        }
//    }

    /// <summary>
    /// slots父类，后续所有特殊玩法继承此类
    /// </summary>
    public class Slots
    {
        public SlotsSymbols SlotsSymbols;                        //元素对象字典
        public SlotsReels SlotsReels;                         //卷轴对象字典
        public PayLine PayLine;                                //中奖线对象
        public List<SlotsSymbol> NormalSymbolsList;            //普通元素列表，在全线中只有这里面的元素会被查询普通中奖
        public List<SlotsSymbol> ScatterSymbolsList;
        public List<SlotsSymbol> BonusSymbolsList;
        public List<SlotsSymbol> CollectSymbolsList;
        public List<SlotsSymbol> CustomSymbolsList;
        
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
        public Slots(SlotsSymbols slotsSymbols,SlotsReels slotsReels, int rowsCount = 3,int columnsCount = 5,PayLine payLine = null)
        {
            SlotsSymbols = slotsSymbols;                //元素总类实体
            SlotsReels = slotsReels;                 //卷轴字典
            Row = rowsCount;                            
            Columns = columnsCount;
            PayLine = payLine;
            foreach (SlotsSymbol slotsSymbol in SlotsSymbols.SlotsSymbolsDic.Values)
            {
                switch (slotsSymbol.SymbolType)
                {
                    case SymbolType.Normal:
                        NormalSymbolsList.Add(slotsSymbol);
                        break;
                    case SymbolType.Scatter:
                        ScatterSymbolsList.Add(slotsSymbol);
                        break;
                    case SymbolType.Bonus:
                        BonusSymbolsList.Add(slotsSymbol);
                        break;
                    case SymbolType.Collect:
                        CollectSymbolsList.Add(slotsSymbol);
                        break;
                    case SymbolType.Custom:
                        CustomSymbolsList.Add(slotsSymbol);
                        break;
                }
            }
        }

        public struct WinSymbolInfo
        {
            public int SymbolId;
            public int SymbolsCount;
            public int SymbolWinTime;

            public WinSymbolInfo(int symbolId,int symbolsCount,int symbolWinTime)
            {
                SymbolId = symbolId;
                SymbolsCount = symbolsCount;
                SymbolWinTime = symbolWinTime;
            }
        }

        /// <summary>
        /// 获取从左向右相同元素的数量(有中奖线的情况)
        /// </summary>
        /// <param name="oneLineSymbolList">一条线上的元素列表</param>
        /// <returns>[元素id，中奖数量]，若id=0标示未中奖</returns>
        public virtual WinSymbolInfo GetSameSymbolInfoWithLine(List<int> oneLineSymbolList)
        {
            int listCount = oneLineSymbolList.Count;
            WinSymbolInfo winSymbolInfo = new WinSymbolInfo(0,0,0);
            if (oneLineSymbolList[0]==21)
            {
                //todo 这里没写
                return winSymbolInfo;
            }
            else
            {
                //当第一列不是21时
                for (int i = 1; i < listCount; i++)
                {
                    if (oneLineSymbolList[i] == 21 || oneLineSymbolList[i] == oneLineSymbolList[0] )
                    {
                        winSymbolInfo.SymbolId = oneLineSymbolList[0];
                        winSymbolInfo.SymbolsCount++;
                    }
                    else
                    {
                        winSymbolInfo.SymbolWinTime++;
                        return winSymbolInfo;
                    }
                }
                return winSymbolInfo;
            }
        }
        
        /// <summary>
        /// 有中奖线类计算中奖金额
        /// </summary>
        /// <param name="symbolArray">实际使用的元素矩阵</param>
        /// <param name="isChangeWinTime">如果为true，则会修改slotsSymbolDictionary中的中奖次数</param>
        /// <returns></returns>
        public virtual double GetWinValueByArrayWithLine(List<List<int>> symbolArray,bool isChangeWinTime = true)
        {
            double totalWin = 0;
            List<int> oneLineSymbolList = new List<int>();
            foreach (var payline in PayLine.payLinesList)
            {
                oneLineSymbolList.Clear();
                int index = 0;
                //获取线上的元素
                foreach (var j in payline)
                {
                    oneLineSymbolList.Add(symbolArray[index][j]);
                    index++;
                }
                WinSymbolInfo tempWinSymbolInfo = GetSameSymbolInfoWithLine(oneLineSymbolList);
                if (tempWinSymbolInfo.SymbolId>0)
                {
                    TotalWin += SlotsSymbols.SlotsSymbolsDic[tempWinSymbolInfo.SymbolId]
                        .GetSymbolPay(tempWinSymbolInfo.SymbolsCount);
                    if (isChangeWinTime)
                    {
                        SlotsSymbols.SlotsSymbolsDic[tempWinSymbolInfo.SymbolId].AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount);
                    }
                }
            }
            return totalWin;
        }

        
        public WinSymbolInfo GetWinSymbolInfoByArrayWithoutLine(List<List<int>> symbolArray, int symbolId)
        {
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo(symbolId,0,0);
            List<int> symbolCountInArray = new List<int>();
            //获取每一列特定元素数量
            for (int i = 0; i < symbolArray.Count; i++)
            {
                symbolCountInArray.Add(Tools.GetElementCountInList(symbolArray[i],symbolId));
            }
            //todo
            return tempWinSymbolInfo;
        }

        /// <summary>
        /// 无中奖线时计算中奖
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <param name="isChangeWinCount"></param>
        /// <param name="isPrintWinInformation"></param>
        /// <returns></returns>
        public virtual double GetWinValueByArrayWithoutLine(List<List<int>> symbolArray,bool isChangeWinCount = true,bool isPrintWinInformation = false)
        {
            double totalWin = 0;
            return totalWin;
        }

        /// <summary>
        /// 遍历卷轴，获取所有元素的中奖中中奖金额，在每个symbols中记录中奖次数
        /// </summary>
        /// <param name="reelName">卷轴名称</param>
        public void ErgodicReel(string reelName)
        {
            SlotsReel slotsReel = SlotsReels.SlotsReelsDictionary[reelName];
            for (int i = 0; i < slotsReel.ColumnsCount; i++)
            {
                
            }
            //打印中奖次数信息
//            foreach (var VARIABLE in SlotsSymbolDictionary)
//            {
//                
//            }
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