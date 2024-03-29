﻿using OpenCvSharp;
using our1314;

namespace AIDatasetsPro.src
{
    internal class test_简单图像搜索 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入搜索图像的路径：");
            var searchimage = Console.ReadLine().Replace("\"", "");
            Console.WriteLine("输入搜索文件夹路径：");
            var searchpath = Console.ReadLine().Replace("\"", "");


            //1、被搜索图像
            var temp = new Mat(searchimage, ImreadModes.Color);

            //2、搜索
            var images = new DirectoryInfo(searchpath).GetFiles("*", searchOption: SearchOption.AllDirectories);
            images = images.Where(p => p.Extension == ".bmp" || p.Extension == ".png" || p.Extension == ".jpg").ToArray();
            images = images.OrderBy((p) =>
            {
                var img = new Mat(p.FullName, ImreadModes.Color);
                img = img.Resize(temp.Size());

                Mat diff = new Mat();
                Cv2.Absdiff(temp, img, diff);

                var sum = diff.Sum().Val0;
                return sum;

                //img = img.Resize(temp.Size());
                //img.ConvertTo(img, MatType.CV_8S);
                //temp.ConvertTo(temp, MatType.CV_8S);

                //Cv2.Subtract(temp, img, diff);
                //Mat abs = diff.Abs();
                //var sum = abs.Sum();
                //return sum.Val0;
            }).ToArray();

            //3、最相似的图像
            var f1 = new Mat(images.First().FullName, ImreadModes.Color);
            f1 = f1.Resize(temp.Size());

            //4、显示
            var dis = new Mat();
            var I = new Mat(temp.Rows, 10, temp.Type(), Scalar.Black);
            Cv2.HConcat(new[] { temp, I, f1 }, dis);

            //如果目标尺寸过大则按比例进行缩放
            var thr_max = 1500.0;
            var max = Math.Max(dis.Width, dis.Height);
            if (max > thr_max)
            {
                var f = thr_max / max;
                dis = dis.Resize(new Size(), f, f, interpolation: InterpolationFlags.Nearest);
            }

            var thr_min = 500.0;
            var min = Math.Min(dis.Width, dis.Height);
            if (min < thr_min)
            {
                var f = thr_min / max;
                dis = dis.Resize(new Size(), f, f, interpolation: InterpolationFlags.Nearest);
            }


            dis = dis.CopyMakeBorder(0, 40, 0, 0, BorderTypes.Constant);
            dis.PutText(images[0].FullName, new Point(0, dis.Height - 10), HersheyFonts.HersheyComplex, 0.3, Scalar.Red, 1);
            Cv2.ImShow(images[0].Name, dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
}
