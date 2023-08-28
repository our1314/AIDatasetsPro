using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using work.cv;
using work.test;
using Scalar = OpenCvSharp.Scalar;

namespace AIDatasetsPro.src
{
    internal class test_生成XRay空洞检测数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            //var x = torch.randn(new[] { 256L, 256L }, torch.ScalarType.Float64);
            //x = (x - 0.7) * 2;
            //var data = x.data<double>().ToArray();
            //var img = new Mat(256, 256, MatType.CV_64F, data);
            //img = img - 0.7;
            //Cv2.ImShow("dis", img);
            //Cv2.WaitKey();

            var background = new Mat(@"D:\desktop\空洞检测\back (1).png", ImreadModes.Grayscale);
            var force = new Mat(50, 50, MatType.CV_8UC1, Scalar.Black);
            force.Circle(25, 25, 3, Scalar.White, -1);

            var roi = background[30, 30 + 50, 50, 50 + 50];

            var dis = new Mat();
            Cv2.AddWeighted(roi, 0.5, force, 0.5, 0, roi);
            
            Cv2.ImShow("dis", background);
            Cv2.WaitKey();


        }
    }
}
