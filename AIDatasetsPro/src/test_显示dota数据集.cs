using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_显示dota数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入dota数据集路径：");
            var dir = Console.ReadLine();
            var files = new DirectoryInfo(dir).GetFiles("*", SearchOption.AllDirectories);
            files = files.OrderBy(f => f.LastWriteTime).ToArray();

            var files_image = files.Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".bmp");
            var files_label = files.Where(f => f.Extension == ".txt");
            foreach (var file_img in files_image)
            {
                var dis = new Mat(file_img.FullName, ImreadModes.Color);
                var file_lab = files_label.Where(f => Path.GetFileNameWithoutExtension(f.Name) == Path.GetFileNameWithoutExtension(file_img.Name));
                if (file_lab.Count() == 0) continue;

                var label = file_lab.First();
                var str = File.ReadAllText(label.FullName).Trim();
                var lines = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var ch = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    var coord = Array.ConvertAll(ch, c => double.Parse(c));
                    var pts = new Point[] { new Point(coord[0], coord[1]), new Point(coord[2], coord[3]), new Point(coord[4], coord[5]), new Point(coord[6], coord[7]) };
                    dis.Polylines(new[] { pts }, true, Scalar.Red, 2);
                }
                Cv2.ImShow("dis", dis);
                Cv2.WaitKey();
            }
        }
    }
}
