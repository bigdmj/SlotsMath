/*
 * slots用到的基础类，包括symbol，reel，line等
 */
using System;
using System.Collections.Generic;
using System.Data;

namespace SlotsMath.Properties
{
    /// <summary>
    /// 元素类型枚举
    /// </summary>
    public enum SymbolType
    {
        Normal,//1-普通元素
        Wild,//2-万能元素
        Scatter,//4-特殊机制元素
        Bonus,//8-大奖元素
        Free,//16-freespin元素
        Collect,//32-收集元素
        Custom//64-自定义元素

    }
    
    
    /// <summary>
    /// 中奖元素信息结构体
    /// </summary>
    public struct WinSymbolInfo
    {
        public SlotsSymbol SlotsSymbol; //元素类
        public int SymbolsCount; //中奖元素数
        public int SymbolWinTime; //中奖次数
        public int _3wildWinTime;//3个wild中奖次数（全中奖线可能同时有普通元素中奖和wild中奖）
        public int _4wildWinTime;
        public int _5wildWinTime;
        

        /// <summary>
        /// 结构体构造函数
        /// </summary>
        /// <param name="symbolId">元素id</param>
        /// <param name="symbolsCount">中奖元素数</param>
        /// <param name="symbolWinTime">中奖次数</param>
        public WinSymbolInfo(SlotsSymbol slotsSymbol, int symbolsCount, int symbolWinTime)
        {
            SlotsSymbol = slotsSymbol;
            SymbolsCount = symbolsCount;
            SymbolWinTime = symbolWinTime;
            _3wildWinTime = 0;
            _4wildWinTime = 0;
            _5wildWinTime = 0;
        }
    }
    
    /// <summary>
    /// 卷轴类型枚举
    /// </summary>
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
        public Dictionary<int,double> SymbolPay;            //赔付金额字典
        public Dictionary<int,int> SymbolWinCount;            //中奖次数记录列表
        
        /// <summary>
        /// 构建函数
        /// </summary>
        /// <param name="symbolId">元素id</param>
        /// <param name="symbolType">元素类型</param>
        public SlotsSymbol(int symbolId,string symbolType)
        {
            SymbolId = symbolId;
            //设置slotsSymbol类型
            switch (symbolType)
            {
                case "normal":
                    SymbolType = SymbolType.Normal;
                    break;
                case "wild":
                    SymbolType = SymbolType.Wild;
                    break;
                case "scatter":
                    SymbolType = SymbolType.Scatter;
                    break;
                case "bonus":
                    SymbolType = SymbolType.Bonus;
                    break;
                case "free":
                    SymbolType = SymbolType.Free;
                    break;
                case "collect":
                    SymbolType = SymbolType.Collect;
                    break;
                case "custom":
                    SymbolType = SymbolType.Custom;
                    break;
            }
            SymbolPay = new Dictionary<int, double>();
            SymbolWinCount = new Dictionary<int, int>();
        }

        /// <summary>
        /// 构建函数
        /// </summary>
        /// <param name="symbolId">id</param>
        /// <param name="symbolPay">赔率字典</param>
        /// <param name="symbolType">类型</param>
        public SlotsSymbol(int symbolId, Dictionary<int, double> symbolPay, string symbolType)
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

