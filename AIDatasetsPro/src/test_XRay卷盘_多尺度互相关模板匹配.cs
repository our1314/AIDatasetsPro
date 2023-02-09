using AIDatasetsPro.core;
using HalconDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work;
using work.cv;
using work.math;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_XRay卷盘_多尺度互相关模板匹配 : ConsoleTestBase
    {
        public override void RunTest()
        {
            var y = new yy();

            var img_files = new DirectoryInfo(@"..\..\..\data").GetFiles();
            img_files = img_files.Where(f => f.FullName.EndsWith(".jpg") || f.FullName.EndsWith(".bmp") || f.FullName.EndsWith(".png")).ToArray();

            xx ic = new xx(1);
            var anchor = ic.size;

            bool MakeBorder = false;
            int border = 300;
            foreach (var f in img_files)
            {
                Mat src = new Mat(f.FullName, ImreadModes.Grayscale);
                var dis = src.CvtColor(ColorConversionCodes.GRAY2BGR);
                if (MakeBorder)
                {
                    src = src.CopyMakeBorder(border, border, border, border, BorderTypes.Constant, CV.GetHistMostGray(src));
                }

                var result_match = ic.FindModel(src, 0.7, 0, out _, out _, MaxOverlap: 0);
                if (result_match == null) continue;

                var str_label = "";
                foreach (var p in result_match)
                {
                    var x0 = MakeBorder ? (int)p[0] - border : (int)p[0];
                    var y0 = MakeBorder ? (int)p[1] - border : (int)p[1];
                    var angle = p[2];
                    var scale = p[3];
                    var score = p[4];

                    angle = angle * Math.PI / 180d;
                    var anchor1 = new Size(anchor.Width * scale, anchor.Height * scale);
                    var pts = CV.DrawRotateRect(ref dis, new Point(x0, y0), anchor1, angle);

                    var classname = ic.GetType().Name;
                    if (!classname.Contains("juanpan"))
                    {
                        var cx = (double)x0 / src.Width;
                        var cy = (double)y0 / src.Height;
                        var w = (double)anchor1.Width / src.Width;
                        var h = (double)anchor1.Height / src.Height;

                        str_label += $"0 {cx} {cy} {w} {h}\r\n";
                    }
                    else
                    {
                        str_label += $"{pts[0].X:F3} {pts[0].Y:F3} {pts[1].X:F3} {pts[1].Y:F3} {pts[2].X:F3} {pts[2].Y:F3} {pts[3].X:F3} {pts[3].Y:F3} 0 0\r\n";
                    }
                }
                str_label = str_label.Trim();

                Cv2.ImShow("dis", dis.PyrDown());
                Cv2.WaitKey();
            }
        }
    }

    class yy
    {
        
        public yy()
        {
            /*
             * 1、以一定的步长多次缩放图像及其模板位姿，创建多个模板
             * 2、使用多个模板进行互相关匹配，取分数最高的
             */
            List<xx> list = new List<xx>();
            for (double i = 0.7; i <= 1/0.7; i += 0.1)
            {
                list.Add(new xx(i));
                //xx x = new xx(i);
            }
        }
    }
    class xx : TemplateMatch, IIc
    {
        public string data_dir_path => @"..\..\..\data";

        public double[] region_coord = new[] { 276.836, 631.382, MathExp.Rad(10.0711), 168.208, 63.286 };
        public int[] contrast = new[] { 40, 73, 4 };
        public int mincontrast = 20;
        public Size size => new Size(region_coord[3] * 2, region_coord[4] * 2);

        public xx(double f)
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\1.jpg", ImreadModes.Grayscale);
            Cv2.Resize(img_temp, img_temp, new Size(), f, f);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0] * f, region_coord[1] * f, region_coord[2], region_coord[3] * f, region_coord[4] * f);//
            var dis = CreateNccModel(img_temp, ModelRegion);

            //Cv2.ImShow("dis", dis);
            //Cv2.WaitKey(1);
            //Cv2.DestroyAllWindows();
        }
    }
}
