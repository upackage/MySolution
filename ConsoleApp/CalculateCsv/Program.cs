using static CalculateCsv.CommonFunc;
using static CalculateCsv.Business;

namespace CalculateCsv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string csvFilePath = @"D:\0_Test\prices.csv";
                string csvFilePath_OutPut = @"D:\0_Test\output.csv";

                decimal model = 0.2M;
                Dictionary<int, string> dic = new Dictionary<int, string>();
                Dictionary<int, int> dic_Judge = new Dictionary<int, int>();
                string readStr = "";

                Console.WriteLine(@"请输入含标题的源文件地址(默认D:\0_Test\prices.csv)：");
                readStr = Console.ReadLine();
                csvFilePath = readStr.Length >= 1 ? readStr : @"D:\0_Test\prices.csv";

                Console.WriteLine("请输入起始行（默认0）：");
                readStr = Console.ReadLine();
                startIndex = readStr.Length >= 1 ? Convert.ToInt32(readStr) : 0;

                Console.WriteLine("请输入间隔行数（默认3）：");
                readStr = Console.ReadLine();
                readRange = readStr.Length >= 1 ? Convert.ToInt32(readStr) : 3;

                Console.WriteLine("请输入分析模式（默认0.2,即20%，百分比请转换成小数输入）：");
                readStr = Console.ReadLine();
                model = readStr.Length >= 1 ? Convert.ToDecimal(readStr) : 0.2M;

                csvFilePath_OutPut = Path.Combine(Path.GetDirectoryName(csvFilePath), "output.csv");

                DateTime dt_Start = DateTime.Now;
                Console.WriteLine($@"开始时间：{dt_Start.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

                ReadCsvAndConvertToInt(csvFilePath);
                CompleteData();

                ZipData();
                CalLastResult(model, dic, dic_Judge);
                WriteRltToCsv(csvFilePath_OutPut, dic, dic_Judge);

                DateTime dt_End = DateTime.Now;
                Console.WriteLine($"完成时间：{dt_Start.ToString("yyyy-MM-dd HH:mm:ss.fff")}\n共花费{(dt_End - dt_Start).TotalSeconds}秒!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"出错了，{ex.ToString()}");
                Console.ReadKey();
            }
        }
    }
}
