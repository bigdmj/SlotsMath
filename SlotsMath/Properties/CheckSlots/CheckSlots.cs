/*
 * 这是校验log的基础类，其他slots只用重载Main函数即可
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SlotsMath.Properties.FileMethod;

namespace SlotsMath.Properties.CheckSlots
{
    public class CheckSlots
    {
        private string CheckLogName;
        private string LogName;
        private int stopAtTime;
        
        public virtual void Main()
        {
            LogName = "1001";
            stopAtTime = 10;
            //读取日志文件并转dataTable
            DataTable logDataTable = ExcelMethod.ExcelToDataTableByName("/Users/dmj/Documents/GitHub/SlotsMath/SlotsMath/log/GameRTPLog (3).xlsx", "Game1", true);
            Console.WriteLine("read log finish;");
            Dictionary<string, DataTable> dictionary =
                FileMethod.ExcelMethod.ExcelToDataTables(
                    Program.configPath+"1001.xlsx");
            SlotsComputer.SlotsComputer1001 slotsComputer = new SlotsComputer.SlotsComputer1001(dictionary,LogName,3,5,false);
            Console.WriteLine("create computer finish;");
            //逐行验证
            bool isWrong = false;
            string wrongInfo = "";
            int wrongCount = 0;
            foreach (DataRow row in logDataTable.Rows)
            {
                isWrong = false;
                wrongInfo = "";

                #region 读取表格数据，并根据array计算中奖
                    string roundId = row["RoundId"].ToString();
                    string subRoundId = row["SubRoundId"].ToString();
                    string type = row["Type"].ToString();
                    string typeKey = row["TypeKey"].ToString();
                    double wager = Convert.ToDouble(row["Wager"].ToString());
                    double win = Convert.ToDouble(row["win"].ToString());
                    int baseTriggerFree = Convert.ToInt32(row["BaseTriggerFree"].ToString()) ;
                    int freeTriggerFree = Convert.ToInt32(row["FreeTriggerFree"].ToString());
                    int trigger1_2 = Convert.ToInt32(row["trigger1_2"].ToString());
                    int trigger1_3 = Convert.ToInt32(row["trigger1_3"].ToString());
                    int trigger1_4 = Convert.ToInt32(row["trigger1_4"].ToString());
                    string positionString = row["ReelStopPositions"].ToString();
                    List<int> position = GetPositionListFromLog(positionString);
                    string symbolArrayString = row["SymbolArray"].ToString();
                    List<List<int>> symbolArray = GetSymbolArrayListFromLog(symbolArrayString);
                    int sameCount = 0;
                    if (trigger1_2 == 1)
                    {
                        sameCount = 2;
                    }
                    if (trigger1_2 == 1)
                    {
                        sameCount = 3;
                    }
                    if (trigger1_2 == 1)
                    {
                        sameCount = 4;
                    }
                    List<List<int>> mySymbolArray = slotsComputer.GetSameReeList(position, typeKey, sameCount);
                double myWinValue = slotsComputer.GetNormalWinValueByArrayWithoutLine(mySymbolArray, false);
                

                #endregion

                #region 校验数据
                    //校验typeKey
                    if (typeKey == "basespin1")
                    {
                        if (type != "BASE" || trigger1_2+trigger1_3+trigger1_4>0)
                        {
                            isWrong = true;
                            wrongInfo += "typeKey error;";
                        } 
                    }
                    if (typeKey == "basespin2")
                    {
                        if (type == "BASE" && trigger1_2+trigger1_3+trigger1_4==0)
                        {
                            isWrong = true;
                            wrongInfo += "typeKey error;";
                        } 
                    }
                    //校验wager
                    if (type == "BASE")
                    {
                        if (wager != 25)
                        {
                            isWrong = true;
                            wrongInfo += "wager error;";
                        }
                    }
                    if (type == "FREE")
                    {
                        if (wager != 0)
                        {
                            isWrong = true;
                            wrongInfo += "wager error;";
                        }
                    }
                    //校验symbolArray
                    if (SlotsTools.DoubleListToString(symbolArray)!= SlotsTools.DoubleListToString(mySymbolArray))
                    {
                        isWrong = true;
                        wrongInfo += $"symbol error,my symbol:{SlotsTools.DoubleListToString(mySymbolArray)};";
                    }
                    //校验win
                    if (win != myWinValue)
                    {
                        isWrong = true;
                        wrongInfo += $"win error,my value:{myWinValue};";
                    }
                    //校验BaseTriggerFree和FreeTriggerFree
                    int scatterCount = SlotsTools.GetSymbolCountByArray(mySymbolArray, 22);
                    if (scatterCount<3)
                    {
                        if (baseTriggerFree+freeTriggerFree>0)
                        {
                            isWrong = true;
                            wrongInfo += $"baseTriggerFree error,"; 
                        }
                    }
                    if (scatterCount>=3)
                    {
                        if (type =="BASE")
                        {
                            if (baseTriggerFree<1)
                            {
                                isWrong = true;
                                wrongInfo += $"baseTriggerFree error,";  
                            }
                        }
                        if (type =="FREE")
                        {
                            if (freeTriggerFree<1)
                            {
                                isWrong = true;
                                wrongInfo += $"freeTriggerFree error,";  
                            }
                        }
                    }
                    #endregion
                
                //显示错误信息
                if (isWrong)
                {
                    wrongCount++;
                    Console.WriteLine($"id:{roundId},subId:{subRoundId}--"+wrongInfo);
                    Console.WriteLine("==================");
                }

                if (wrongCount>stopAtTime)
                {
                    break;
                }
            }    //逐行校验数据

        }

        /// <summary>
        /// 从日志文件获取位置好列表
        /// </summary>
        /// <param name="positionString">日志中的位置号信息字符串</param>
        /// <returns></returns>
        protected List<int> GetPositionListFromLog(string positionString)
        {
            
            string newList =  positionString.Substring(1,positionString.Length-2);
            System.String[] listSplit = newList.Split(new[] {','});
            List<int> outList = new List<int>();
            foreach (string str in listSplit)
            {
                outList.Add(Convert.ToInt32(str));
            }
            return outList;
        }
        
        /// <summary>
        /// 从日志文件获取元素矩阵
        /// </summary>
        /// <param name="symbolArrayString">元素矩阵信息字符串</param>
        /// <returns></returns>
        protected List<List<int>> GetSymbolArrayListFromLog(string symbolArrayString)
        {
            
            string newList =  symbolArrayString.Substring(2,symbolArrayString.Length-4);
            System.String[] listSplit = newList.Split(new[] {'[',',',']'});
            List<int> tempList = new List<int>();
            List<List<int>> outList = new List<List<int>>();
            foreach (string str in listSplit)
            {
                if (str == "") continue;
                tempList.Add(Convert.ToInt32(str));
            }
            for (int i = 0; i < 5; i++)
            {
                List<int> tempList2 = new List<int>();
                for (int j = 0; j < 3; j++)
                {
                    tempList2.Add(tempList[i*3+j]);
                } 
                outList.Add(tempList2);
            }
            return outList;
        }
    }
}