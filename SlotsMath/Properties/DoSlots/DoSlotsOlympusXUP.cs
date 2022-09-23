using System;
using System.Collections.Generic;
using System.Data;
using NPOI.SS.Formula.Functions;
using SlotsMath.Properties.FileMethod;
using SlotsMath.Properties.SlotsComputer;

namespace SlotsMath.Properties.SlotsMethod
{
    public class DoSlotsOlympusXUP:DoSlotsById
    {

        public int TokenCount;
        
        public DoSlotsOlympusXUP(string id) : base(id)
        {
            
        }

        public override void Main(int simulateTime , int logTime)
        {
            #region 变量赋值
            IsWithLine = true;
            Row = 3;
            Columns = 5;
            Bet = 3; //每次下注金额
            BaseWin = 0;
            FreeWin = 0;
            TotalWin = 0;
            TokenCount = 0;
            SimulateTime = simulateTime; //模拟次数
            LogTime = logTime;        //打印日志次数
            #endregion
            
            LogFile.ClearLog(LogName); //清空日志
            //生成DataTable
            Dictionary<string, DataTable> dictionary =
                FileMethod.ExcelMethod.ExcelToDataTables(
                    Program.configPath+"OlympusXUP.xlsx");
            foreach (var key in dictionary.Keys)
            {
                DataTableMethod.DataToExcel(dictionary[key], LogName+key);
            }
            //生成SLots
            SlotsComputer_OlympusXUP slotsComputer = new SlotsComputer_OlympusXUP(dictionary,LogName,3,5,false);
            TotalWin = SimulateSlotsWithoutLine(slotsComputer, base.SimulateTime, logTime);// 模拟slots
            //输出数据
            LogFile.SaveLog(LogName,$"is OK,total Win :{TotalWin}");
            LogFile.SaveLog(LogName,$"baseWin :{BaseWin}");
            LogFile.SaveLog(LogName,$"freeWin :{FreeWin}");
            Console.WriteLine($"is OK,total Win :{TotalWin}");
            Console.WriteLine($"is OK,baseWin :{BaseWin}");
            Console.WriteLine($"is OK,freeWin :{FreeWin}");
        }
        
