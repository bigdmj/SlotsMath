/*
 * 玩法说明：
 * 5*3，243路
 * wild会扩张到整列                                                                              --base和free都要改  --ok  
 * 在spin中每次出现2个scatter都会让奖励token数量+1,当token到达23个后，token数量不会再增长.              --需要添加一个变量token  --ok
 * 根据累积的token数量，freespin的所有奖励都会有对应的翻倍.                                            --需要添加一个方法，将所有free的奖励翻倍  --ok
 * 积累50个token会自动进入freegame                                                                --改do
 * 一屏出现3个或以上scatter会进入freegame，                                                        --改base
 * freegame中每出现2个或以上scatter元素会增加token并增加2次freegame                                  --改free --ok
 * 在basegame中scatter只会出现在2，3，4列
 * freegame中scatter只会出现在2，4列
 * 触发freegame时可以购买token，购买token的价格由已积攒的token数量决定
 * 完成freegame会重置token                                                                         --改do
 * token积攒数量和下注倍率绑定，每个下注数量都会储存一份token数量
 */


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SlotsMath.Properties.SlotsMethod;

namespace SlotsMath.Properties.SlotsComputer
{
    /// <summary>
    /// 模拟结果，添加了增加的token数量这一个字段
    /// </summary>
    public class SimulateDateInfoOlympusXUP : SimulateDataInfo
    {
        public double AddTokenCount;                //本次spin添加的token数
        public double FinishSpinTokenCount;        //spin结束时的token数
        public List<List<int>> ExpandSymbolArray;    //扩展后的元素矩阵
        
        public SimulateDateInfoOlympusXUP()
        {
            AddTokenCount = 0;
            FinishSpinTokenCount = 0;        //spin结束时的token数
            ExpandSymbolArray = new List<List<int>>();
        }        

        public SimulateDateInfoOlympusXUP(List<SlotsSymbol> slotsSymbols)
        {
            AddTokenCount = 0;
            FinishSpinTokenCount = 0;
            ExpandSymbolArray = new List<List<int>>();
        }

        public override void Reset()
        {
            SpinType = SpinType.BseSpin;
            ReelName = "";
            Position.Clear();
            SymbolArray.Clear();
            NormalWinValue = 0;
            AddFreeTime = 0;
            ScatterWinValue = 0;
            WinLineIndexList.Clear();
            NormalSymbolWinInfoDict.Clear();
            AddTokenCount = 0;
            FinishSpinTokenCount = 0;
            ExpandSymbolArray.Clear();
        }
    }
    
    
    public class SlotsComputer_OlympusXUP:SlotsComputer
    /*
     * 此类是给OlympusXUP用的，它的scatter元素会触发free，scatter本身也会有奖励
     */
    {
        SimulateDateInfoOlympusXUP SimulateDataInfoObject;
        int NowTokenCount;
        private Dictionary<int, int> TokenMult;
        
        private Dictionary<int, double> tokenMapMultDictionary;        //token数和奖励倍数对应关系
        
        public SlotsComputer_OlympusXUP(Dictionary<string, DataTable> dataTableDictionary, string logName, int rowsCount = 3, int columnsCount = 5, bool isWithLine = false) : base(dataTableDictionary, logName, rowsCount, columnsCount, isWithLine)
        {
            SimulateDataInfoObject = new SimulateDateInfoOlympusXUP(NormalSymbolsList);
            baseReelNameList = new List<string>();
            freeReelNameList = new List<string>();
            baseReelWeightList = new List<double>();
            freeReelWeightList = new List<double>();
            FreeCountDict.Add(3,8);        //free元素和free回合数对应关系
            FreeCountDict.Add(4,8);       
            FreeCountDict.Add(5,8);
            ScatterRewardDict.Add(3,6);    //scatter元素和scatter奖励倍数对应关系(这个游戏的scatter奖励不用乘bet)
            ScatterRewardDict.Add(4,6);
            ScatterRewardDict.Add(5,6);
            SetReelsWeight(dataTableDictionary);
        }

        /// <summary>
        /// 计算获取的free次数
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <returns></returns>
        public override int GetFreeTime(List<List<int>> symbolArray)
        {
            int outInt = 0;
            List<int> symbolIds = new List<int>();
            foreach (SlotsSymbol slotsSymbol in ScatterSymbolsList) //olympus是scatter触发free，这里改成了统计scatter元素的数量
            {
                symbolIds.Add(slotsSymbol.SymbolId);
            }

            int symbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, symbolIds); //获取free元素的总数量
            if (FreeCountDict.Keys.Contains(symbolCount))
            {
                outInt = FreeCountDict[symbolCount];
            }
            else
            {
                outInt = 0;
            }
            return outInt;
        }

        /// <summary>
        /// 重置token数量
        /// </summary>
        public void ResetNowTokenCount()
        {
            NowTokenCount = 0;
        }

        /// <summary>
        ///根据token的数量获得freeGame的翻倍数
        /// </summary>
        /// <returns></returns>
        public double GetTokenWinMul(int tokenCount)
        {
            if (tokenMapMultDictionary.ContainsKey(tokenCount))
            {
                return tokenMapMultDictionary[tokenCount];
            }
            throw (new SlotsTools.TempIsZeroException("token count is bigger or simmler"));
        }
        
        /// <summary>
        /// 确定本次Spin用哪个卷轴
        /// </summary>
        /// <returns></returns>
        public new string GetBaseReelByRandom()
        {
            string outString = SlotsTools.GetRandomObjByWeight(baseReelNameList, baseReelWeightList);
            return outString;
        }
        
