using System;
using System.Collections.Generic;
using System.Data;

namespace SlotsMath.Properties.SlotsComputer
{
    /// <summary>
    /// slots父类，后续所有特殊玩法继承此类
    /// </summary>
    public class Slots
    {
        public SlotsSymbols SlotsSymbols; //元素对象字典
        public SlotsReels BaseSlotsReels; //卷轴对象字典
        public SlotsReels FreeSlotsReels; //卷轴对象字典
        public PayLine PayLine; //中奖线对象
        public List<SlotsSymbol> NormalSymbolsList; //普通元素列表，在全线中只有这里面的元素会被查询普通中奖
        public List<SlotsSymbol> WildSymbolsList; 
        public List<SlotsSymbol> ScatterSymbolsList;
        public List<SlotsSymbol> BonusSymbolsList;
        public List<SlotsSymbol> CollectSymbolsList;
        public List<SlotsSymbol> CustomSymbolsList;

        public bool IsWithLine; //是否有winLine
        public int Row; //显示区域的行数            
        public int Columns; //显示区域的列数
        public double BaseWin; //baseGame赢数
        public double FreeWin; //baseGame赢数
        public double TotalWin; //baseGame赢数

        /// <summary>
        /// slots构造函数
        /// </summary>
        /// <param name="dictionary">右excel转化来的DataTable字典</param>
        /// <param name="rowsCount">显示区域行数</param>
        /// <param name="columnsCount">显示区域列数</param>
        /// <param name="isWithLine">是否有中奖线</param>
        public Slots(Dictionary<string, DataTable> dictionary , int rowsCount = 3, int columnsCount = 5,
            bool isWithLine = true)
        {
            //生成SlotsSymbols
            SlotsSymbols = new SlotsSymbols(dictionary["element"], dictionary["reward"]);
            //生成SlotsReels(所有的basespin的卷轴都必须有这个关键字，且其他任何sheet不能用这个关键字；freespin卷轴亦然)
            BaseSlotsReels = new SlotsReels();
            FreeSlotsReels = new SlotsReels();
            foreach (string key in dictionary.Keys)
            {
                if (key.Contains("basespin"))
                {
                    BaseSlotsReels.AddSlotsReel(key, dictionary[key]);
                }

                if (key.Contains("freespin"))
                {
                    FreeSlotsReels.AddSlotsReel(key, dictionary[key]);
                }
            }
            //生产line配置
            if (isWithLine)
            {
                PayLine = new PayLine(dictionary["line"]);
            }
            //将元素装到对应的类型容器中
            NormalSymbolsList = new List<SlotsSymbol>();
            WildSymbolsList = new List<SlotsSymbol>();
            ScatterSymbolsList = new List<SlotsSymbol>();
            BonusSymbolsList = new List<SlotsSymbol>();
            CollectSymbolsList = new List<SlotsSymbol>();
            CustomSymbolsList = new List<SlotsSymbol>();
            foreach (int key in SlotsSymbols.SlotsSymbolsDic.Keys)
            {
                switch (SlotsSymbols.SlotsSymbolsDic[key].SymbolType)
                {
                    case SymbolType.Normal:
                        NormalSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    case SymbolType.Wild:
                        WildSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    case SymbolType.Scatter:
                        ScatterSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    case SymbolType.Bonus:
                        BonusSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    case SymbolType.Collect:
                        CollectSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    case SymbolType.Custom:
                        CustomSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
                        break;
                    default:
                        throw (new SlotsTools.TempIsZeroException("Symbol Type is Wrong"));
                }
            }
            //给常用变量赋值
            IsWithLine = isWithLine;
            Row = rowsCount;
            Columns = columnsCount;
            BaseWin = 0;
            FreeWin = 0;
            TotalWin = 0;
        }

        /// <summary>
        /// 中奖元素信息结构体
        /// </summary>
        public struct WinSymbolInfo
        {
            public int SymbolId; //元素id
            public int SymbolsCount; //中奖元素数
            public int SymbolWinTime; //中奖次数

            /// <summary>
            /// 结构体构造函数
            /// </summary>
            /// <param name="symbolId">元素id</param>
            /// <param name="symbolsCount">中奖元素数</param>
            /// <param name="symbolWinTime">中奖次数</param>
            public WinSymbolInfo(int symbolId, int symbolsCount, int symbolWinTime)
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
            WinSymbolInfo winSymbolInfo = new WinSymbolInfo(0, 0, 0);
            if (oneLineSymbolList[0] == 21)
            {
                //todo 这里没写
                return winSymbolInfo;
            }
            else
            {
                //当第一列不是21时
                for (int i = 1; i < listCount; i++)
                {
                    if (oneLineSymbolList[i] == 21 || oneLineSymbolList[i] == oneLineSymbolList[0])
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
        public virtual double GetWinValueByArrayWithLine(List<List<int>> symbolArray, bool isChangeWinTime = true)
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
                if (tempWinSymbolInfo.SymbolId > 0)
                {
                    TotalWin += SlotsSymbols.SlotsSymbolsDic[tempWinSymbolInfo.SymbolId]
                        .GetSymbolPay(tempWinSymbolInfo.SymbolsCount);
                    if (isChangeWinTime)
                    {
                        SlotsSymbols.SlotsSymbolsDic[tempWinSymbolInfo.SymbolId]
                            .AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount);
                    }
                }
            }

