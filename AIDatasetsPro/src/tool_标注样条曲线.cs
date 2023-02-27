using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.test;

namespace AIDatasetsPro.src
{
    internal class tool_标注样条曲线 : ConsoleTestBase
    {
        Window DisplayWindow = null;
        Mat dis = new Mat();
        public override void RunTest()
        {
            /*
             * 创建与背景图像同尺寸的mask图像，同时显示背景图、mask、融合图像。
             * 1、操作：首先确定起点和终点，点第三点时绘制样条曲线，当点选定后，还可手动调整其位置。
             */

            Console.WriteLine("输入需要标注的图像路径：");
            var path = Console.ReadLine().Replace("\"", "");

            var src = new Mat(path, ImreadModes.Grayscale);
            Mat mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
            mask.SetTo(0);

            //1、创建滚动条窗体
            var TrackbarWindow = new Window("TrackbarWindow");
            TrackbarWindow.Resize(500, 200);

            TrackbarWindow.CreateTrackbar("Value1", 32, 255, (a) => { }); //Cv2.SetTrackbarMin("Value1", "TrackbarWindow", 1);
            TrackbarWindow.CreateTrackbar("Value2", 255, 255, (a) => { }); //Cv2.SetTrackbarMin("Value2", "TrackbarWindow", 1);
            TrackbarWindow.CreateTrackbar("Value3", 0, 16, (a) => { });//Cv2.SetTrackbarMin("Value3", "TrackbarWindow", 1);
            
            //2、创建显示窗体
            DisplayWindow = new Window("DisplayWindow", flags: WindowFlags.KeepRatio | WindowFlags.GuiExpanded);
            Cv2.SetMouseCallback("DisplayWindow", MouseCallback);

            //var dis = new Mat();
            //while(true) 
            //{
            //    //Mat add = src + mask;
            //    //Cv2.HConcat(new[] { src, mask, add }, dis);
            //    //DisplayWindow.ShowImage(dis);
            //    //var key = Cv2.WaitKey();
            //}
            var key = Cv2.WaitKey();
        }

        void MouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            if(@event == MouseEventTypes.LButtonUp&& flags == MouseEventFlags.CtrlKey)
            {
                dis.Circle(x, y, 10, Scalar.Red, -1);
            }
            //var dis = new Mat(500, 1000, MatType.CV_8UC1, 0);
            //dis.PutText($"{x},{y}", new Point(0, 60), HersheyFonts.Italic, 2, Scalar.White, 2);
            DisplayWindow.ShowImage(dis);
            Cv2.WaitKey(500);
        }
    }
}
