/*
 * SLots计算的基础类，包括最基础的计算中奖，输出中奖信息的逻辑
 * 一次spin的计算逻辑在这里,比如说一次basespin，一次小游戏
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using SlotsMath.Properties.FileMethod;

namespace SlotsMath.Properties.SlotsComputer
{
    /// <summary>
    /// 模拟结果的数据结构类
    /// </summary>
    public class SimulateDataInfo
    {
        public double NormalWinValue; //普通元素总中奖金额
        public int AddFreeTime; // 获得的Free次数
        public double ScatterWinValue;    //scatter元素的中奖金额
        public List<int> WinLineIndexList;    //有中奖的中奖线id
        public Dictionary<int,Dictionary<int,int>> NormalSymbolWinInfoDict; //有中奖的普通元素信息列表
        
        public SimulateDataInfo()
        {
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
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList.Clear();
            NormalSymbolWinInfoDict.Clear();
//            NormalSymbolWinInfoDict = new Dictionary<int, Dictionary<int, int>>();
//            List<int> dictKeys1List = new List<int>();
//            List<int> dictKeys2List = new List<int>();
//            dictKeys1List.AddRange(NormalSymbolWinInfoDict.Keys);
//            foreach (int key1 in dictKeys1List)
//            {
//                dictKeys2List.Clear();
//                dictKeys2List.AddRange(NormalSymbolWinInfoDict[key1].Keys);
//                foreach (int key2  in dictKeys2List)
//                {
//                    NormalSymbolWinInfoDict[key1][key2] = 0;
//                }
//            }
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
        public SimulateDataInfo SimulateDataInfoObject;
        public bool IsWithLine; //是否有winLine
        public double Bet;    //总下注金额    
        public int Row; //显示区域的行数            
        public int Columns; //显示区域的列数
        public double BaseWin; //baseGame赢数（1次下注）
        public double FreeWin; //freeGame赢数（1次下注）
        public double TotalWin; //总赢钱数（1次下注）
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
            //将元素装dao对应元素容器中
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
            Bet = Convert.ToDouble(BaseConfigDict["linebet"]);
            IsWithLine = isWithLine;
            Row = rowsCount;
            Columns = columnsCount;
            BaseWin = 0;
            FreeWin = 0;
            TotalWin = 0;
            //free元素和次数对应关系（这里需要改成读配置）
            FreeCountDict = new Dictionary<int, int>();
            FreeCountDict.Add(3,5);
            FreeCountDict.Add(4,8);
            FreeCountDict.Add(5,15);
            //Scatter元素和倍数对应关系(这里需要改成读配置)
            ScatterRewardDict = new Dictionary<int, double>();
            ScatterRewardDict.Add(3,5);
            ScatterRewardDict.Add(4,8);
            ScatterRewardDict.Add(5,15);
            LogName = logName;
            SimulateDataInfoObject = new SimulateDataInfo(NormalSymbolsList); //创建模拟时输出的数据
            #endregion
        }

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
        /// <param name="freeSymbolCount">free元素数量</param>
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
       /// 输出一条中奖线上元素的中奖信息(对第一列是wild无效)[0:元素id，1：中奖长度]
       /// </summary>
       /// <param name="oneLineSymbolIdList"></param>
       /// <returns>int[0:元素id，1：中奖长度]</returns>
        public int[] GetMaxWinNormalSymbolInfoWithLine(List<int> oneLineSymbolIdList)
        {
            int[] outList = {0, 0};//第一位是元素id，第二位是中奖长度数
            int listCount = oneLineSymbolIdList.Count;
            int maxWinSymbolId = 0;
            int maxWinSymbolCount = 0;
            if (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[0]].SymbolType == SymbolType.Wild)
            {
                //todo 这里没写
                return outList;
            }
            else
            {
                if (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[0]].SymbolType != SymbolType.Normal)
                {
                    return outList;
                } //当第一列是不为wild的特殊元素，不会普通元素中奖 
                else
                {
                    //当第一列不是21且第一列是普通元素
                    for (int i = 1; i < listCount; i++)
                    {
                        if (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[i]].SymbolType != SymbolType.Wild
                            && oneLineSymbolIdList[i] != oneLineSymbolIdList[0])
                        {
                            if (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[0]].SymbolPay.ContainsKey(i))
                            {
                                maxWinSymbolId = oneLineSymbolIdList[0];
                                maxWinSymbolCount = i;
                            }
                            break;
                        }
                        if (i == listCount-1 && (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[i]].SymbolType == SymbolType.Wild
                                                 || oneLineSymbolIdList[i] == oneLineSymbolIdList[0]))
                        {
                            if (SlotsSymbols.SlotsSymbolsDic[oneLineSymbolIdList[0]].SymbolPay.ContainsKey(listCount))
                            {
                                maxWinSymbolId = oneLineSymbolIdList[0];
                                maxWinSymbolCount = listCount;
                            }
                        }
                    }
                }//当第一列不是21且第一列是普通元素
            }
            outList[0] = maxWinSymbolId;
            outList[1] = maxWinSymbolCount;
            return outList;
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
                int[] tempWinList = GetMaxWinNormalSymbolInfoWithLine(oneLineSymbolList);//获取线上中奖元素和中奖长度
                if (tempWinList[0] > 0)
                {
                    totalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]);
                    TotalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]);
                    if (isSaveWinInfo)
                    {
                        SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                            .AddSymbolWinCount(tempWinList[1]);
                    } //如果需要则把中奖信息保存到symbol中
                    #region "把中奖信息保存到log文件"
                        string tempString = $"lineIndex:{PayLine.payLinesList.IndexOf(payLine)},";
                        tempString += $"symbolId:{tempWinList[0]},winValue:{tempWinList[1]};";
                        LogFile.SaveLog(LogName,tempString);
                    #endregion 
                }//如果中奖则计算中奖金额，并保存相关信息
            }
            if (totalWin == 0)
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
                }//获取线上的元素
                int[] tempWinList = GetMaxWinNormalSymbolInfoWithLine(oneLineSymbolList);//获取线上中奖元素和中奖长度
                if (tempWinList[0] > 0)
                {
                    totalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //本次spin的总中奖金额
                    TotalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //整个程序的总中奖金额
                    SimulateDataInfoObject.NormalWinValue+=SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //把普通元素的中奖数据装输出中
                    SimulateDataInfoObject.WinLineIndexList.Add(PayLine.payLinesList.IndexOf(payLine));
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinList[0],tempWinList[1],1);
                    if (isSaveWinInfo)
                    {
                        SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                            .AddSymbolWinCount(tempWinList[1]);
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
                int[] tempWinList = GetMaxWinNormalSymbolInfoWithLine(oneLineSymbolList);//获取线上中奖元素和中奖长度
                if (tempWinList[0] > 0)
                {
                    totalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //本次spin的总中奖金额
                    TotalWin += SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //整个程序的总中奖金额
                    SimulateDataInfoObject.NormalWinValue+=SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                        .GetSymbolPay(tempWinList[1]); //把普通元素的中奖数据装输出中
                    SimulateDataInfoObject.WinLineIndexList.Add(PayLine.payLinesList.IndexOf(payLine));
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinList[0],tempWinList[1],1);
                    if (isSaveWinInfo)
                    {
                        SlotsSymbols.SlotsSymbolsDic[tempWinList[0]]
                            .AddSymbolWinCount(tempWinList[1]);
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

        /// <summary>
        /// 全中奖线时获取特定普通元素的中奖信息（对第一列有wild的情况无效）
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="symbolId">元素id</param>
        /// <param name="isWildCanSub">此元素是否能被wild替代</param>
        /// <returns>中奖元素信息</returns>
        public WinSymbolInfo GetWinSymbolInfoByArrayWithoutLine(List<List<int>> symbolArray, int symbolId,
            bool isWildCanSub = true)
        {
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo(SlotsSymbols.SlotsSymbolsDic[symbolId],0,0);
            List<int> symbolCountInArray = new List<int>(); //每列特定元素的数量
            //获取每一列特定元素数量
            for (int i = 0; i < symbolArray.Count; i++)
            {
                int tempElementCount = SlotsTools.GetSymbolCountInList(symbolArray[i], symbolId);
                if (isWildCanSub)
                {
                    tempElementCount += SlotsTools.GetSymbolCountInList(symbolArray[i], 21);
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
                    tempWinSymbolInfo.SymbolsCount = i+1;
                    tempWinSymbolInfo.SymbolWinTime = i == 0
                        ? symbolCountInArray[0]
                        : tempWinSymbolInfo.SymbolWinTime * symbolCountInArray[i];
                }
            }
            return tempWinSymbolInfo;
        }
        
        /// <summary>
        /// 全线计算中奖金额
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <param name="isChangeWinCount"></param>
        /// <param name="isLogWinInformation"></param>
        /// <returns></returns>
        public double GetWinValueByArrayWithoutLine(List<List<int>> symbolArray,
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
                    GetWinSymbolInfoByArrayWithoutLine(symbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                totalWin += tempWin;   //本次spin的总中奖金额
                TotalWin += tempWin;  //全局的总奖励金额
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
            double totalWin = 0;
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();
            Dictionary<int, WinSymbolInfo> tempWinSymbolInfoDict = new Dictionary<int, WinSymbolInfo>();
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoByArrayWithoutLine(symbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                totalWin += tempWin;
                TotalWin += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }
            SimulateDataInfoObject.NormalWinValue = totalWin;
            //获取free次数
            SimulateDataInfoObject.AddFreeTime += GetFreeTime(symbolArray);
            //获取scatter奖励金额
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            return SimulateDataInfoObject;
        }
        
        /// <summary>
        /// 模拟单次baseSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public virtual SimulateDataInfo GetFreeSpinSimulateDateWithoutLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
        {
            SimulateDataInfoObject.Reset();
            double totalWin = 0;
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();
            Dictionary<int, WinSymbolInfo> tempWinSymbolInfoDict = new Dictionary<int, WinSymbolInfo>();
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoByArrayWithoutLine(symbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                totalWin += tempWin;
                TotalWin += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }
            SimulateDataInfoObject.NormalWinValue = totalWin;
            //获取free次数
            SimulateDataInfoObject.AddFreeTime += GetFreeTime(symbolArray);
            //获取scatter奖励金额
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            return SimulateDataInfoObject;
        }
        
    }
    
    
   
}