            SymbolPay = symbolPay;
            SymbolWinCount = new Dictionary<int, int>();
        }

        /// <summary>
        /// 设置元素赔率（构建时可不设置赔率，单独使用次方法设置赔率）
        /// </summary>
        public void SetSlotsSymbolPay(int symbolCount,double payValue)
        {
            if (!SymbolPay.ContainsKey(symbolCount))
            {
                SymbolPay.Add(symbolCount,payValue);
            }
            else
            {
                SymbolPay[symbolCount] = payValue;
            }
        }

        /// <summary>
        /// 获取元素赔率
        /// </summary>
        /// <param name="symbolCount">元素数量</param>
        /// <returns>中奖赔率</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double GetSymbolPay(int symbolCount)
        {
            if (!SymbolPay.ContainsKey(symbolCount))
            {
                return 0;
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
            if (!SymbolWinCount.ContainsKey(symbolCount))
            {
                throw new  ArgumentOutOfRangeException(symbolCount+"个"+SymbolId+"元素的中奖次数在次数表中不存在");
            }
            return SymbolWinCount[symbolCount];
        }

        /// <summary>
        /// 元素中奖次数增加（默认增加1）
        /// </summary>
        /// <param name="symbolCount">中奖元素数</param>
        /// <param name="winCount">中奖次数增加数</param>
        public void AddSymbolWinCount(int symbolCount,int winCount =1)
        {
            if (!SymbolWinCount.ContainsKey(symbolCount))
            {
                SymbolWinCount.Add(symbolCount,winCount);
            }
            else
            {
                SymbolWinCount[symbolCount] += winCount;
            }
        }
    }


    /// <summary>
    /// 元素总类
    /// </summary>
    public class SlotsSymbols
    {
        public Dictionary<int, SlotsSymbol> SlotsSymbolsDic;

        /// <summary>
        /// 从元素表，生产元素字典对象
        /// </summary>
        /// <param name="symbolDataTable">元素表</param>
        /// <param name="payDataTable">赔率表</param>
        public SlotsSymbols(DataTable symbolDataTable, DataTable payDataTable)
        {
            SlotsSymbolsDic = new Dictionary<int, SlotsSymbol>();
            int normalSymbolCount = 0;
            int specialSymbolCount = 0;
            foreach (DataRow dataRow in symbolDataTable.Rows)
            {
                int symbolId = Convert.ToInt32(dataRow[0].ToString());
                string symbolType;
                switch (dataRow["type"].ToString())
                {
                    case "1":
                        symbolType = "normal";
                        break;
                    case "2":
                        symbolType = "wild";
                        break;
                    case "4":
                        symbolType = "scatter";
                        break;
                    case "8":
                        symbolType = "bonus";
                        break;
                    case "16":
                        symbolType = "free";
                        break;
                    case "32":
                        symbolType = "collect";
                        break;
                    case "64":
                        symbolType = "custom";
                        break;
                    default:
                        throw (new SlotsTools.TempIsZeroException("指定元素类型不存在"));
                }//设置元素类型
                SlotsSymbol tempSlotsSymbol = new SlotsSymbol(symbolId, symbolType);
                SlotsSymbolsDic[symbolId] = tempSlotsSymbol;
                if (symbolType == "normal")
                {
                    normalSymbolCount++;
                }
                else
                {
                    specialSymbolCount++;
                }
            }
            foreach (DataRow dataRow in payDataTable.Rows)
            {
                int symbolId = Convert.ToInt32(dataRow["elem"].ToString());
                SlotsSymbolsDic[symbolId].SetSlotsSymbolPay( Convert.ToInt32(dataRow["num"].ToString()), Convert.ToDouble(dataRow["mult"].ToString()));
            }
        }


        /// <summary>
        /// 获取wild的赔率,wild本身如果没有赔率或赔率比1好元素小则输出1号元素的赔率
        /// </summary>
        /// <param name="symbolCount"></param>
        /// <returns></returns>
        public double GetWildPay(int symbolCount)
        {
            if (SlotsSymbolsDic[21].GetSymbolPay(symbolCount)>SlotsSymbolsDic[1].GetSymbolPay(symbolCount))
            {
                return SlotsSymbolsDic[21].GetSymbolPay(symbolCount);
            }
            return SlotsSymbolsDic[1].GetSymbolPay(symbolCount);
        }
    }
    
    
    /// <summary>
    /// 单个卷轴类,包含卷轴的截取方法    
    /// </summary>
    public class SlotsReel
    {
        public string name;
        public List<List<int>> Reel; //扩展前的卷轴
        public List<List<int>> ExpandReel; //扩展后的卷轴
        public double ReelWeight;//卷轴的权重
        public List<List<double>> PositionWeightList;//卷轴的位置号权重
        public readonly int ColumnsCount; //列数       
        public readonly List<int> RowsCount; //行数列表

        /// <summary>
        /// 卷轴构造函数
        /// </summary>
        /// <param name="reelDataTable">卷轴的DataTable</param>
        public SlotsReel(DataTable reelDataTable)
        {
            List<List<int>> reel = new List<List<int>>();
            int rowCount = reelDataTable.Rows.Count;
            int dataTableColumnsCount = reelDataTable.Columns.Count;
            RowsCount = new List<int>();
            ColumnsCount = dataTableColumnsCount-1;
            for (int i = 0; i < ColumnsCount; i++)
            {
                RowsCount.Add(0);
                reel.Add(new List<int>());
            }
            for (int i = 1; i < dataTableColumnsCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    if (reelDataTable.Rows[j][i].ToString() != "0" && reelDataTable.Rows[j][i] != DBNull.Value)
                    {
                        try
                        {
                            reel[i-1].Add(Convert.ToInt32(reelDataTable.Rows[j][i].ToString()));
                            RowsCount[i-1]++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("dataTable转换Reel失败，行数为" + j + ",列数为" + i + ";");
                            throw;
                        }
                    }
                }
                
                Reel = reel;
                ExpandReel = GetExpandReel();
            }
        }

        /// <summary>
        /// 给reel的位置号权重赋值
        /// </summary>
        /// <param name="reelDataTable"></param>
        public void SetPositionWeight(DataTable reelDataTable)
        {
            List<List<int>> reel = new List<List<int>>();
            for (int i = 1; i < ColumnsCount; i++)
            {
                for (int j = 0; j < RowsCount[i-1]; j++)
                {
                    try
                    {
                        PositionWeightList[i - 1].Add(Convert.ToInt32(reelDataTable.Rows[j][i].ToString()));
                        RowsCount[i - 1]++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("dataTable give Reel weight fail.失败，行数为" + j + ",列数为" + i + ";");
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// 输入卷轴位置号，和截取长度，输出元素矩阵
        /// </summary>
        /// <param name="position">位置号</param>
        /// <param name="interceptRowsCount">截取行数</param>
        /// <returns></returns>
        public List<List<int>> GetArray(List<int> position, int interceptRowsCount = 3)
        {
            List<List<int>> symbolArray = new List<List<int>>();
            for (int i = 0; i < position.Count; i++)
            {
                List<int> tempList = ExpandReel[i].GetRange(position[i]-interceptRowsCount+Reel[i].Count+1, interceptRowsCount);    //由于0号位置是最下面。所以这里需要把position调到最上面一个位置，并进行倒叙
                tempList.Reverse();
                symbolArray.Add(tempList);
            }

            return symbolArray;
        }
        
        /// <summary>
        /// 在卷轴内随机截取出矩阵
        /// </summary>
        /// <param name="interceptRowsCount">列高</param>
        /// <returns></returns>
        public List<List<int>> GetRandomArray(int interceptRowsCount = 3)
        {
            List<int> position = new List<int>();
            for (int i = 0; i < Reel.Count; i++)
            {
                position.Add(new Random().Next(0,Reel[i].Count));
            }
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
            List<List<int>> expandReel = SlotsTools.DepthClone(Reel);
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

        /// <summary>
        /// 构造函数
        /// </summary>
        public SlotsReels()
        {
            SlotsReelsDictionary = new Dictionary<string, SlotsReel>();
        }
        
        /// <summary>
        /// 用卷轴的dataTable构造
        /// </summary>
        /// <param name="name">卷轴名称</param>
        /// <param name="slotsReel">卷轴ob</param>
        public SlotsReels(string name,DataTable reelDataTable)
        {
            SlotsReelsDictionary = new Dictionary<string, SlotsReel>();
            SlotsReelsDictionary.Add(name,new SlotsReel(reelDataTable));
        }

        /// <summary>
        /// 将SlotsReel添加到SLotsReelsDictionary中
        /// </summary>
        /// <param name="reelDataTable">卷轴数据源</param>
        /// <param name="reelName">卷轴名称</param>
        /// <param name="reelType">卷轴类型</param>
        public void AddSlotsReel(string reelName,DataTable reelDataTable)
        {
            SlotsReelsDictionary[reelName] = new SlotsReel(reelDataTable);
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
        public List<List<int>> payLinesList; //中奖线列表 {{1,2,3}{1,2,3}}

        /// <summary>
        /// 用中奖线配置生成payLinesList
        /// </summary>
        /// <param name="winLineDataTable">中奖线dataTable</param>
        public PayLine(DataTable winLineDataTable)
        {
            payLinesList = GetLineConfig(winLineDataTable);
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
        /// 用配置的string获取单条中奖线列表
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        public List<int> GetOneLineList(string lineString)
        {
            List<int> outList = new List<int>();
            string[] arr = lineString.Split('|');
            foreach (string str in arr)
            {
                try
                {
                    string[] strings = str.Split(',');
                    int ints = Convert.ToInt32(strings[1]);
                    outList.Add(ints);
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