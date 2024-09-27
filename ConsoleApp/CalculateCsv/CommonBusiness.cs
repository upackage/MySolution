using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculateCsv
{
    public static class Business
    {
        public static int startIndex = 0;
        public static int readRange = 3;
        public static List<int> prices = new List<int>();
        public static List<int> prices_New = new List<int>();
        public static Dictionary<int, List<int>> dic_Zip = new Dictionary<int, List<int>>();
    }
}