        /// <summary>
        /// 模拟特定次数的slots（无中奖线）
        /// </summary>
        /// <param name="slotsComputer"></param>
        /// <param name="simulateTime"></param>
        /// <param name="logTime"></param>
        /// <returns></returns>
        public new double SimulateSlotsWithoutLine(SlotsComputer.SlotsComputer_OlympusXUP slotsComputer,int simulateTime,int logTime)
        {
             int nowLogTime = 0;
            double totalWin = 0;
            int waitFreeTime = 0;
            SimulateDateInfoOlympusXUP tempSimulateDataInfo = new SimulateDateInfoOlympusXUP();
            
            #region 创建导出excel用的dataTable，并定义表头
            DataTable saveDataTable = new DataTable(); //将日志保存成dataTable，方便保存到excel中
            saveDataTable.Columns.Add("RoundId", typeof(int)); //模拟序号
            saveDataTable.Columns.Add("SubRoundId", typeof(int)); //spin序号，free时roundId不变，但是subId+1
            saveDataTable.Columns.Add("Type", typeof(string)); //spin类型
            saveDataTable.Columns.Add("TypeKey", typeof(string)); //使用的卷轴名称
            saveDataTable.Columns.Add("Level", typeof(int)); //记录重转的序号(有reSpin时用)
            saveDataTable.Columns.Add("ReelStopPositions", typeof(string)); //位置号列表
            saveDataTable.Columns.Add("OriSymbolArray", typeof(string)); //原始原始矩阵
            saveDataTable.Columns.Add("ExpandSymbolArray", typeof(string)); //扩展后矩阵列表
            saveDataTable.Columns.Add("basetriggerfree", typeof(string)); //base触发free时填1否则填0
            saveDataTable.Columns.Add("freeretrigger", typeof(string)); //free触发free时填1否则填0
            saveDataTable.Columns.Add("winSymbolInfo", typeof(string)); //中奖元素信息
            saveDataTable.Columns.Add("winValue", typeof(double)); //总赢钱金额
            saveDataTable.Columns.Add("Wager", typeof(double)); //下注金额
            saveDataTable.Columns.Add("Payoff", typeof(double)); //赢钱数-成本
            saveDataTable.Columns.Add("addToken", typeof(int)); //本次spin增加的token数
            saveDataTable.Columns.Add("finishToken", typeof(int)); //最终的token数
            #endregion
            
            for (int i = 0; i < simulateTime; i++,nowLogTime++)
            {
                if ((i+1)%Convert.ToInt32(simulateTime/100) == 0)
                {
                    Console.WriteLine($" i is {i},finish {(double) (i+1)/(double)simulateTime*100}%，time is {DateTime.Now}" );
                }  //实时显示进度
                int subRoundId = 0;//spin子序号id
                
                #region 完成一次base模拟
                //获取spin的reelName和position,元素矩阵
                tempSimulateDataInfo = slotsComputer.GetBaseSpinSimulateDateWithoutLine(true); //模拟base
                totalWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                BaseWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue;
                waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                #endregion
                
                if (nowLogTime <= logTime)
                {
                    string tempString = "symbolArray:";
                    foreach (List<int> list in tempSimulateDataInfo.SymbolArray)
                    {
                        tempString += SlotsTools.ListToString(list);
                    }
                    tempString += ";";
                    LogFile.SaveLog(LogName,tempString);
                } //输出日志
                //打印本次spin日志(txt 和 DataTable都有)
                if (nowLogTime < logTime)
                {
                    LogFile.SaveLog(LogName, "finish oneTime Base\n");
                    //将模拟数据导入到dataTable中方便输出
                    saveDataTable.Rows.Add(i, 0,"basespin",tempSimulateDataInfo.ReelName,0,SlotsTools.ListToString(tempSimulateDataInfo.Position),SlotsTools.DoubleListToString(tempSimulateDataInfo.SymbolArray),SlotsTools.DoubleListToString(tempSimulateDataInfo.ExpandSymbolArray),tempSimulateDataInfo.IsTriggerFree.ToString(),"0",tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue,slotsComputer.Bet,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue-slotsComputer.Bet,tempSimulateDataInfo.AddTokenCount,tempSimulateDataInfo.FinishSpinTokenCount);
                }
                
                //处理free 将来如果有其他特殊机制也是在这里处理
                #region 若有free则进行free模拟
                while (waitFreeTime >0)
                {
                    //获取spin的reelName和position,元素矩阵
                    tempSimulateDataInfo = slotsComputer.GetFreeSpinSimulateDateWithoutLine(true);//模拟free
                    totalWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                    FreeWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                    waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                    //打印本次spin日志(txt 和 DataTable都有)
                    if (nowLogTime < logTime)
                    {
                        LogFile.SaveLog(LogName, "finish oneTime Free\n");
                        //将模拟数据导入到dataTable中方便输出
                        saveDataTable.Rows.Add(i, subRoundId,"freespin",tempSimulateDataInfo.ReelName,0,SlotsTools.ListToString(tempSimulateDataInfo.Position),SlotsTools.DoubleListToString(tempSimulateDataInfo.SymbolArray),SlotsTools.DoubleListToString(tempSimulateDataInfo.ExpandSymbolArray),"0",tempSimulateDataInfo.IsTriggerFree.ToString(),tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue,slotsComputer.Bet,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue-slotsComputer.Bet,tempSimulateDataInfo.AddTokenCount,tempSimulateDataInfo.FinishSpinTokenCount);
                    }
                    waitFreeTime--;
                    if (waitFreeTime == 0)
                    {
                        slotsComputer.ResetNowTokenCount();
                    } //退出free时将token数重置
                    subRoundId++;
                }
                #endregion
                
                LogFile.SaveLog(LogName,"\n");
            } //进行模拟
            DataTableMethod.DataToExcel(saveDataTable, LogName);
            return totalWin;
        }
    }
}