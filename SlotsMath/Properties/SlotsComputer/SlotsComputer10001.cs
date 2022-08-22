using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SlotsMath.Properties.SlotsComputer
{
    public class SlotsComputer10001:SlotsComputer
    {
        public Dictionary<int, int> BonusCountDict;  
        public SlotsComputer10001(Dictionary<string, DataTable> dataTableDictionary, double bet, string logName = "10001", int rowsCount = 3, int columnsCount = 5, bool isWithLine = true) : base(dataTableDictionary, bet, logName, rowsCount, columnsCount, isWithLine)
        {
            BonusCountDict = new Dictionary<int, int>();
            BonusCountDict.Add(3,5);
            BonusCountDict.Add(4,8);
            BonusCountDict.Add(5,15);
        }

        /// <summary>
        /// 计算获取的free次数
        /// </summary>
        /// <param name="freeSymbolCount">free元素数量</param>
        /// <returns></returns>
        public override int GetFreeTime(List<List<int>> symbolArray)
        {
            int outInt = 0;
            List<int> bonusSymbolIdList = new List<int>();
            foreach (SlotsSymbol slotsSymbol in BonusSymbolsList)
            {
                bonusSymbolIdList.Add(slotsSymbol.SymbolId);
            }
            int freeSymbolCount = SlotsTools.GetSymbolsCountByArray(symbolArray, bonusSymbolIdList);
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
        /// 模拟单次baseSpin，输出奖励数据
        /// </summary>
        /// <param name="symbolArray"></param>
        /// <param name="isSaveWinInfo"></param>
        /// <returns></returns>
        public override SimulateDataInfo GetBaseSpinSimulateDateWithLine(List<List<int>> symbolArray, bool isSaveWinInfo = true)
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
    }
}