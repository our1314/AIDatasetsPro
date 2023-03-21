using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.math;
using work.test;

namespace AIDatasetsPro.src
{
    internal class tool_标注样条曲线 : ConsoleTestBase
    {
        Window DisplayWindow = null, DrawWindow = null;
        Mat src = new Mat(),
            dis_draw = new Mat(),
            dis = new Mat(),
            mask = new Mat();
        Mat pts = new Mat();

        public override void RunTest()
        {
            /*
             * 创建与背景图像同尺寸的mask图像，同时显示背景图、mask、融合图像。
             * 1、操作：首先确定起点和终点，点第三点时绘制样条曲线，当点选定后，还可手动调整其位置。
             */

            #region 0、读取图像
            //Console.WriteLine("输入需要标注的图像路径：");
            //var path = Console.ReadLine().Replace("\"", "");
            var path = @"D:\desktop\2.png";

            src = new Mat(path, ImreadModes.Color);
            mask = Mat.Zeros(src.Size(), MatType.CV_8UC3);
            mask.SetTo(0);
            #endregion

            //1、创建滚动条窗体
            var TrackbarWindow = new Window("TrackbarWindow"); TrackbarWindow.Resize(500, 200);
            DisplayWindow = new Window("DisplayWindow", flags: WindowFlags.AutoSize | WindowFlags.GuiExpanded);
            DrawWindow = new Window("DrawWindow", flags: WindowFlags.KeepRatio | WindowFlags.GuiExpanded);

            Cv2.SetMouseCallback("DrawWindow", MouseCallback);
            DrawWindow.Resize(src.Width, src.Height);
            DrawWindow.ShowImage(src);

            TrackbarWindow.CreateTrackbar("Value1", 3, 5, (a) => { DrawWindow.Resize(src.Width * a, src.Height * a); }); Cv2.SetTrackbarMin("Value1", "TrackbarWindow", 1);
            TrackbarWindow.CreateTrackbar("Value2", 255, 255, (a) => { }); //Cv2.SetTrackbarMin("Value2", "TrackbarWindow", 1);
            TrackbarWindow.CreateTrackbar("Value3", 0, 16, (a) => { });//Cv2.SetTrackbarMin("Value3", "TrackbarWindow", 1);

            var key = Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }

        //点击点、拖动点（按下不放根据鼠标位置调整点、松开确定）
        void MouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            if (@event == MouseEventTypes.LButtonUp && flags == MouseEventFlags.CtrlKey)
            {
                //添加坐标点
                mask.Line(x, y, x, y, Scalar.Red, 1);
                pts.PushBack(new Mat(1, 2, MatType.CV_64FC1, new double[] { x, y }));
                if (pts.Rows < 3) return;
                //绘制样条曲线
                Spline spline = new Spline();
                Mat line = spline.QuadraticSpline(pts);
                spline.GetQuadraticSplineCoord(pts, line, out double[] xx, out double[] yy);

                dis_draw = new Mat();
                Cv2.AddWeighted(src, 1, mask, 0.2, 0, dis_draw);
                DrawLine(dis_draw, xx, yy);
                DrawWindow.ShowImage(dis_draw);
            }
            if (@event == MouseEventTypes.LButtonDown && flags == (MouseEventFlags.LButton | MouseEventFlags.AltKey))
            {
                //移动坐标点
                var mouse_pt = new Mat(2, 1, MatType.CV_64FC1, new double[] { x, y });
                pts.GetRectangularArray(out double[,] pts_array);
                var index = GetIndex(mouse_pt, pts);
                pts.Set<double>(0, index, x);
                pts.Set<double>(1, index, y);

                //绘制样条曲线
                Spline spline = new Spline();
                Mat line = spline.QuadraticSpline(pts);

                spline.GetQuadraticSplineCoord(pts, line, out double[] xx, out double[] yy);

                dis_draw = new Mat();
                Cv2.AddWeighted(src, 1, mask, 0.2, 0, dis_draw);
                DrawLine(dis_draw, xx, yy);
                DrawWindow.ShowImage(dis_draw);
            }

            //var dis_draw = new Mat();
            //Cv2.AddWeighted(src, 1, mask, 0.2, 0, dis_draw);
            //DrawWindow.ShowImage(dis_draw);

            //Mat add = src + mask;
            //Cv2.HConcat(new[] { src, mask, add }, dis);
            //DisplayWindow.ShowImage(dis);
            Cv2.WaitKey(1);
        }

        int GetIndex(Mat pt, Mat pts)
        {
            var dis = new List<double>();
            for (int i = 0; i < pts.Cols; i++)
            {
                Mat d = pt - pts.Col(i);
                dis.Add(d.Norm());
            }

            var min = dis.Min();
            var index_min = dis.IndexOf(min);
            return index_min;
        }

        void DrawLine(Mat dis, double[] x, double[] y)
        {
            var pt1 = new Point(x[0], y[0]);
            for (int i = 1; i < x.Length; i++)
            {
                var pt2 = new Point(x[i], y[i]);
                dis.Line(pt1, pt2, Scalar.Red, 1);
                pt1 = pt2;
            }
        }
    }
}
