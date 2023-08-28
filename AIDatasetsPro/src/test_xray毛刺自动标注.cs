using AIDatasetsPro.core;
using HalconDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.cv;
using work;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_xray毛刺自动标注 : ConsoleTestBase
    {
        public override void RunTest()
        {
            var @namespace = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;

            Console.WriteLine("输入类名：");

            var className = $"{@namespace}.{Console.ReadLine().Trim()}";
            var type = Type.GetType(className);
            var ic = (IIc)type.Assembly.CreateInstance(className);
            var data_dir_path = ic.data_dir_path;
            var img_files = new DirectoryInfo(data_dir_path).GetFiles();
            img_files = img_files.Where(f => f.Extension == ".jpg" || f.Extension == ".bmp" || f.Extension == ".png").ToArray();

            HOperatorSet.SetSystem("border_shape_models", "true");
            var anchor = ic.size;//new Size(juanpan.size.Width,juanpan.size.Height+10);//


            bool MakeBorder = className.Contains("juanpan") ? true : false;
            int border = 300;
            foreach (var f in img_files)
            {
                Mat src = new Mat(f.FullName, ImreadModes.Grayscale);
                var temp = src.Clone();
                var dis = src.CvtColor(ColorConversionCodes.GRAY2BGR);
                if (MakeBorder)
                {
                    temp = temp.CopyMakeBorder(border, border, border, border, BorderTypes.Constant, Utils.GetHistMostGray(temp));
                }

                var result_match = ic.FindModel(temp, 0.45, 0, out _, out _, MaxOverlap: 0);
                if (result_match == null) continue;

                var str_label = "";
                foreach (var p in result_match)
                {
                    var x0 = MakeBorder ? (int)p[0] - border : (int)p[0];
                    var y0 = MakeBorder ? (int)p[1] - border : (int)p[1];
                    var angle = 0;//p[2] + 0;
                    var scale = p[3];
                    var score = p[4];

                    //angle = angle * Math.PI / 180d;
                    var anchor1 = new Size(anchor.Width * scale, anchor.Height * scale);
                    var pts = Utils.DrawRotateRect(ref dis, new Point(x0, y0), anchor1, angle);

                    #region 保存ROI
                    {
                        var roi = Utils.GetRotateROI(src, new Point(x0, y0), anchor1, angle);
                        roi.ImSave(@$"{data_dir_path}\ROI\{Utils.Now}.png");
                    }
                    #endregion

                    var classname = ic.GetType().Name;
                    if (!classname.Contains("juanpan"))
                    {
                        var cx = (double)x0 / src.Width;
                        var cy = (double)y0 / src.Height;
                        var w = (double)anchor1.Width / src.Width;
                        var h = (double)anchor1.Height / src.Height;

                        str_label += $"0 {Math.Round(cx, 6)} {Math.Round(cy, 6)} {Math.Round(w, 6)} {Math.Round(h, 6)}\r\n";
                    }
                    else
                    {
                        str_label += $"{pts[0].X:F3} {pts[0].Y:F3} {pts[1].X:F3} {pts[1].Y:F3} {pts[2].X:F3} {pts[2].Y:F3} {pts[3].X:F3} {pts[3].Y:F3} 0 0\r\n";
                    }
                }
                str_label = str_label.Trim();

                var save_path = f.FullName.Replace(".jpg", ".txt");
                if (!File.Exists(save_path))
                    File.WriteAllText(save_path, str_label);

                Cv2.ImShow("dis", dis.PyrDown());
                Cv2.WaitKey(1);
            }

            //在目录下保存classes.txt，LabelImg需要此文件。
            var ff = @$"{data_dir_path}\classes.txt";
            if (!File.Exists(ff))
                File.WriteAllText(ff, "0");
        }

        #region xray毛刺
        class xray_毛刺1 : TemplateMatch, IIc
        {
            public string data_dir_path => @"D:\desktop\xray毛刺检测\TO252样品图片\TO252框架好品";
            public double[] region_coord = new[] { 154d, 3, 295, 413 };
            public int[] contrast = new[] { 20, 41, 8 };
            public int mincontrast = 3;
            public int angle_range = 360;

            public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
            public xray_毛刺1()
            {
                var img_temp = new Mat(@$"{"D:\\desktop\\xray毛刺检测\\TO252样品图片\\TO252编带好品"}\2.jpg", ImreadModes.Grayscale);

                HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
                //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
                var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
                Cv2.ImShow("dis", dis);
                Cv2.WaitKey();
                Cv2.DestroyAllWindows();
            }
        }

        class xray_毛刺2 : TemplateMatch, IIc
        {
            public string data_dir_path => @"D:\desktop\xray毛刺检测\TO252样品图片\TO252编带好品";
            public double[] region_coord = new[] { 154d, 3, 295, 413 };
            public int[] contrast = new[] { 20, 41, 8 };
            public int mincontrast = 3;

            public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
            public xray_毛刺2()
            {
                var img_temp = new Mat(@$"{data_dir_path}\2.jpg", ImreadModes.Grayscale);

                HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
                //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
                var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
                Cv2.ImShow("dis", dis);
                Cv2.WaitKey();
                Cv2.DestroyAllWindows();
            }
        }
        #endregion
    }
}
