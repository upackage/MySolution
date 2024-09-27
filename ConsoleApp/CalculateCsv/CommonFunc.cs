using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static CalculateCsv.Business;

namespace CalculateCsv
{
    internal class CommonFunc
    {
        public static Dictionary<int, string> CalLastResult(decimal model, Dictionary<int, string> dic, Dictionary<int, int> dic_Judge)
        {
            int readStep = readRange + 1;
            int rangeStart = 0;
            int rangeEnd = 0;

            int doWorkValue = 0;           //X
            int judgeValue = 0;            //X*(1+Y%)
            int targetValue = 0;           //X*(1+Y%)*(1+Y%)
            int oppositeTargetValue = 0;   //X*(1+Y%)*(1-Y%)
            for (int i = startIndex; i < prices_New.Count; i += readStep)
            {
                doWorkValue = prices_New[i];
                judgeValue = (int)(Math.Round(doWorkValue * (1 + model)));

                rangeStart = i + 1;
                rangeEnd = i + readRange;
                if (rangeStart >= prices_New.Count)//超过范围，退出
                {
                    dic.Add(i, "NA1");
                    break;
                }
                //rangeEnd = (rangeEnd >= prices.Count ? prices.Count - 1 : rangeEnd);
                rangeEnd = prices_New.Count - 1;

                //所有范围去找，
                int judgeIndex = FindValueInWorkRange(prices_New, rangeStart, rangeEnd, judgeValue);
                if (judgeIndex == -1) //范围内存在，才正常作业，否则设置NA1
                {
                    dic.Add(i, "NA1");
                }
                else
                {
                    if (dic_Judge.ContainsKey(judgeIndex) == false)
                    {
                        dic_Judge.Add(judgeIndex, judgeValue);
                    }

                    targetValue = (int)(Math.Round(doWorkValue * (1 + model) * (1 + model)));
                    oppositeTargetValue = (int)(Math.Round(doWorkValue * (1 + model) * (1 - model)));
                    //int tmp = FindFirst(prices, i + 1, targetValue, oppositeTargetValue);
                    int tmp = FindFirstNew(judgeIndex + 1, targetValue, oppositeTargetValue);
                    if (tmp == -1)//NA2
                    {
                        dic.Add(i, "NA2");
                    }
                    else
                    {
                        if (tmp == targetValue)//True
                        {
                            dic.Add(i, "True");
                        }
                        else if (tmp == oppositeTargetValue)//False
                        {
                            dic.Add(i, "False");
                        }
                    }
                }
            }
            return dic;
        }

        /// <summary>
        /// 补数据
        /// </summary>
        public static void CompleteData()
        {
            prices_New = new List<int>();
            int? lastValue = null; // 使用Nullable<int>来存储上一行的整数值  
            if (prices.Count > 0)
            {
                lastValue = prices[0];
                prices_New.Add(prices[0]);

                for (int i = 1; i < prices.Count; i++)
                {
                    if (prices[i] != lastValue.Value + 1)
                    {
                        if (prices[i] > lastValue.Value + 1)
                        {
                            for (int j = lastValue.Value + 1; j < prices[i]; j++)
                            {
                                prices_New.Add(j);
                            }
                        }
                        else
                        {
                            for (int j = lastValue.Value - 1; j > prices[i]; j--)
                            {
                                prices_New.Add(j);
                            }
                        }
                    }
                    prices_New.Add(prices[i]);
                    lastValue = prices[i];
                }
            }
            //return prices_New;
        }

        /// <summary>
        /// 读文件并转int
        /// </summary>
        /// <param name="csvFilePath"></param>
        public static void ReadCsvAndConvertToInt(string csvFilePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(csvFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 假设CSV文件中的每行都只有一个decimal类型的值，且没有额外的逗号或引号  
                        if (decimal.TryParse(line, out decimal price))
                        {
                            prices.Add((int)Math.Round(price, 0, MidpointRounding.AwayFromZero));
                        }
                        else
                        {
                            Console.WriteLine($"无法解析价格：{line}");
                        }
                    }
                }

                Console.WriteLine($"已加载 {prices.Count} 条价格数据。");
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("内存不足，无法加载所有数据。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
        }

        public static void WriteRltToCsv(string csvFilePath, Dictionary<int, string> dic, Dictionary<int, int> dic_Judge)
        {
            using (StreamWriter writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine("价格数据");
                for (int i = 0; i < prices_New.Count; i++)
                {
                    string value = "";
                    if (dic.ContainsKey(i))
                    {
                        value = dic[i].Trim() + "," + prices_New[i];
                    }
                    else
                    {
                        value = "," + prices_New[i];
                    }

                    if (dic_Judge.ContainsKey(i))
                    {
                        value += ("," + dic_Judge[i]);
                    }
                    writer.WriteLine(value); // 写入CSV文件  
                    // 如果需要每写入一定数量行就刷新缓冲区，可以取消注释以下行  
                    //if (i % 500000 == 0) writer.Flush();  
                }
                writer.Flush(); // 确保所有数据都已写入文件  
            }
            Console.WriteLine("写入完成。");
        }

        static int FindValueInWorkRange(List<int> list, int startindex, int endIndex, int value)
        {
            for (int i = startindex; i <= endIndex; i++)
            {
                int number = list[i];
                if (number == value)
                {
                    return i;
                }
            }
            return -1;
        }

        static int FindFirst(List<int> list, int index, int a, int b)
        {
            for (int i = index; i < list.Count; i++)
            {
                int number = list[i];
                if (number == a)
                {
                    return a;
                }
                else if (number == b)
                {
                    return b;
                }
            }

            return -1;
        }


        static int FindFirstNew(int startindex, int targetValue, int oppositeTargetValue)
        {
            //if (dic_Zip.ContainsKey(targetValue))
            //{
            //    List<int> lst = dic_Zip[targetValue];
            //    i = lst.Where(p => p >= startindex).Min();
            //}
            int i = 0, j = 0;
            if (dic_Zip.ContainsKey(targetValue))
            {
                List<int> lst = dic_Zip[targetValue];
                for (int m = 0; m < lst.Count; m++)
                {
                    if (lst[m] >= startindex)
                    {
                        i = lst[m];
                        break;
                    }
                }
                // 如果需要，可以在这里处理i未被赋值的情况（即没有找到符合条件的元素）  
            }
            if (dic_Zip.ContainsKey(oppositeTargetValue))
            {
                List<int> lst = dic_Zip[oppositeTargetValue];
                for (int m = 0; m < lst.Count; m++)
                {
                    if (lst[m] >= startindex)
                    {
                        j = lst[m];
                        break;
                    }
                }
            }
            if (i == j && i == 0)
                return -1;
            if (i > 0 && i < j)
            {
                return targetValue;
            }
            else
                return oppositeTargetValue;
        }

        public static void ZipData()
        {
            dic_Zip = new Dictionary<int, List<int>>();
            for (int i = 0; i < prices_New.Count; i++)
            {
                int value = prices_New[i];
                if (dic_Zip.ContainsKey(value))
                {
                    dic_Zip[value].Add(i);
                }
                else
                {
                    dic_Zip.Add(value, new List<int> { i });
                }
            }
        }
    }
}
