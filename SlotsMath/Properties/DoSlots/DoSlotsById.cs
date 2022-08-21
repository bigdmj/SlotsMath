/*
 * 执行逻辑，是遍历模拟还是快速次数模拟还是带日志的满数模拟在这里控制
 * 具体模拟逻辑也在这里
 */
using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using SlotsMath.Properties.FileMethod;
using SlotsMath.Properties.SlotsComputer;


namespace SlotsMath.Properties.SlotsMethod
{
    public enum SpinType
    {
        BseSpin,
        FreeSpin,
        MiniGame,
        Customer,
    }
    
    public class DoSlotsById
    {
        public string logName;//日志文件名
        public void Main()
        {
            bool isWithLine = true;
            int row = 3;
            int  columns = 5;
            double bet = 1; //每次下注金额
            double baseWin = 0;
            double freeWin = 0;
            double totalWin = 0;
            int simulateTime = 100; //模拟次数
            int logTime = 100;        //打印日志次数
            Dictionary<int,int> freeCountDict = new Dictionary<int, int>();
            freeCountDict.Add(3,5);
            freeCountDict.Add(4,8);
            freeCountDict.Add(5,15);
            Dictionary<int,double> scatterRewardDict = new Dictionary<int, double>();
            scatterRewardDict.Add(3,5);
            scatterRewardDict.Add(4,8);
            scatterRewardDict.Add(5,15);
            logName = "1001.txt";
            LogFile.ClearLog(logName); //清空日志
            //生成DataTable
            Dictionary<string, DataTable> dictionary =
                FileMethod.FileMethod.ExcelToDataTables(
                    Program.configPath+"10001_casino.xlsx");
            //生成SLots
            SlotsComputer.SlotsComputer slotsComputer = new SlotsComputer.SlotsComputer10001(dictionary,27,logName,3,5,true);
//            List<List<int>> symbolArray = new List<List<int>>();
//            List<int> positionList = new List<int>(){0,0,26,0,3};
//            symbolArray = slotsComputer.BaseSlotsReels.SlotsReelsDictionary["basespin1"].GetArray(positionList);
            //调用Slots
            totalWin = SimulateSlotsWithLine(slotsComputer, simulateTime, logTime);// 模拟slots
//            totalWin = SimulateSlotsWithoutLine(slotsComputer, simulateTime, logTime);// 模拟slots
            LogFile.SaveLog(logName,$"is OK,total Win :{totalWin}");
            Console.WriteLine($"is OK,total Win :{totalWin}");
        }

        /// <summary>
        /// 模拟特定次数的slots(有中奖线)
        /// todo 把free等特殊机制考虑入内
        /// </summary>
        /// <param name="slotsComputer"></param>
        /// <param name="simulateTime"></param>
        /// <param name="logTime"></param>
        /// <returns></returns>
        public virtual double SimulateSlotsWithLine(SlotsComputer.SlotsComputer slotsComputer,int simulateTime,int logTime)
        {
            int nowLogTime = 0;
            double totalWin = 0;
            int waitFreeTime = 0;
            SimulateDataInfo tempSimulateDataInfo = new SimulateDataInfo();
            for (int i = 0; i < simulateTime; i++,nowLogTime++)
            {
                List<List<int>> tempArray = slotsComputer.BaseSlotsReels.SlotsReelsDictionary["basespin1"].GetRandomArray();
                if (nowLogTime <= logTime)
                {
                    string tempString = "symbolArray:";
                    foreach (List<int> list in tempArray)
                    {
                        tempString += SlotsTools.ListToString(list);
                    }
                    tempString += ";";
                    LogFile.SaveLog(logName,tempString);
                }
                totalWin += slotsComputer.GetWinValueByArrayWithLine(tempArray, true, false);
                tempSimulateDataInfo = slotsComputer.GetBaseSpinSimulateDateWithLine(tempArray);
                waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                //打印本次spin日志
                if (nowLogTime < logTime)
                {
                    LogFile.SaveLog(logName, "finish oneTime Base\n");
                }
                //处理free 将来如果有其他特殊机制也是在这里处理
                while (waitFreeTime >0)
                {
                    LogFile.SaveLog(logName, "finish oneTime Free\n");
                    waitFreeTime--;
                }
                LogFile.SaveLog(logName,"\n");
            }
            return totalWin;
        }
        
        /// <summary>
        /// 模拟特定次数的slots（无中奖线）
        /// todo 把free等特殊机制考虑入内
        /// </summary>
        /// <param name="slotsComputer"></param>
        /// <param name="simulateTime"></param>
        /// <param name="logTime"></param>
        /// <returns></returns>
        public virtual double SimulateSlotsWithoutLine(SlotsComputer.SlotsComputer slotsComputer,int simulateTime,int logTime)
        {
            int nowLogTime = 0;
            double totalWin = 0;
            int waitFreeTime = 0;
            for (int i = 0; i < simulateTime; i++,nowLogTime++)
            {
                if (nowLogTime <= logTime) LogFile.SaveLog(logName, $"index:{i};");
                totalWin += slotsComputer.GetWinValueByArrayWithoutLine(
                    slotsComputer.BaseSlotsReels.SlotsReelsDictionary["basespin1"].GetRandomArray(), true,
                    nowLogTime <= logTime);
            }
            return totalWin;
        }
        
        /// <summary>
        /// 遍历卷轴，获取所有元素的中奖金额，在每个symbols中记录中奖次数
        /// </summary>
        /// <param name="reelName">卷轴名称</param>
        public virtual void ErgodicReel(SlotsComputer.SlotsComputer slotsComputer,string reelName)
        {
            SlotsReel slotsReel = slotsComputer.BaseSlotsReels.SlotsReelsDictionary[reelName];
            for (int i = 0; i < slotsReel.ColumnsCount; i++)
            {

            }

            //打印中奖次数信息
            //            foreach (var VARIABLE in SlotsSymbolDictionary)
            //            {
            //                
            //            }
        }
    }

}