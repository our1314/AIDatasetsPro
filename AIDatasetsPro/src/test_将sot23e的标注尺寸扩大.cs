using AIDatasetsPro.core;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_将sot23e的标注尺寸扩大 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入路径：");
            var dir = Console.ReadLine();

            var list_files = Directory.GetFiles(dir);
            var images = list_files.Where(f => Path.GetExtension(f) == ".jpg").ToList();
            var labels = list_files.Where(f => Path.GetExtension(f) == ".txt" && Path.GetFileName(f) != "classes.txt").ToList();
            
            foreach (var label_path in labels)
            {
                var image_path = images.First(f => Path.GetFileNameWithoutExtension(f) == Path.GetFileNameWithoutExtension(label_path));
                var img = new Mat(image_path);
                var (width, height) = ((double)img.Width, (double)img.Height);

                var yolo_str = File.ReadAllText(label_path, Encoding.UTF8);
                var yolodata = Base.yolostr2doublearray(yolo_str);
                for (int i = 0; i < yolodata.Count; i++)
                {
                    var data = yolodata[i];

                    //data[3] = 60d / width;
                    //data[4] = 102d / height;
                    //data[2] = data[2] + 1d / height;
                    data[3] = 62d / width;
                    data[4] = 104d / height;

                    yolodata[i] = data;
                }
                var new_yolostr = Base.doublearray2yolostr(yolodata);
                new_yolostr.StrSave(label_path);
            }
        }
    }
}
