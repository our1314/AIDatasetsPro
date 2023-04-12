using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDatasetsPro.core
{
    internal static class Base
    {
        public static List<double[]> yolostr2doublearray(string yolo_str)
        {
            var list = new List<double[]>();
            var strs = yolo_str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in strs)
            {
                var temp = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var data = Array.ConvertAll(temp, p => double.Parse(p));
                list.Add(data);
            }
            return list;
        }

        public static string doublearray2yolostr(List<double[]> yolo_data)
        {
            var yolo_str = new List<string>();
            foreach (var data in yolo_data)
            {
                var dd = Array.ConvertAll(data, p => Math.Round(p, 8));
                var temp = string.Join(" ", dd);
                yolo_str.Add(temp);
            }
            var result = string.Join("\r\n", yolo_str).Trim();
            return result;
        }
    }
}
