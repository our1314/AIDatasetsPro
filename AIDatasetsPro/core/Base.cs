using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDatasetsPro.core
{
    internal static class Base
    {
        public static List<(int, double, double, double, double)> yolostr2doublearray(string yolo_str)
        {
            var list = new List<(int, double, double, double, double)>();
            var strs = yolo_str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in strs)
            {
                var temp = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var data = ((int.Parse(temp[0])), (double.Parse(temp[1])), (double.Parse(temp[2])), (double.Parse(temp[3])), (double.Parse(temp[4])));
                list.Add(data);
            }

            return list;
        }

        public static string doublearray2yolostr(List<(int, double, double, double, double)> yolo_data)
        {
            var yolo_str = new List<string>();
            foreach (var data in yolo_data)
            {
                var temp = $"{data.Item1} {data.Item2:F6} {data.Item3:F6} {data.Item4:F6} {data.Item5:F6}";
                yolo_str.Add(temp);
            }
            var result = string.Join("\r\n", yolo_str).Trim();
            return result;
        }
    }
}
