﻿using OpenCvSharp;
using work.cv;
using work.test;
using static System.Math;
namespace AIDatasetsPro.src
{
    internal class test_xxxxxxxxxxxxxxxxxxxxxxxxx : ConsoleTestBase
    {
        public override void RunTest()
        {
            var p = new Mat(3, 3, MatType.CV_64FC1, new double[,]
            {
                {Cos(PI/4),0,-Cos(PI/4)},
                {Sin(PI/4) ,0,Sin(PI/4)},
                {0 ,-1,0}
            });
            //var p = new Mat(3, 3, MatType.CV_64FC1, new double[,]
            //{
            //    {-Cos(PI/4),0,-Cos(PI/4)},
            //    {-Sin(PI/4),0, Sin(PI/4)},
            //    {0 ,-1,0}
            //});
            //Console.WriteLine((p.Col(0).T() * p.Col(1)).ToMat().Dump());
            //Console.WriteLine((p.Col(1).T() * p.Col(2)).ToMat().Dump());
            //Console.WriteLine((p.Col(2).T() * p.Col(0)).ToMat().Dump());

            var a = work.math.MathExp._SO3(p);
            Console.WriteLine(a.Dump());
            return;

            Console.WriteLine("输入图像文件：");
            var path = Console.ReadLine();//.Replace("\"", "");
            var src = new Mat(path, ImreadModes.Unchanged);
            Cv2.MinMaxLoc(src, out double min, out double max);

            Console.WriteLine($"Channels:{src.Channels()}, min:{min},max:{max}");

            return;

            for (int i = 0; i < 50; i++)
            {
                var scale_start = 0.7;
                var scale_end = 1 / 0.7;

                var scale = scale_start + (scale_end - scale_start) * new Random().NextDouble();

                var img = src.Resize(new Size(), scale, scale);
                img = img - 100;
                var name = Path.GetFileNameWithoutExtension(path);
                img.ImSave(path.Replace(name, work.Work.Now));
            }
        }
    }
}