        /// <summary>
        /// 模拟单次baseSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public new SimulateDateInfoOlympusXUP GetBaseSpinSimulateDateWithoutLine(bool isSaveWinInfo = true)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;        //普通元素的中奖金额
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();        //普通元素的中奖信息
            string baseReelName = GetBaseReelByRandom();    //单次模拟用到的baseReel名称
            List<int>  position = GetPositionByRandom(BaseSlotsReels.SlotsReelsDictionary[baseReelName]);
            List<List<int>> oriSymbolArray = BaseSlotsReels.SlotsReelsDictionary[baseReelName].GetArray(position);
            List<List<int>> expandSymbolArray = SlotsTools.GetExpandSymbolArray(oriSymbolArray,WildSymbolsList[0].SymbolId);//wild元素扩展到整列
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(expandSymbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime;
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            }//计算普通元素中奖
            //todo 计算纯wild中奖
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(expandSymbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            int addToken =  (int)Math.Floor((double) scatterSymbolCount/(double)2);
            NowTokenCount = new List<int>() {NowTokenCount + addToken, 13}.Min();
            int addFreeTime = GetFreeTime(expandSymbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.BseSpin;
            SimulateDataInfoObject.ReelName = baseReelName;
            SimulateDataInfoObject.Position = position;
            SimulateDataInfoObject.SymbolArray = oriSymbolArray;
            SimulateDataInfoObject.ExpandSymbolArray = expandSymbolArray;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.AddFreeTime += addFreeTime;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            SimulateDataInfoObject.AddTokenCount = addToken;
            SimulateDataInfoObject.FinishSpinTokenCount = NowTokenCount;
            #endregion
            
            return SimulateDataInfoObject;
        }

        /// <summary>
        /// 模拟单次freeSpin，输出奖励数据(全线)
        /// </summary>
        /// <param name="symbolArray">元素矩阵</param>
        /// <param name="tokenCount">token的数量</param>
        /// <param name="isSaveWinInfo">是否把中奖信息保存到slotsComputer的slotsSymbol中</param>
        /// <returns></returns>
        public new SimulateDateInfoOlympusXUP GetFreeSpinSimulateDateWithoutLine(bool isSaveWinInfo = true)
        {
            SimulateDataInfoObject.Reset();
            double normalWinValue = 0;        //普通元素的中奖金额
            WinSymbolInfo tempWinSymbolInfo = new WinSymbolInfo();        //普通元素的中奖信息
            string freeReelName = GetFreeReelByRandom();    //单次模拟用到的baseReel名称
            List<int>  position = GetPositionByRandom(FreeSlotsReels.SlotsReelsDictionary[freeReelName]);
            List<List<int>> oriSymbolArray = FreeSlotsReels.SlotsReelsDictionary[freeReelName].GetArray(position);
            List<List<int>> expandSymbolArray = SlotsTools.GetExpandSymbolArray(oriSymbolArray,WildSymbolsList[0].SymbolId);//wild元素扩展到整列
            int scatterSymbolCount = SlotsTools.GetSymbolsCountByArray(expandSymbolArray, GetSymbolIdListFromContainer(ScatterSymbolsList));
            //获取本次spin会增加的token数量，由于是先计算token数量再结算奖励，所以本次spin计算翻倍数需要把本轮获得的token考虑进去
            int addToken =  (int)Math.Floor((double) scatterSymbolCount/(double)2);
            NowTokenCount = new List<int>() {NowTokenCount + addToken, 13}.Min();
            double winMul = GetTokenWinMul(NowTokenCount);
            foreach (SlotsSymbol normalSymbol in NormalSymbolsList)
            {
                tempWinSymbolInfo =
                    GetWinSymbolInfoWithoutLine_skip3Wild(expandSymbolArray, normalSymbol.SymbolId);
                double tempWin = normalSymbol.GetSymbolPay(tempWinSymbolInfo.SymbolsCount) * tempWinSymbolInfo.SymbolWinTime*winMul;
                normalWinValue += tempWin;
                if (tempWin >0)
                {
                    if (isSaveWinInfo)  normalSymbol.AddSymbolWinCount(tempWinSymbolInfo.SymbolsCount, tempWinSymbolInfo.SymbolWinTime);
                    SimulateDataInfoObject.NormalSymbolWinInfoAddWinTime(tempWinSymbolInfo.SlotsSymbol.SymbolId,tempWinSymbolInfo.SymbolsCount,tempWinSymbolInfo.SymbolWinTime);
                }
            } //计算普通元素中奖
            int addFreeTime = GetFreeTime(expandSymbolArray);
            #region 记录信息
            SimulateDataInfoObject.SpinType = SpinType.BseSpin;
            SimulateDataInfoObject.ReelName = freeReelName;
            SimulateDataInfoObject.Position = position;
            SimulateDataInfoObject.SymbolArray = oriSymbolArray;
            SimulateDataInfoObject.ExpandSymbolArray = expandSymbolArray;
            SimulateDataInfoObject.NormalWinValue = normalWinValue;
            SimulateDataInfoObject.AddFreeTime += addFreeTime;
            SimulateDataInfoObject.IsTriggerFree = addFreeTime > 0 ? 1 : 0;
            SimulateDataInfoObject.ScatterWinValue += GetScatterMultiple(scatterSymbolCount)*Bet;
            SimulateDataInfoObject.AddTokenCount = addToken;
            SimulateDataInfoObject.FinishSpinTokenCount = NowTokenCount;
            #endregion
            return SimulateDataInfoObject;
        }
    }
}