            return totalWin;
        }

        /// <summary>
        /// 全中奖线时获取特定元素的中奖信息（对第一列有wild的情况无效）
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="symbolId">元素id</param>
        /// <param name="isWildCanSub">此元素是否能被wild替代</param>
        /// <returns>中奖元素信息</returns>
        public WinSymbolInfo GetWinSymbolInfoByArrayWithoutLine(List<List<int>> symbolArray, int symbolId,
            bool isWildCanSub = true)
        {
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo(symbolId, 0, 0);
            List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量
            //获取每一列特定元素数量
            for (int i = 0; i < symbolArray.Count; i++)
            {
                int tempElementCount = SlotsTools.GetElementCountInList(symbolArray[i], symbolId);
                if (isWildCanSub)
                {
                    tempElementCount += SlotsTools.GetElementCountInList(symbolArray[i], 21);
                }

                symbolCountInArray.Add(tempElementCount);
            }

            //确定中奖元素数和中奖次数
            for (int i = 0; i < symbolCountInArray.Count; i++)
            {
                if (symbolCountInArray[i] == 0)
                {
                    return tempWinSymbolInfo;
                }
                else
                {
                    tempWinSymbolInfo.SymbolsCount = i;
                    tempWinSymbolInfo.SymbolWinTime = i == 0
                        ? symbolCountInArray[0]
                        : tempWinSymbolInfo.SymbolWinTime * symbolCountInArray[i];
                }
            }

            return tempWinSymbolInfo;
        }

        /// <summary>
        /// 从元素矩阵中获取离散元素的信息
        /// </summary>
        /// <param name="symbolId"></param>
        /// <returns></returns>
        public WinSymbolInfo GetScatterSymbolInfoByArray(List<List<int>> symbolArray, int symbolId)
        {
            WinSymbolInfo winSymbolInfo = new WinSymbolInfo();
            List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量
            //获取每一列特定元素数量
            foreach (var t in symbolArray)
            {
                int tempElementCount = SlotsTools.GetElementCountInList(t, symbolId);
                symbolCountInArray.Add(tempElementCount);
            }

            foreach (var t in symbolCountInArray)
            {
                if (t > 0)
                {
                    winSymbolInfo.SymbolsCount++;
                    winSymbolInfo.SymbolWinTime =
                        winSymbolInfo.SymbolsCount == 0 ? t : winSymbolInfo.SymbolsCount * t;
                }
            }

            return winSymbolInfo;
        }

        /// <summary>
        /// 无中奖线时计算中奖
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <param name="isChangeWinCount"></param>
        /// <param name="isPrintWinInformation"></param>
        /// <returns></returns>
        public virtual double GetWinValueByArrayWithoutLine(List<List<int>> symbolArray,
            bool isChangeWinCount = true, bool isPrintWinInformation = false)
        {
            double totalWin = 0;
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                WinSymbolInfo tempWinSymbolInfo =
                    GetWinSymbolInfoByArrayWithoutLine(symbolArray, normalSymbol.SymbolId);
                totalWin += SlotsSymbols.SlotsSymbolsDic[normalSymbol.SymbolId]
                                .GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                if (isChangeWinCount)
                {
                    SlotsSymbols.SlotsSymbolsDic[normalSymbol.SymbolId]
                        .AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                }

                if (isPrintWinInformation)
                {
                    Console.WriteLine("元素id：" + tempWinSymbolInfo.SymbolId + "，中奖元素数：" +
                                      tempWinSymbolInfo.SymbolsCount + "中奖次数:" + tempWinSymbolInfo.SymbolWinTime +
                                      ";");
                }
            }

            return totalWin;
        }

        /// <summary>
        /// 遍历卷轴，获取所有元素的中奖金额，在每个symbols中记录中奖次数
        /// </summary>
        /// <param name="reelName">卷轴名称</param>
        public void ErgodicReel(string reelName)
        {
            SlotsReel slotsReel = BaseSlotsReels.SlotsReelsDictionary[reelName];
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
        /// 全过程模拟
        /// </summary>
        /// <param name="analogTime"></param>
        public void AnalogMethod(int analogTime)
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
    }
}