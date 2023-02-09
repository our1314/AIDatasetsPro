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
            var ic = new yy();
            var src1 = new Mat(@"..\..\..\data\1.jpg", ImreadModes.Grayscale);
            //ic.FindModel(src1, 0.6, 0, out Mat dis1, out _);

            var img_files = new DirectoryInfo(@"..\..\..\data").GetFiles();
            img_files = img_files.Where(f => f.FullName.EndsWith(".jpg") || f.FullName.EndsWith(".bmp") || f.FullName.EndsWith(".png")).ToArray();

            //xx ic = new xx(1);
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

    class yy:TemplateMatch, IIc
    {
        public string data_dir_path => _data_dir_path;
        public Size size => _size;

        string _data_dir_path;
        Size _size = new Size();

        List<xx> list = new List<xx>();
        public yy()
        {
            for (double i = 0.7; i <= 1/0.7; i += 0.1)
            {
                list.Add(new xx(i));
            }

            _data_dir_path = list[0].data_dir_path;
            _size = list[0].size;
        }

        

        public List<double[]> FindModel(Mat Src, double MinScore, int NumMatches, out Mat dis, out Mat mask, int NumLevels = 5, double MaxOverlap = 0.0, string SubPixel = "true", double Greediness = 0.7)
        {
            dis = new Mat();
            mask = new Mat();
            
            var scale = new[] { 0.7d, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.4 };

            var result = new List<(double,double, List<double[]>)>();
            for (int i = 0; i < list.Count; i++)
            {
                var x = list[i];
                var r = x.FindModel(Src, 0.6, 0, out dis, out mask);
                var mean = 0d;
                if (r != null)
                {
                    mean = r.Sum(p => p[4]) / r.Count;
                }
                result.Add((scale[i], mean, r));
            }
            result = result.OrderByDescending(p => p.Item2).ToList();
            var max = result.First();

            //List<(double, double, double)> result = new List<(double, double, double)>();
            //var a = result.Max(p => Math.Sqrt(p.Item1 * p.Item1 + p.Item2 * p.Item2));

            //for(a)
            var aaa = max.Item3;
            for (int i = 0; i < aaa.Count; i++)
            {
                aaa[i][3] = max.Item1;
            }
            return aaa;
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
