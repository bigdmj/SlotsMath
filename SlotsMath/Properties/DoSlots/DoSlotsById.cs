/*
 * 执行逻辑，是遍历模拟还是快速次数模拟还是带日志的满数模拟在这里控制
 * 多次模拟的特殊规则逻辑在这里，比如说添加free次数，进入小游戏等
 * 具体模拟逻辑也在这里
 */
using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using SlotsMath.Properties.FileMethod;
using SlotsMath.Properties.SlotsComputer;

/// <summary>
/// spin的类型枚举
/// </summary>


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
        public bool IsWithLine;
        public int Row ;
        public int  Columns;
        public double Bet ; //每次下注金额
        public double BaseWin;
        public double FreeWin;
        public double TotalWin;
        public int SimulateTime; //模拟次数
        public int LogTime ;        //打印日志次数
        public DoSlotsById(string id)
        {
            LogName = id.ToString();
        }

        public string LogName;//日志文件名
        
        /// <summary>
        /// 模拟主程序
        /// </summary>
        /// <param name="SimulateTime">模拟次数</param>
        /// <param name="logTime">打印次数</param>
        public virtual void Main(int simulateTime , int logTime)
        {
            IsWithLine = true;
            Row = 3;
            Columns = 5;
            Bet = 1; //每次下注金额
            BaseWin = 0;
            FreeWin = 0;
            TotalWin = 0;
            SimulateTime = simulateTime; //模拟次数
            LogTime = logTime;        //打印日志次数

            #region 记录free元素和free次数对应关系
            Dictionary<int,int> freeCountDict = new Dictionary<int, int>();
            freeCountDict.Add(3,5);
            freeCountDict.Add(4,8);
            freeCountDict.Add(5,15);
            #endregion

            #region 记录scatter元素和scatter倍数对应关系
            Dictionary<int,double> scatterRewardDict = new Dictionary<int, double>();
            scatterRewardDict.Add(3,5);
            scatterRewardDict.Add(4,8);
            scatterRewardDict.Add(5,15);
            

            #endregion
            
            LogFile.ClearLog(LogName); //清空日志
            //生成DataTable
            Dictionary<string, DataTable> dictionary =
                FileMethod.ExcelMethod.ExcelToDataTables(
                    Program.configPath+"test.xlsx");
            //生成SLots
            SlotsComputer.SlotsComputer slotsComputer = new SlotsComputer.SlotsComputerTest(dictionary,LogName,3,5,false);
//            List<List<int>> symbols = slotsComputer.BaseSlotsReels.SlotsReelsDictionary["basespin1"].GetArray(new List<int>(){1,2,2,9,1});
//            foreach (var VARIABLE in symbols)
//            {
//                Console.Write(SlotsTools.ListToString(VARIABLE));
//            }
//
//            WinSymbolInfo winSymbolInfo = slotsComputer.GetWinSymbolInfoWithoutLine_skip3Wild(symbols,1);
//            double winValue = slotsComputer.GetWinValueByInfo(winSymbolInfo);
//            slotsComputer.PrintInfo(winSymbolInfo);
//            Console.WriteLine($"winValue:{winValue}");
            //            totalWin = SimulateSlotsWithLine(slotsComputer, simulateTime, logTime);// 模拟slots
            TotalWin = SimulateSlotsWithoutLine(slotsComputer, this.SimulateTime, logTime);// 模拟slots
            LogFile.SaveLog(LogName,$"is OK,total Win :{TotalWin}");
            Console.WriteLine($"is OK,total Win :{TotalWin}");
//            List<string> stringList = new List<string>(){"A3.1","B5.2","C7.3","C9.5","C12.5"};
//            List<double> weightList = new List<double>(){3.1,5.2,7.3,9.5,12.5};
//            List<int> resultList = new List<int>(){0,0,0,0,0};
//            int Rtime = 1000000;
//            while (Rtime>0)
//            {
//                string a = SlotsTools.GetRandomObjByWeight(stringList, weightList);
//                Console.WriteLine(a);
//                resultList[stringList.IndexOf(a)] += 1;
//                Rtime--;
//            }
//            Console.WriteLine($"random time is {resultList.Count}");

        }
        
        
        /// <summary>
        /// 模拟特定次数的slots(有中奖线)
        /// </summary>
        /// <param name="slotsComputer">slotsComputer规则实体</param>
        /// <param name="simulateTime">模拟次数</param>
        /// <param name="logTime">日志打印次数</param>
        /// <returns></returns>
        public virtual double SimulateSlotsWithLine(SlotsComputer.SlotsComputer slotsComputer,int simulateTime,int logTime)
        {
            int nowLogTime = 0;
            double totalWin = 0;
            int waitFreeTime = 0;
            int freeIndex = 0;
            SimulateDataInfo tempSimulateDataInfo = new SimulateDataInfo();
            
            #region 创建导出excel用的dataTable，并定义表头
            DataTable saveDataTable = new DataTable(); //将日志保存成dataTable，方便保存到excel中
            saveDataTable.Columns.Add("id", typeof(int)); //模拟序号
            saveDataTable.Columns.Add("index", typeof(int)); //spin序号
            saveDataTable.Columns.Add("spinType", typeof(string)); //spin类型
            saveDataTable.Columns.Add("positon", typeof(string)); //位置号列表
            saveDataTable.Columns.Add("Bet", typeof(double)); //下注金额
            saveDataTable.Columns.Add("isWin", typeof(int)); //是否有赢钱
            saveDataTable.Columns.Add("lineIndex", typeof(string)); //有中奖的中奖线列表
            saveDataTable.Columns.Add("winSymbolInfo", typeof(string)); //中奖元素信息
            saveDataTable.Columns.Add("winValue", typeof(double)); //总赢钱金额
            #endregion
            
            List<int> position = new List<int>(); //位置号列表
            List<List<int>> tempArray = new List<List<int>>(); //元素矩阵
            for (int i = 0; i < simulateTime; i++,nowLogTime++)
            {
                //实时显示进度
                if ((i+1)%Convert.ToInt32(simulateTime/100) == 0)
                {
                    Console.WriteLine($" i is {i},finish {(double) (i+1)/(double)simulateTime*100}%，time is {DateTime.Now}" );
                }
                position.Clear();
                tempArray.Clear();
                freeIndex = 0;

                #region 完成一次base模拟
                //获取spin的reelName和position,元素矩阵
                string baseReelName = slotsComputer.GetBaseReelByRandom();    //单次模拟用到的baseReel名称
                position = slotsComputer.GetPositionByRandom(
                    slotsComputer.BaseSlotsReels.SlotsReelsDictionary[baseReelName]);
                tempArray = slotsComputer.BaseSlotsReels.SlotsReelsDictionary[baseReelName].GetArray(position);
                tempSimulateDataInfo = slotsComputer.GetBaseSpinSimulateDateWithLine(tempArray,true); //模拟base
                totalWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                #endregion
                
                if (nowLogTime <= logTime)
                {
                    string tempString = "symbolArray:";
                    foreach (List<int> list in tempArray)
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
                    saveDataTable.Rows.Add(i, 0,"basespin",SlotsTools.ListToString(position),slotsComputer.Bet,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue>0?1:0,SlotsTools.ListToString(tempSimulateDataInfo.WinLineIndexList),tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue);
                }

                #region 如果有free则执行free
                //处理free 将来如果有其他特殊机制也是在这里处理
                while (waitFreeTime >0)
                {
                    position.Clear();
                    tempArray.Clear();
                    //获取spin的reelName和position,元素矩阵
                    string freeReelName = slotsComputer.GetFreeReelByRandom();    //单次模拟用到的freeReel名称
                    position = slotsComputer.GetPositionByRandom(
                        slotsComputer.FreeSlotsReels.SlotsReelsDictionary[freeReelName]);
                    tempArray = slotsComputer.FreeSlotsReels.SlotsReelsDictionary[freeReelName].GetArray(position);
                    tempSimulateDataInfo = slotsComputer.GetFreeSpinSimulateDateWithLine(tempArray, true);//模拟free
                    totalWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                    waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                    //打印本次spin日志(txt 和 DataTable都有)
                    if (nowLogTime < logTime)
                    {
                        LogFile.SaveLog(LogName, "finish oneTime Free\n");
                        //将模拟数据导入到dataTable中方便输出
                        saveDataTable.Rows.Add(i, freeIndex,"freespin",SlotsTools.ListToString(position),0,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue>0?1:0,SlotsTools.ListToString(tempSimulateDataInfo.WinLineIndexList),tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue);
                    }
                    waitFreeTime--;
                    freeIndex++;
                }
                

                #endregion
                
                LogFile.SaveLog(LogName,"\n");
            } //进行模拟
            DataTableMethod.DataToExcel(saveDataTable, LogName);
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
            SimulateDataInfo tempSimulateDataInfo = new SimulateDataInfo();
            
            #region 创建导出excel用的dataTable，并定义表头
            DataTable saveDataTable = new DataTable(); //将日志保存成dataTable，方便保存到excel中
            saveDataTable.Columns.Add("id", typeof(int)); //模拟序号
            saveDataTable.Columns.Add("index", typeof(int)); //spin序号
            saveDataTable.Columns.Add("spinType", typeof(string)); //spin类型
            saveDataTable.Columns.Add("positon", typeof(string)); //位置号列表
            saveDataTable.Columns.Add("Bet", typeof(double)); //下注金额
            saveDataTable.Columns.Add("isWin", typeof(int)); //是否有赢钱
            saveDataTable.Columns.Add("winSymbolInfo", typeof(string)); //中奖元素信息
            saveDataTable.Columns.Add("winValue", typeof(double)); //总赢钱金额
            #endregion
            
            for (int i = 0; i < simulateTime; i++,nowLogTime++)
            {
                if ((i+1)%Convert.ToInt32(simulateTime/100) == 0)
                {
                    Console.WriteLine($" i is {i},finish {(double) (i+1)/(double)simulateTime*100}%，time is {DateTime.Now}" );
                }  //实时显示进度
                int freeIndex = 0;
                
                #region 完成一次base模拟
                //获取spin的reelName和position,元素矩阵
                tempSimulateDataInfo = slotsComputer.GetBaseSpinSimulateDateWithoutLine(true); //模拟base
                slotsComputer.AddBaseWin(tempSimulateDataInfo.NormalWinValue);
                slotsComputer.AddScatterWin(tempSimulateDataInfo.ScatterWinValue);
                slotsComputer.AddTotalWin(tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue); //将总赢钱数进行计算
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
                    saveDataTable.Rows.Add(i, 0,"basespin",SlotsTools.ListToString(tempSimulateDataInfo.Position),slotsComputer.Bet,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue>0?1:0,tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue);
                }
                //处理free 将来如果有其他特殊机制也是在这里处理

                #region 若有free则进行free模拟
                while (waitFreeTime >0)
                {
                    tempSimulateDataInfo = slotsComputer.GetFreeSpinSimulateDateWithoutLine(true);//模拟free
                    totalWin += tempSimulateDataInfo.NormalWinValue + tempSimulateDataInfo.ScatterWinValue; //将总赢钱数进行计算
                    waitFreeTime += tempSimulateDataInfo.AddFreeTime;
                    //打印本次spin日志(txt 和 DataTable都有)
                    if (nowLogTime < logTime)
                    {
                        LogFile.SaveLog(LogName, "finish oneTime Free\n");
                        //将模拟数据导入到dataTable中方便输出
                        saveDataTable.Rows.Add(i, freeIndex,"freespin",SlotsTools.ListToString(tempSimulateDataInfo.Position),0,tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue>0?1:0,tempSimulateDataInfo.NormalSymbolWinInfoDictToString(),tempSimulateDataInfo.NormalWinValue+tempSimulateDataInfo.ScatterWinValue);
                    }
                    waitFreeTime--;
                    freeIndex++;
                }
                #endregion
                
                LogFile.SaveLog(LogName,"\n");
            } //进行模拟
            DataTableMethod.DataToExcel(saveDataTable, LogName);
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