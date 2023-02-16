using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_将目录下的图像全部改为指定格式 : ConsoleTestBase
    {
        public override void RunTest()
        {
            var default_ext = ".jpg";
            Console.WriteLine($"输入目标格式(例：{default_ext}，直接回车默认为：{default_ext})：");
            var target_ext = Console.ReadLine().Trim();//".jpg";
            target_ext = target_ext == "" ? default_ext : target_ext;

            Console.WriteLine($"将路径下的图像全部改为{target_ext}格式，输入文件夹路径：(可以拖拽!)");
            var path = Console.ReadLine();
            var files = new DirectoryInfo(path).GetFiles();
            var img_ext = new[] { ".png", ".jpg", ".bmp", ".jif" };
            files = files.Where(f => img_ext.Contains(f.Extension) && f.Extension != target_ext).ToArray();

            foreach (var f in files)
            {
                var img = new Mat(f.FullName, ImreadModes.Unchanged);
                img.SaveImage(f.FullName.Replace(f.Extension, target_ext));
                Console.WriteLine(f.Name);
            }
        }
    }
}
