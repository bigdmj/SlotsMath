/*
 * SLots计算的基础类，包括最基础的计算中奖，输出中奖信息的逻辑
 * 一次spin的计算逻辑在这里,比如说一次basespin，一次小游戏
 * */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using SlotsMath.Properties.FileMethod;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath.Properties.SlotsComputer
{
    /// <summary>
    /// 模拟结果的数据结构类
    /// </summary>
    public class SimulateDataInfo
    {
        #region 公共属性定义
        public SpinType SpinType;
        public string ReelName;                        //卷轴名称
        public List<int> Position;                    //位置号
        public List<List<int>> SymbolArray;        //元素矩阵
        public int IsTriggerFree;    //是否触发了free
        public double NormalWinValue; //普通元素总中奖金额
        public int AddFreeTime; // 获得的Free次数
        public double ScatterWinValue;    //scatter元素的中奖金额
        public List<int> WinLineIndexList;    //有中奖的中奖线id
        public Dictionary<int,Dictionary<int,int>> NormalSymbolWinInfoDict; //有中奖的普通元素信息列表
        public Dictionary<int,Dictionary<int,int>> WildWinInfoDict;        //纯wild中奖数据
        #endregion
        
        public SimulateDataInfo()
        {
            SpinType = SpinType.BseSpin;
            ReelName = "";
            Position = new List<int>();
            SymbolArray = new List<List<int>>();
            IsTriggerFree = 0;
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList = new List<int>();
            NormalSymbolWinInfoDict = new Dictionary<int, Dictionary<int, int>>();
        }

        /// <summary>
        /// 用元素集合类来创建
        /// </summary>
        /// <param name="slotsSymbols"></param>
        public SimulateDataInfo(List<SlotsSymbol> slotsSymbols)
        {
            SpinType = SpinType.BseSpin;
            ReelName = "";
            Position = new List<int>();
            SymbolArray = new List<List<int>>(); 
            IsTriggerFree = 0;
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList = new List<int>();
            NormalSymbolWinInfoDict = new Dictionary<int, Dictionary<int, int>>();
            foreach (SlotsSymbol slotsSymbol in slotsSymbols)
            {
                NormalSymbolWinInfoDict.Add(slotsSymbol.SymbolId,new Dictionary<int, int>());
                foreach (int symbolCount in slotsSymbol.SymbolPay.Keys)
                {
                    NormalSymbolWinInfoDict[slotsSymbol.SymbolId].Add(symbolCount,0);
                }
            }
        }

        /// <summary>
        /// 将NormalSymbolWinInfo转化为字符串(只打印中奖的)
        /// </summary>
        /// <returns></returns>
        public string NormalSymbolWinInfoDictToString()
        {
            string outString = "";
            if (NormalSymbolWinInfoDict.Count == 0) return "[]";
            foreach (int fatherKey in NormalSymbolWinInfoDict.Keys)
            {
                outString += $"[{fatherKey}:";
                foreach (int sonKey in NormalSymbolWinInfoDict[fatherKey].Keys)
                {
                    outString += $"{sonKey},{NormalSymbolWinInfoDict[fatherKey][sonKey]};";
                }
                outString += "]";
            }
            return outString;
        }

        /// <summary>
        /// 将储存的数据重置
        /// </summary>
        public virtual void Reset()
        {
            SpinType = SpinType.BseSpin;
            ReelName = "";
            Position.Clear();
            SymbolArray.Clear();
            IsTriggerFree = 0;
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList.Clear();
            NormalSymbolWinInfoDict.Clear();
        }

        /// <summary>
        /// 普通元素的中奖次数添加到字典中来记录
        /// </summary>
        /// <param name="symbolId">元素id</param>
        /// <param name="count">中奖数量</param>
        /// <param name="winTime">中奖次数</param>
        public virtual void NormalSymbolWinInfoAddWinTime(int symbolId ,int count,int winTime)
        {
            if (!NormalSymbolWinInfoDict.ContainsKey(symbolId)) NormalSymbolWinInfoDict.Add(symbolId,new Dictionary<int, int>());
            if (!NormalSymbolWinInfoDict[symbolId].ContainsKey(count)) NormalSymbolWinInfoDict[symbolId].Add(count,winTime);
            NormalSymbolWinInfoDict[symbolId][count] += winTime;
        }
        
    }

    
    /// <summary>
    /// SlotsComputer父类，后续所有特殊玩法继承此类
    /// </summary>
    public  class SlotsComputer
    {
        #region 公共属性定义区
        public SlotsSymbols SlotsSymbols; //元素对象集合
        public SlotsReels BaseSlotsReels; //卷轴对象集合
        public SlotsReels FreeSlotsReels; //卷轴对象集合
        public PayLine PayLine; //中奖线对象
        public List<SlotsSymbol> NormalSymbolsList; //普通元素列表，在全线中只有这里面的元素会被查询普通中奖()
        public List<SlotsSymbol> WildSymbolsList; 
        public List<SlotsSymbol> ScatterSymbolsList;
        public List<SlotsSymbol> BonusSymbolsList;
        public List<SlotsSymbol> FreeSymbolsList;
        public List<SlotsSymbol> CollectSymbolsList;
        public List<SlotsSymbol> CustomSymbolsList;
        protected List<string> baseReelNameList;
        protected List<string> freeReelNameList;
        protected List<double> baseReelWeightList;
        protected List<double> freeReelWeightList;
        public SimulateDataInfo SimulateDataInfoObject;
        public bool IsWithLine; //是否有winLine
        public double Bet;    //总下注金额    
        public int Row; //显示区域的行数            
        public int Columns; //显示区域的列数
        public double TotalBaseWin; //baseGame总赢数
        public double TotalFreeWin; //freeGame总赢数
        public double TotalScatterWin; //特殊机制总赢数
        public double TotalWin; //总赢钱数
        public Dictionary<int, int> FreeCountDict;        //free元素数量和free次数对应关系字典
        public Dictionary<int, double> ScatterRewardDict;        //scatter元素数量和scatter奖励倍数字典
        public string LogName;
        public Dictionary<string, string> BaseConfigDict;    //储存base分页数据的字典
        #endregion
        
        /// <summary>
        /// slotsComputer构造函数
        /// </summary>
        /// <param name="dataTableDictionary">右excel转化来的DataTable字典</param>
        /// <param name="logName">日志名称(不带后缀)</param>
        /// <param name="rowsCount">显示区域行数</param>
        /// <param name="columnsCount">显示区域列数</param>
        /// <param name="isWithLine">是否有中奖线</param>
        public SlotsComputer(Dictionary<string, DataTable> dataTableDictionary ,string logName, int rowsCount = 3, int columnsCount = 5,
            bool isWithLine = true)
        {
            //生成baseConfigDict
            BaseConfigDict = new Dictionary<string, string>();
            foreach (DataRow dataRow in dataTableDictionary["base"].Rows)
            {
                BaseConfigDict.Add(dataRow[0].ToString(),dataRow[1].ToString());
            }
            //生成SlotsSymbols
            SlotsSymbols = new SlotsSymbols(dataTableDictionary["element"], dataTableDictionary["reward"]);
            //定义ReelNameList和权重list
            baseReelNameList = new List<string>();
            freeReelNameList = new List<string>();
            baseReelWeightList = new List<double>();
            freeReelWeightList = new List<double>();
            //生成SlotsReels(所有的basespin的卷轴都必须有这个关键字，且其他任何sheet不能用这个关键字；freespin卷轴亦然)
            BaseSlotsReels = new SlotsReels();
            FreeSlotsReels = new SlotsReels();
            foreach (string key in dataTableDictionary.Keys)
            {
                if (key.Contains("basespin"))
                {
                    BaseSlotsReels.AddSlotsReel(key, dataTableDictionary[key]);
                }

                if (key.Contains("freespin"))
                {
                    FreeSlotsReels.AddSlotsReel(key, dataTableDictionary[key]);
                }
            }
            //生成line配置
            if (isWithLine)
            {
                PayLine = new PayLine(dataTableDictionary["line"]);
            }
            
            #region 公共属性赋值
            //将元素装到对应的类型容器中
            NormalSymbolsList = new List<SlotsSymbol>();
            WildSymbolsList = new List<SlotsSymbol>();
            ScatterSymbolsList = new List<SlotsSymbol>();
            BonusSymbolsList = new List<SlotsSymbol>();
            FreeSymbolsList = new List<SlotsSymbol>();
            CollectSymbolsList = new List<SlotsSymbol>();
            CustomSymbolsList = new List<SlotsSymbol>();
            //将元素装入对应元素容器中
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
                    case SymbolType.Free:
                        FreeSymbolsList.Add(SlotsSymbols.SlotsSymbolsDic[key]); 
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
            Bet = Convert.ToDouble(BaseConfigDict["bet"]);
            IsWithLine = isWithLine;
            Row = rowsCount;
            Columns = columnsCount;
            TotalBaseWin = 0;
            TotalFreeWin = 0;
            TotalScatterWin = 0;
            TotalWin = 0;
            //free元素和次数对应关系
            FreeCountDict = new Dictionary<int, int>();
            FreeCountDict.Add(3,0);        //free元素和free回合数对应关系
            FreeCountDict.Add(4,0);       
            FreeCountDict.Add(5,0);
            //Scatter元素和倍数对应关系
            ScatterRewardDict = new Dictionary<int, double>();
            ScatterRewardDict.Add(3,0);
            ScatterRewardDict.Add(4,0);
            ScatterRewardDict.Add(5,0);
            LogName = logName;
            SimulateDataInfoObject = new SimulateDataInfo(NormalSymbolsList); //创建模拟时输出的数据
            #endregion
        }

        #region 修改赢钱金额函数，不要直接操作金额，防止重复价钱
        /// <summary>
        /// 增加baseGame的赢钱时用，主要用do调用,防止computer中加了太多次
        /// </summary>
        /// <param name="winValue"></param>
        public void AddBaseWin(double winValue)
        {
            TotalBaseWin += winValue;
        }

        /// <summary>
        /// 增加FreeGame的赢钱时用，主要用do调用,防止computer中加了太多次
        /// </summary>
        /// <param name="winValue"></param>
        public void AddFreeWin(double winValue)
        {
            TotalFreeWin += winValue;
        }

        /// <summary>
        /// 增加FeatureGame的赢钱时用，主要用do调用,防止computer中加了太多次
        /// </summary>
        /// <param name="winValue"></param>
        public void AddScatterWin(double winValue)
        {
            TotalScatterWin += winValue;
        }
        
        /// <summary>
        /// 增加FeatureGame的赢钱时用，主要用do调用,防止computer中加了太多次
        /// </summary>
        /// <param name="winValue"></param>
        public void AddTotalWin(double winValue)
        {
            TotalWin += winValue;
        }
        
        #endregion

        #region SET方法
        /// <summary>
        /// 设置ReelNameList 和 ReelWeightList
        /// </summary>
        protected void SetReelsWeight(Dictionary<string, DataTable> dataTableDictionary)
        {
            try
            {
                foreach (DataRow row in dataTableDictionary["baseweight"].Rows)
                {
                    BaseSlotsReels.SlotsReelsDictionary[row[0].ToString()].ReelWeight = Convert.ToDouble(row[1]);
                }
                foreach (DataRow row in dataTableDictionary["freeweight"].Rows)
                {
                    FreeSlotsReels.SlotsReelsDictionary[row[0].ToString()].ReelWeight = Convert.ToDouble(row[1]);
                }
            }
            catch (Exception e)
            {
                
                throw;
            }   
            foreach (string key in BaseSlotsReels.SlotsReelsDictionary.Keys)
            {
                baseReelNameList.Add(key);
                baseReelWeightList.Add(BaseSlotsReels.SlotsReelsDictionary[key].ReelWeight);
            }
            foreach (string key in FreeSlotsReels.SlotsReelsDictionary.Keys)
            {
                freeReelNameList.Add(key);
                freeReelWeightList.Add(FreeSlotsReels.SlotsReelsDictionary[key].ReelWeight);
            }
        }

        /// <summary>
        /// 设置元素位置号权重 todo 未完成
        /// </summary>
        public void SetReelsPositionWeight()
        {
        }
        #endregion

        #region GET方法

        /// <summary>
        /// 确定本次Spin用哪个卷轴
        /// </summary>
        /// <returns></returns>
        public virtual string GetBaseReelByRandom()
        {
            string outString = "";
            List<string> baseReelNames = new List<string>();
            foreach (string reelNames in BaseSlotsReels.SlotsReelsDictionary.Keys)
            {
                baseReelNames.Add(reelNames);
            }
            int i = new Random().Next(0,baseReelNames.Count);
            outString = baseReelNames[i];
            return outString;
        }
        
        /// <summary>
        /// 确定本次FreeSpin用哪个卷轴
        /// </summary>
        /// <returns></returns>
        public virtual string GetFreeReelByRandom()
        {
            string outString = "";
            List<string> freeReelNames = new List<string>();
            foreach (string reelNames in FreeSlotsReels.SlotsReelsDictionary.Keys)
            {
                freeReelNames.Add(reelNames);
            }
            int i = new Random().Next(0,freeReelNames.Count);
            outString = freeReelNames[i];
            return outString;
        }
        
        /// <summary>
        /// 确定本次spin的position
        /// </summary>
        /// <param name="slotsReel">slotsReel</param>
        /// <returns></returns>
        public virtual List<int> GetPositionByRandom(SlotsReel slotsReel)
        {
            List<int> position = new List<int>();
            for (int reelindex = 0;
                reelindex < slotsReel.Reel.Count;
                reelindex++)
            {
                position.Add(new Random().Next(0,
                    slotsReel.Reel[reelindex].Count));
            }
            return position;
        }

        /// <summary>
        /// 计算获取的free次数
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <returns></returns>
        public virtual int GetFreeTime(List<List<int>> symbolArray)
        {
            int outInt = 0;
            List<int> freeSymbolIdList = new List<int>();
            foreach (SlotsSymbol slotsSymbol in FreeSymbolsList)
            {
                freeSymbolIdList.Add(slotsSymbol.SymbolId);
            }
            int freeSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, freeSymbolIdList); //获取free元素的总数量
            if (FreeCountDict.Keys.Contains(freeSymbolCount))
            {
                outInt = FreeCountDict[freeSymbolCount];
            }
            else
            {
                outInt = freeSymbolCount > FreeCountDict.Keys.Max() ? FreeCountDict[FreeCountDict.Keys.Max()] : 0;
            }
            return outInt;
        }
        
        /// <summary>
        /// 计算获取的scatter奖励倍数
        /// </summary>
        /// <param name="scatterSymbolCount">scatter元素数量</param>
        /// <returns></returns>
        public virtual double GetScatterMultiple(int scatterSymbolCount)
        {
            double outInt = 0;
            if (ScatterRewardDict.Keys.Contains(scatterSymbolCount))
            {
                outInt = ScatterRewardDict[scatterSymbolCount];
            }
            else
            {
                outInt = scatterSymbolCount > ScatterRewardDict.Keys.Max() ? ScatterRewardDict[ScatterRewardDict.Keys.Max()] : 0;
            }
            return outInt;
        }

        /// <summary>
        /// 从元素容器中获取id列表
        /// </summary>
        /// <param name="slotsSymbols">元素容器</param>
        /// <returns></returns>
        public List<int> GetSymbolIdListFromContainer(List<SlotsSymbol> slotsSymbols)
        {
            List<int>  outList = new List<int>();
            for (int i = 0; i < slotsSymbols.Count; i++)
            {
                outList.Add(slotsSymbols[i].SymbolId);
            }
            return outList;
        }

        /// <summary>
        /// 通过winSymbolInfo获取赢钱金额,如果给予的info是wild的，会自动和1好元素比较奖励金额，输出大的那个
        /// </summary>
        /// <param name="winSymbolInfo">元素中奖信息</param>
        /// <returns></returns>
        public double GetWinValueByInfo(WinSymbolInfo winSymbolInfo)
        {
            double winValue = 0;
            if (winSymbolInfo.SlotsSymbol.SymbolId ==21)
            {
                winValue = SlotsSymbols.GetWildPay(winSymbolInfo.SymbolsCount) * winSymbolInfo.SymbolWinTime;
            }
            else
            {
                winValue = winSymbolInfo.SlotsSymbol.GetSymbolPay(winSymbolInfo.SymbolsCount) * winSymbolInfo.SymbolWinTime;
            }

            if (winSymbolInfo._3wildWinTime>0)
            {
                winValue += SlotsSymbols.GetWildPay(3) * winSymbolInfo._3wildWinTime;
            }
            if (winSymbolInfo._4wildWinTime>0)
            {
                winValue += SlotsSymbols.GetWildPay(4) * winSymbolInfo._4wildWinTime;
            }
            if (winSymbolInfo._5wildWinTime>0)
            {
                winValue += SlotsSymbols.GetWildPay(5) * winSymbolInfo._5wildWinTime;
            }
            return winValue;
        }

        /// <summary>
        /// 判断前3列是否都有wild
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <returns></returns>
        private bool Is3Wild(List<List<int>> symbolArray)
        {
            int firstLineWildCount = SlotsTools.GetSymbolCountInList(symbolArray[0], 21);
            int secLineWildCount = SlotsTools.GetSymbolCountInList(symbolArray[0], 21);
            int thrLineWildCount = SlotsTools.GetSymbolCountInList(symbolArray[0], 21);
            bool is3Wild = firstLineWildCount > 0 && secLineWildCount > 0 && thrLineWildCount > 0;
            return is3Wild;
        }
        
        #endregion

        /// <summary>
        /// 打印winSymbolInfo的信息
        /// </summary>
        /// <param name="winSymbolInfo"></param>
        public void PrintInfo(WinSymbolInfo winSymbolInfo)
        {
            Console.WriteLine($"id:{winSymbolInfo.SlotsSymbol.SymbolId},count:{winSymbolInfo.SymbolsCount},time:{winSymbolInfo.SymbolWinTime},3wild:{winSymbolInfo._3wildWinTime},4wild:{winSymbolInfo._4wildWinTime},5wild:{winSymbolInfo._5wildWinTime}");
        }

        #region 有线情况用的方法
            /// <summary>
           /// 输出一条中奖线上元素的中奖信息
           /// </summary>
           /// <param name="oneLineSymbolIdList"></param>
           /// <returns>WinSymbolInfo</returns>
            public virtual WinSymbolInfo GetMaxWinSymbolInfoWithLine(List<int> oneLineSymbolIdList)
            {
                WinSymbolInfo winSymbolInfo = new WinSymbolInfo(NormalSymbolsList[0],0,0);
                WinSymbolInfo tempSymbolInfo;
                int listCount = oneLineSymbolIdList.Count;
                int firstSymbolId = 0;    //第一个元素的id
                int firstSymbolCount = 0;    //第一个元素的长度
                int wildSymbolCount = 0;    //wild元素的长度
                double firstSymbolWin = 0;    //第一个元素的赢钱金额
                double wildSymbolWin = 0;    //wild元素的赢钱金额
                //如果所有元素都是wild，直接和1号元素比奖励
                if (oneLineSymbolIdList.Count == SlotsTools.GetSymbolCountInList(oneLineSymbolIdList,21))
                {
                    if (NormalSymbolsList[0].GetSymbolPay(oneLineSymbolIdList.Count)>WildSymbolsList[0].GetSymbolPay(oneLineSymbolIdList.Count))
                    {
                        winSymbolInfo = new WinSymbolInfo(NormalSymbolsList[0],oneLineSymbolIdList.Count,1);
                        return winSymbolInfo;
                    }
                    winSymbolInfo = new WinSymbolInfo(WildSymbolsList[0],oneLineSymbolIdList.Count,1);
                    return winSymbolInfo;
                }
                //非全都是wild
                for (int i = 0; i < listCount; i++)
                {
                    if (firstSymbolId == 0)
                    {
                        if (oneLineSymbolIdList[0] <21)
                        {
                            firstSymbolId = oneLineSymbolIdList[0];
                            firstSymbolCount++;
                        }
                        if (oneLineSymbolIdList[0] == 21)
                        {
                            firstSymbolCount++; 
                        }
                        if (oneLineSymbolIdList[0] > 21)
                        {
                            firstSymbolId = NormalSymbolsList[0].SymbolId;
                            break;
                        }
                    }
                    else
                    {
                        if (oneLineSymbolIdList[i] == firstSymbolId || oneLineSymbolIdList[i] == 21)
                        {
                            firstSymbolCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }//计算wild当普通元素算的最大中奖长度
                for (int i = 0; i < listCount; i++)
                {
                    if (oneLineSymbolIdList[i] !=21)
                    {
                        break;
                    }
                    else
                    {
                        wildSymbolCount++;
                    }
                }//计算wild的最大长度
                //计算纯wild中奖金额
                if (WildSymbolsList[0].SymbolPay.ContainsKey(wildSymbolCount))
                {
                    wildSymbolWin =
                        WildSymbolsList[0].GetSymbolPay(wildSymbolCount) >
                        NormalSymbolsList[0].GetSymbolPay(wildSymbolCount)
                            ? WildSymbolsList[0].GetSymbolPay(wildSymbolCount)
                            : NormalSymbolsList[0].GetSymbolPay(wildSymbolCount);
                }
                else
                {
                    wildSymbolWin = NormalSymbolsList[0].GetSymbolPay(wildSymbolCount);
                }
                //计算普通元素中奖金额
                firstSymbolWin = SlotsSymbols.SlotsSymbolsDic[firstSymbolId].GetSymbolPay(firstSymbolCount);
                //将值更大的一个奖励输出
                if (firstSymbolWin>0 && firstSymbolWin>wildSymbolWin)
                {
                    winSymbolInfo.SlotsSymbol = SlotsSymbols.SlotsSymbolsDic[firstSymbolId];
                    winSymbolInfo.SymbolsCount = firstSymbolCount;
                    winSymbolInfo.SymbolWinTime = 1;
                }
                else
                {
                    if (wildSymbolWin>firstSymbolWin)
                    {
                       winSymbolInfo.SlotsSymbol = SlotsSymbols.SlotsSymbolsDic[21];
                       winSymbolInfo.SymbolsCount = wildSymbolCount;
                       winSymbolInfo.SymbolWinTime = 1;
                    }
                }
                return winSymbolInfo;
            }
            
            /// <summary>
            /// 输入元素矩阵，输出中赢钱金额（有中奖线）
            /// </summary>
            /// <param name="symbolArray">实际使用的元素矩阵</param>
            /// <param name="isSaveWinInfo">如果为true，则会修改slotsSymbolDictionary中的中奖次数</param>
            /// <returns></returns>
            public double GetWinValueByArrayWithLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
            {
                double totalWin = 0;
                List<int> oneLineSymbolList = new List<int>();
                {
                    string tempString = "symbolArray:";
                    foreach (List<int> i in symbolArray)
                    {
                        tempString += SlotsTools.ListToString(i);
                    }
                    tempString += ";";
                    LogFile.SaveLog(LogName,tempString);
                } //把元素矩阵保存到log
                foreach (List<int> payLine in PayLine.payLinesList)
                {
                    oneLineSymbolList.Clear();
                    int index = 0;
                    foreach (int j in payLine)
                    {
                        oneLineSymbolList.Add(symbolArray[index][j]);
                        index++;
                    }//获取线上的元素
                    WinSymbolInfo winNormalSymbolInfoWithLine = GetMaxWinSymbolInfoWithLine(oneLineSymbolList);//获取线上中奖元素和中奖长度
                    if (winNormalSymbolInfoWithLine.SymbolWinTime > 0)
                    {
                        totalWin += winNormalSymbolInfoWithLine.SlotsSymbol
                            .GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount);
                        if (isSaveWinInfo)
                        {
                            winNormalSymbolInfoWithLine.SlotsSymbol
                                .AddSymbolWinCount(winNormalSymbolInfoWithLine.SymbolsCount);
                            #region "把中奖信息保存到log文件"
                            string tempString = $"lineIndex:{PayLine.payLinesList.IndexOf(payLine)},";
                            tempString += $"id:{winNormalSymbolInfoWithLine.SlotsSymbol.SymbolId},count:{winNormalSymbolInfoWithLine.SlotsSymbol.GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount)}";
                            LogFile.SaveLog(LogName,tempString);
                            #endregion
                        } //如果需要则把中奖信息保存到symbol中
                    }
                }
                if (totalWin <= 0)
                {
                    LogFile.SaveLog(LogName,"no win!!!");
                } //未中奖则打印未中奖
                return totalWin;
            }

            /// <summary>
            /// 模拟单次baseSpin，输出奖励数据
            /// </summary>
            /// <param name="symbolArray"></param>
            /// <param name="isSaveWinInfo"></param>
            /// <returns></returns>
            public virtual SimulateDataInfo GetBaseSpinSimulateDateWithLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
            {
                SimulateDataInfoObject.Reset();
                double totalWin = 0;
                List<int> oneLineSymbolList = new List<int>();
                foreach (List<int> payLine in PayLine.payLinesList)
                {
                    oneLineSymbolList.Clear();
                    int index = 0;
                    foreach (int j in payLine)
                    {
                        oneLineSymbolList.Add(symbolArray[index][j]);
                        index++;
                    } //获取线上的元素
                    WinSymbolInfo
                        winNormalSymbolInfoWithLine = GetMaxWinSymbolInfoWithLine(oneLineSymbolList); //获取线上中奖元素和中奖长度
                    if (winNormalSymbolInfoWithLine.SymbolsCount > 0)
                    {
                        totalWin += winNormalSymbolInfoWithLine.SlotsSymbol
                            .GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount); //本次spin的总中奖金额
                        SimulateDataInfoObject.NormalWinValue += winNormalSymbolInfoWithLine.SlotsSymbol
                            .GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount); //把普通元素的中奖数据装输出中
                        SimulateDataInfoObject.WinLineIndexList.Add(PayLine.payLinesList.IndexOf(payLine));
                        SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(
                            winNormalSymbolInfoWithLine.SlotsSymbol.SymbolId, winNormalSymbolInfoWithLine.SymbolsCount,
                            winNormalSymbolInfoWithLine.SymbolWinTime);
                        if (isSaveWinInfo)
                        {
                            winNormalSymbolInfoWithLine.SlotsSymbol
                                .AddSymbolWinCount(winNormalSymbolInfoWithLine.SymbolsCount, 1); //如果需要则把中奖信息保存到symbol中
                        } //如果中奖则计算中奖金额，并保存相关信息
                    } //普通元素中奖计算

                    //获取free次数
                    SimulateDataInfoObject.AddFreeTime += GetFreeTime(symbolArray);
                    //获取scatter奖励金额
                    int scatterSymbolCount =
                        SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
                    SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount) * Bet;
                }
                return SimulateDataInfoObject;
            }

            /// <summary>
            /// 模拟1次freespin，输出模拟结果
            /// </summary>
            /// <param name="symbolArray">元素矩阵</param>
            /// <param name="isSaveWinInfo">是否保存信息</param>
            /// <returns></returns>
            public virtual SimulateDataInfo GetFreeSpinSimulateDateWithLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
            {
                SimulateDataInfoObject.Reset();
                double totalWin = 0;
                List<int> oneLineSymbolList = new List<int>();
                foreach (List<int> payLine in PayLine.payLinesList)
                {
                    oneLineSymbolList.Clear();
                    int index = 0;
                    foreach (int j in payLine)
                    {
                        oneLineSymbolList.Add(symbolArray[index][j]);
                        index++;
                    }//获取线上的元素
                    WinSymbolInfo winNormalSymbolInfoWithLine = GetMaxWinSymbolInfoWithLine(oneLineSymbolList);//获取线上中奖元素和中奖长度
                    if (winNormalSymbolInfoWithLine.SymbolsCount > 0)
                    {
                        totalWin += winNormalSymbolInfoWithLine.SlotsSymbol
                            .GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount); //本次spin的总中奖金额
                        SimulateDataInfoObject.NormalWinValue+=winNormalSymbolInfoWithLine.SlotsSymbol
                            .GetSymbolPay(winNormalSymbolInfoWithLine.SymbolsCount); //把普通元素的中奖数据装输出中
                        SimulateDataInfoObject.WinLineIndexList.Add(PayLine.payLinesList.IndexOf(payLine));
                        SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(winNormalSymbolInfoWithLine.SlotsSymbol.SymbolId,winNormalSymbolInfoWithLine.SymbolsCount,1);
                        if (isSaveWinInfo)
                        {
                            winNormalSymbolInfoWithLine.SlotsSymbol
                                .AddSymbolWinCount(winNormalSymbolInfoWithLine.SymbolsCount);
                        } //如果需要则把中奖信息保存到symbol中
                        
                    }//如果中奖则计算中奖金额，并保存相关信息
                } //普通元素中奖计算
                
                //获取free次数
                SimulateDataInfoObject.AddFreeTime += GetFreeTime(symbolArray);
                //获取scatter奖励金额
                int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
                SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
                return SimulateDataInfoObject;
            }

        #endregion

        #region 无中奖线情况用的方法

        /// <summary>
         /// 全中奖线时获取特定普通元素的中奖信息-只有1号元素会考虑纯wild中奖(如果第四列或第五列全都不是普通元素或wild，会忽略掉纯3个wild或纯4个wild中奖)
         /// </summary>
         /// <param name="symbolArray">元素矩阵</param>
         /// <param name="symbolId">元素id</param>
         /// <returns>中奖元素信息</returns>
        public WinSymbolInfo GetWinSymbolInfoWithoutLine_skip3Wild(List<List<int>> symbolArray, int symbolId)
             {
                 WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo(SlotsSymbols.SlotsSymbolsDic[symbolId],0,0);
                 List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量+wild元素的数量
                 List<int> wildCountInArray = new List<int>(); //每列wild元素的数量
                 for (int i = 0; i < symbolArray.Count; i++)
                 {
                     int tempElementCount = SlotsTools.GetSymbolCountInList(symbolArray[i], symbolId);
                     int wildElementCount = SlotsTools.GetSymbolCountInList(symbolArray[i], 21);
                     symbolCountInArray.Add(tempElementCount+wildElementCount);
                     wildCountInArray.Add(wildElementCount);
                 }
                 int winCount = SlotsTools.GetSymbolIndexInList(symbolCountInArray, 0);
                 int winTime = 0;
                 int _3wildWinTime = 0;
                 int _4wildWinTime = 0;
                 int _5wildWinTime = 0;
                 int wildWinTime = wildCountInArray[0] * wildCountInArray[1] * wildCountInArray[2];
                 for (int i = 0; i < winCount; i++)
                 {
                     if (i == 0)
                     {
                         winTime = symbolCountInArray[i];
                     }
                     else
                     {
                         winTime *= symbolCountInArray[i];
                     }
                     if (i>2)
                     {
                         wildWinTime *= symbolCountInArray[i];
                     }
                 }//获取把wild全部当指定元素时的中奖次数
                 winTime -= wildWinTime;//前3个元素是wild的中奖这里减掉，后面单独计算加进去
                 if (wildCountInArray[0]>0 &&wildCountInArray[1]>0 &&wildCountInArray[2]>0)
                 {
                     //前3个元素为wild，第4个元素是指定元素
                     _3wildWinTime = wildCountInArray[0] * wildCountInArray[1] * wildCountInArray[2] *
                                     symbolCountInArray[3] - wildCountInArray[3] * symbolCountInArray[4];
                     //前4个元素为wild，第5个元素是指定元素
                     _4wildWinTime = wildCountInArray[0] * wildCountInArray[1] * wildCountInArray[2] *wildCountInArray[3]* 
                                     (symbolCountInArray[4] - wildCountInArray[4]);
                     
                     if (SlotsSymbols.GetWildPay(3)<SlotsSymbols.SlotsSymbolsDic[symbolId].GetSymbolPay(winCount))
                     {
                         winTime += _3wildWinTime;
                         _3wildWinTime = 0;
                     }
                     if (SlotsSymbols.GetWildPay(4)<SlotsSymbols.SlotsSymbolsDic[symbolId].GetSymbolPay(winCount))
                     {
                         winTime += _4wildWinTime;
                         _4wildWinTime = 0;
                     }
                     if (symbolId == 1)
                     {
                         //如果有5个wild，只会在计算1号元素时计算中奖
                         _5wildWinTime = wildCountInArray[0] * wildCountInArray[1] * wildCountInArray[2] *
                                                       wildCountInArray[3] * wildCountInArray[4];
                         if (SlotsSymbols.GetWildPay(5)<SlotsSymbols.SlotsSymbolsDic[symbolId].GetSymbolPay(winCount))
                         {
                             winTime += _5wildWinTime;
                             _5wildWinTime = 0;
                         }
                     }
                 }
                 tempWinSymbolInfo.SymbolsCount = winCount;
                 tempWinSymbolInfo.SymbolWinTime = winTime;
                 tempWinSymbolInfo._3wildWinTime = _3wildWinTime;
                 tempWinSymbolInfo._4wildWinTime = _4wildWinTime;
                 tempWinSymbolInfo._5wildWinTime = _5wildWinTime;
                 return tempWinSymbolInfo;
             }

        /// <summary>
        /// 全线计算普通中奖金额
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <param name="isChangeWinCount"></param>
        /// <param name="isLogWinInformation"></param>
        /// <returns></returns>
        public double GetNormalWinValueByArrayWithoutLine(List<List<int>> symbolArray,
            bool isChangeWinCount = true, bool isLogWinInformation = false)
        {
            double totalWin = 0;
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();
            if (isLogWinInformation)
            {
                string tempString = "";
                tempString += "the symbolArray:";
                foreach (List<int> symbolList in symbolArray)
                {
                    tempString += SlotsTools.ListToString(symbolList);
                }
                tempString += ";";
                LogFile.SaveLog(LogName,tempString);
            } //log symbolArray
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(symbolArray, normalSymbol.SymbolId);
                double tempWin = GetWinValueByInfo(tempWinSymbolInfo);
                totalWin += tempWin;   //本次spin的总中奖金额
                if (isChangeWinCount && tempWin >0)
                {
                    normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                } //slotsSymbol记录中奖信息
                if (isLogWinInformation && tempWin>0)
                {
                    string tempString = "";
                    tempString += "id:"+normalSymbol.SymbolId+",";
                    tempString += "winCount:"+tempWinSymbolInfo.SymbolsCount+",";
                    tempString += "winTime:"+tempWinSymbolInfo.SymbolWinTime+",";
                    tempString += "WinValue:"+tempWin+",";
                    LogFile.SaveLog(LogName,tempString);
                }//log 中奖元素的信息，包括id 中奖长度 中奖数 中奖金额
            } //计算普通元素的中奖金额
            if (isLogWinInformation)
            {
                string tempString = "";
                tempString += "totalWin:"+totalWin+";\n";
                LogFile.SaveLog(LogName,tempString);
            }//log 中奖金额
            return totalWin;
        }
        
        /// <summary>
        /// 模拟单次baseSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public virtual SimulateDataInfo GetBaseSpinSimulateDateWithoutLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();
            Dictionary<int, WinSymbolInfo> tempWinSymbolInfoDict = new Dictionary<int, WinSymbolInfo>();
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(symbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }//如果有普通元素中奖，会把普通元素中奖记录到simulateData的Dict重
            }
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            int addFreeTime = GetFreeTime(symbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.BseSpin;
            SimulateDataInfoObject.SymbolArray = symbolArray;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            #endregion
            return SimulateDataInfoObject;
        }
        
        /// <summary>
        /// 模拟单次baseSpin，输出奖励数据(全线),scatter是按照总下注的倍率返奖
        /// </summary>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public virtual SimulateDataInfo GetBaseSpinSimulateDateWithoutLine(bool isSaveWinInfo = false)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;        //普通元素的中奖金额
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();        //普通元素的中奖信息
            string baseReelName = GetBaseReelByRandom();    //单次模拟用到的baseReel名称
            List<int>  position = GetPositionByRandom(BaseSlotsReels.SlotsReelsDictionary[baseReelName]);
            List<List<int>> symbolArray = BaseSlotsReels.SlotsReelsDictionary[baseReelName].GetArray(position);
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(symbolArray, normalSymbol.SymbolId);
                double tempWin = GetWinValueByInfo(tempWinSymbolInfo);
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }//计算中奖
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            int addFreeTime = GetFreeTime(symbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.BseSpin;
            SimulateDataInfoObject.ReelName = baseReelName;
            SimulateDataInfoObject.Position = position;
            SimulateDataInfoObject.SymbolArray = symbolArray;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.AddFreeTime += GetFreeTime(symbolArray);
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            #endregion
            return SimulateDataInfoObject;
        }
        
        /// <summary>
        /// 模拟单次freeSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public virtual SimulateDataInfo GetFreeSpinSimulateDateWithoutLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();
            Dictionary<int, WinSymbolInfo> tempWinSymbolInfoDict = new Dictionary<int, WinSymbolInfo>();
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(symbolArray, normalSymbol.SymbolId);
                double tempWin = GetWinValueByInfo(tempWinSymbolInfo);
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            int addFreeTime = GetFreeTime(symbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.FreeSpin;
            SimulateDataInfoObject.SymbolArray = symbolArray;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.AddFreeTime +=addFreeTime;
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            #endregion
            return SimulateDataInfoObject;
        }
        
        /// <summary>
        /// 模拟单次freeSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public virtual SimulateDataInfo GetFreeSpinSimulateDateWithoutLine(bool isSaveWinInfo = false)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;        //普通元素的中奖金额
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();        //普通元素的中奖信息
            string freeReelName = GetFreeReelByRandom();    //单次模拟用到的baseReel名称
            List<int>  position = GetPositionByRandom(FreeSlotsReels.SlotsReelsDictionary[freeReelName]);
            List<List<int>> symbolArray = FreeSlotsReels.SlotsReelsDictionary[freeReelName].GetArray(position);
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(symbolArray, normalSymbol.SymbolId);
                double tempWin = GetWinValueByInfo(tempWinSymbolInfo);
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }//计算中奖
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            int addFreeTime = GetFreeTime(symbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.FreeSpin;
            SimulateDataInfoObject.ReelName = freeReelName;
            SimulateDataInfoObject.Position = position;
            SimulateDataInfoObject.SymbolArray = symbolArray;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.AddFreeTime += addFreeTime;
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            #endregion
            
            return SimulateDataInfoObject;
        }
        #endregion
    }
    
    
}