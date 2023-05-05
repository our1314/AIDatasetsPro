using AIDatasetsPro.core;
using HalconDotNet;
using OpenCvSharp;
using work;
using work.cv;
using work.math;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_XRay基于模板匹配的自动标注 : ConsoleTestBase
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
            img_files = img_files.Where(f => f.Extension==".jpg" || f.Extension == ".bmp" || f.Extension == ".png").ToArray();

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
                    temp = temp.CopyMakeBorder(border, border, border, border, BorderTypes.Constant, CV.GetHistMostGray(temp));
                }

                var result_match = ic.FindModel(temp, 0.85, 0, out _, out _, MaxOverlap: 0);
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
                    var pts = CV.DrawRotateRect(ref dis, new Point(x0, y0), anchor1, angle);

                    #region 保存ROI
                    {
                        var roi = CV.GetRotateROI(src, new Point(x0, y0), anchor1, angle);
                        roi.ImSave(@$"{data_dir_path}\ROI\{Work.Now}.png");
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
    }

    #region 料片类
    class xray_sc88 : TemplateMatch, IIc
    {
        public string data_dir_path => throw new NotImplementedException();
        public Size size => throw new NotImplementedException();
        public xray_sc88()
        {
            Mat img_temp = new Mat(@"D:\桌面\LF-SC88\LF-SC70 88-8units-all__1__002_12-6544_0000.jpg", ImreadModes.Grayscale);
            HOperatorSet.GenRectangle1(out HObject ModelRegion, 213.019, 261.933, 322.282, 452.218);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, new[] { 13, 19, 9 }, 5, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sc70 : TemplateMatch, IIc
    {
        public string data_dir_path => throw new NotImplementedException();
        public Size size => throw new NotImplementedException();
        public xray_sc70()
        {
            Mat img_temp = new Mat(@"D:/桌面/sc70-1212/LF-SC70 88-8units-all__1__009_sc70-1212_0000.jpg", ImreadModes.Grayscale);
            HOperatorSet.GenRectangle1(out HObject ModelRegion, 200.382, 268.132, 306.447, 458.814);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, new[] { 14, 25, 4 }, 6, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sc70_1 : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\smt sc70\src_ng";
        public static double[] region_coord = new[] { 126.507, 235.61, 240.983, 421.285 };
        public static int[] contrast = new[] { 25, 45, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sc70_1()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SC70 88-8units-clean__1__000_LA23040203-1234_0000.png", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sc89 : work.cv.TemplateMatch
    {
        public xray_sc89()
        {
            Mat img_temp = new Mat(@"D:/桌面/新建文件夹/SC89-1235/LF-SC89 SOT723-24units-all__1__000_723-1235_0000.jpg", ImreadModes.Grayscale);
            HOperatorSet.GenRectangle1(out HObject ModelRegion, 173.703, 298.48, 241.394, 440.055);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, new[] { 25, 30, 4 }, 6, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sod123 : work.cv.TemplateMatch
    {
        public xray_sod123()
        {
            Mat img_temp = new Mat(@"D:/桌面/新建文件夹/sod123 sc123-1213/LF-SOD123-12units-ALL__1__000_sc123-1213_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, 144.976, 216.532, 359.287, 332.475);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, new[] { 11, 16, 4 }, 5, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
        public Size size = new(332.475 - 216.532, 359.287 - 144.976);
    }
    class xray_sod323 : TemplateMatch, IIc
    {
        public string data_dir_path => "D:/桌面/新建文件夹/sod323-111";
        public static double[] region_coord = new[] { 177.491, 236.672, 330.402, 344.531 };
        public static int[] contrast = new[] { 12, 24, 4 };
        public static int mincontrast = 5;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sod323()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOD323-12units-all__1__000_323-111_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }

    }
    class xray_sot23e : TemplateMatch, IIc
    {
        //public string data_dir_path = "\\\\192.168.11.10\\Public\\HuangRX\\X-RAY\\银浆焊 sot23e\\SOT23E-1237";
        public static double[] region_coord = new[] { 316.355, 556.537, 503.409, 662.145 };
        public static int[] contrast = new[] { 19, 26, 4 };
        public static int mincontrast = 3;
        //public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);



        //Size IIc.size => throw new NotImplementedException();

        public string data_dir_path => "\\\\192.168.11.10\\Public\\HuangRX\\X-RAY\\银浆焊 sot23e\\SOT23E-1237";

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);

        public xray_sot23e()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT23E-12units-all__1__000_SOT23E-1237_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class 锡膏检测 : TemplateMatch, IIc
    {
        public string data_dir_path => "\\\\192.168.11.12\\自动化\\刘林\\temp\\aaaaa";
        public static double[] region_coord = new[] { 226.32, 207.482, 302.414, 265.998 };
        //public static int[] contrast = new[] { 17, 21, 4 };
        //public static int mincontrast = 4;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public 锡膏检测()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\bad_20220929_024907_924_c37541.bmp", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);
            var dis = CreateNccModel(img_temp, ModelRegion);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sod523 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\work\files\deeplearn_datasets\x-ray\obj-det\sod523";
        public static double[] region_coord = new[] { 294.994, 540.787, 412.348, 616.143 };
        public static int[] contrast = new[] { 15, 27, 4 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sod523()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\1---2-12_034_1.33_FAIL.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sod723 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\work\files\deeplearn_datasets\x-ray\obj-det\sod723";
        public static double[] region_coord = new[] { 184.549, 106.406, 254.157, 218.014 };
        public static int[] contrast = new[] { 16, 39, 4 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sod723()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SC89 SOT723-24units-all__1__000_TEST-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey(1);
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sod23lc : TemplateMatch, IIc
    {
        public string data_dir_path => @"//192.168.11.10/Public/HuangRX/X-RAY/银浆焊 sot23lc/SOT23LC1237";
        public static double[] region_coord = new[] { 334.842, 718.912, 594.006, 857.916 };
        public static int[] contrast = new[] { 12, 21, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sod23lc()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT2526LC-9units-all__1__000_SOT23LC1237_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sod23lc_1 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\xray数据\smt1\SOT23LC-NG";
        public static double[] region_coord = new[] {  306, 1104, 545d, 1404 };
        public static int[] contrast = new[] { 22, 51, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sod23lc_1()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT2526LC-9units-all__1__007_SOT23LC02_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.7, scaleMax: 1/0.7);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sot23 : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\smt sot23\retrain3\det";
        public static double[] region_coord = new[] { 399.344, 649.157, 500.396, 901.083 };
        public static int[] contrast = new[] { 12, 21, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot23()
        {
            Mat img_temp = new Mat(@$"{@"\\192.168.11.10\Public\HuangRX\X-RAY\smt sot23\retrain1\det"}\LF-SOT23-9units-clean__1__024_LA23020689-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    //17.9786, 303.962, 270.159, 438.426
    class xray_sot25 : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\银浆焊 sot25\SOT251235";
        public static double[] region_coord = new[] { 17.9786, 303.962, 270.159, 438.426 };
        public static int[] contrast = new[] { 12, 21, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot25()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT2526LC-9units-all__1__000_SOT251235_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sot25_1 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\sss";
        public string data_dir_path1 => @"\\192.168.11.10\Public\HuangRX\X-RAY\银浆焊 sot25\SOT25 SW";
        public static double[] region_coord = new[] { 19.9225, 292.365, 267.523, 424.329 };
        public static int[] contrast = new[] { 12, 21, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot25_1()
        {
            Mat img_temp = new Mat(@$"{data_dir_path1}\LF-SOT2526LC-9units-all__1__000_LA22493927-02_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sot26 : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\银浆焊 sot26\SOT26 SW";
        public static double[] region_coord = new[] { 8.96439, 296.582, 259.153, 434.584 };
        public static int[] contrast = new[] { 20, 41, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot26()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT2526LC-9units-all__1__000_26 SW12_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sot26_1 : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\银浆焊 sot26\SOT261236";
        public static double[] region_coord = new[] { 44.3358, 291.407, 290.211, 427.684 };
        public static int[] contrast = new[] { 20, 41, 8 };
        public static int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot26_1()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\LF-SOT2526LC-9units-all__1__000_SOT261236_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    //class xray_dfn_1610 : TemplateMatch, IIc
    //{
    //    public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\DFN\DFN1610";
    //    public static double[] region_coord = new[] { 238.915, 805.225, 424.296, 928.626 };
    //    public static int[] contrast = new[] { 8, 50, 20 };
    //    public static int mincontrast = 10;

    //    public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
    //    public xray_dfn_1610()
    //    {
    //        Mat img_temp = new Mat(@$"{data_dir_path}\01_001_1.0_FAIL.jpg", ImreadModes.Grayscale);

    //        HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
    //        var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

    //        Cv2.ImShow("dis", dis);
    //        Cv2.WaitKey();
    //        Cv2.DestroyAllWindows();
    //    }
    //}
    //349,162,428,280

    //class xray_dfn_SOD882 : TemplateMatch, IIc
    //{
    //    public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\DFN\SOD882";
    //    public static double[] region_coord = new[] { 162d, 349d, 280d, 428d };
    //    public static int[] contrast = new[] { 8, 50, 20 };
    //    public static int mincontrast = 10;

    //    public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
    //    public xray_dfn_SOD882()
    //    {
    //        Mat img_temp = new Mat(@$"{data_dir_path}\01_001_1.0_FAIL.jpg", ImreadModes.Grayscale);

    //        HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
    //        var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

    //        Cv2.ImShow("dis", dis);
    //        Cv2.WaitKey();
    //        Cv2.DestroyAllWindows();
    //    }
    //}

    class xray_dfn_SOD0603 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\work\files\deeplearn_datasets\x-ray\obj-det\dfn0603\train";//D:\desktop\xray数据\dfn\DFN0603
        public double[] region_coord = new[] { 255,561d, 334, 601  };//{ 149d, 379d, 240d, 433d };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_dfn_SOD0603()
        {
            var img_temp = new Mat(@$"{data_dir_path}\01_007_1.6_FAIL.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    class xray_dfn_DFN1610 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\dfn\DFN1610";
        public double[] region_coord = new[] { 52d, 308, 237, 432 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_dfn_DFN1610()
        {
            var img_temp = new Mat(@$"{data_dir_path}\01_001_1.0_FAIL.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    
    class xray_dfn_SOD882 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\dfn\SOD882";
        public double[] region_coord = new[] { 162d, 193, 281, 270 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_dfn_SOD882()
        {
            var img_temp = new Mat(@$"{data_dir_path}\01_001_1.0_FAIL.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    class xray_dfn_SOD883 : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\dfn\SOT883";
        public double[] region_coord = new[] { 196d, 560d, 309d, 634d };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_dfn_SOD883()
        {
            var img_temp = new Mat(@$"{data_dir_path}\01_001_1.0_FAIL.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 0);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    #endregion

    #region 卷盘类
    class xray_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => "D:\\桌面\\新建文件夹";
        public static double[] region_coord = new[] { 216.825, 777.231, 456.29, 803.118 };
        public static int[] contrast = new[] { 40, 73, 4 };
        public static int mincontrast = 3;
        public Size size => new(268, 104);//new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
        public xray_juanpan()
        {
            Mat img_temp = new Mat(@$"D:\桌面\JP\JuanPan__1__152_SW-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1, TemplateAngle: 0);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_juanpan_ncc : TemplateMatch, IIc
    {
        public static double[] region_coord = new[] { 320.551, 1006.42, MathExp.Rad(95.7475), 112.918, 45.7898 };
        //public static int[] contrast = new[] { 12, 24, 4 };
        //public static int mincontrast = 5;

        public string data_dir_path => "D:/桌面/juanpan1";
        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_juanpan_ncc()
        {
            Mat img_temp = new Mat(@$"{data_dir_path}\REEL-X-RAY-ALL__1__106_TEST-01_0000.jpg", ImreadModes.Grayscale);
            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);//
            var dis = CreateNccModel(img_temp, ModelRegion);

            Cv2.ImShow("dis", dis);
            Cv2.WaitKey(300);
            Cv2.DestroyAllWindows();
        }
    }

    class xray_sot23_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\卷盘 sot23\LA22171622-28";
        public double[] region_coord = new[] { 131.391, 718.695, MathExp.Rad(-0.266538), 172.6, 66.2477 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sot23_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\SOT23 20210621__1__1713_LA22171622-28_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sc70_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\卷盘 sc70\2.4SC70-01";
        public double[] region_coord = new[] { 198.446, 1127.02, MathExp.Rad(-18.4308), 95.5052, 49.2835 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sc70_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\REEL-X-RAY-ALL__1__000_2.4SC70-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sc88_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"\\192.168.11.10\Public\HuangRX\X-RAY\卷盘 sc88\2.4 SC88-01";
        public double[] region_coord = new[] { 226.494, 1130.51, MathExp.Rad(-17.3797), 99.0642, 47.0692 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sc88_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\REEL-X-RAY-ALL__1__000_2.4 SC88-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    class xray_sot23lc_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\卷盘 SOT23LC";
        public double[] region_coord = new[] { 165.017, 1157.89, 259.483, 1413.93 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3]- region_coord[1], region_coord[2] - region_coord[0]);
        public xray_sot23lc_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\sot23lc.jpg", ImreadModes.Grayscale);

            //HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    //class xray_sot25_juanpan : TemplateMatch, IIc
    //{
    //    public string data_dir_path => @"D:\desktop\xray\sot25";
    //    public double[] region_coord = new[] { 239.664, 1033.29, MathExp.Rad(-91.972), 54.458, 129.304 };
    //    public int[] contrast = new[] { 20, 41, 8 };
    //    public int mincontrast = 3;

    //    public Size size => new(region_coord[4] * 2, region_coord[3] * 2);
    //    public xray_sot25_juanpan()
    //    {
    //        var img_temp = new Mat(@$"{data_dir_path}\sot25.jpg", ImreadModes.Grayscale);

    //        HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
    //        //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
    //        var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
    //        Cv2.ImShow("dis", dis);
    //        Cv2.WaitKey();
    //        Cv2.DestroyAllWindows();
    //    }
    //}

    //class xray_sot26_2 : TemplateMatch, IIc
    //{
    //    public string data_dir_path => @"D:\desktop\SOT26-lp";
    //    //public static double[] region_coord = new[] { 336.959, 652.485, 610.185, 913.941 };
    //    public static double[] region_coord = new[] { 336.959, 652.485, 610.185, 913.941 };
    //    public static int[] contrast = new[] { 20, 41, 8 };
    //    public static int mincontrast = 3;

    //    public Size size => new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
    //    public xray_sot26_2()
    //    {
    //        Mat img_temp = new Mat(@$"{data_dir_path}\a.jpg", ImreadModes.Grayscale);

    //        HOperatorSet.GenRectangle1(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3]);
    //        var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.9, scaleMax: 1.1);

    //        Cv2.ImShow("dis", dis);
    //        Cv2.WaitKey();
    //        Cv2.DestroyAllWindows();
    //    }
    //}

    class xray_sot26_2_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\xray\误报率高\LA23124275-12";
        public double[] region_coord = new[] { 473.574, 787.358, MathExp.Rad(-88.971), 130, 130 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[4] * 2, region_coord[3] * 2);
        public xray_sot26_2_juanpan()
        {
            var img_temp = new Mat(@$"D:\desktop\xray\SOT26-lp-1\a.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            //var dis = CreateScaledShapeModel(img_temp, ModelRegion, contrast, mincontrast, scaleMin: 0.5, scaleMax: 2.0);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    //314,1148.5,1,252,91
    class xray_sot23e_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\xray数据\smt\sot23e_juanpan";
        public double[] region_coord = new[] { 314, 1148.5, MathExp.Rad(-91), 91 / 2.0, 252 / 2.0 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sot23e_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\REEL-X-RAY-ALL__1__002_LA23E-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }

    //689.5,,5,254,98
    class xray_sot25_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\xray数据\smt\sot25_juanpan";
        public double[] region_coord = new[] { 224, 689.5, MathExp.Rad(-95), 106 / 2.0, 256 / 2.0 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sot25_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\sot25.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    //1140.5,162,1,255,122
    class xray_sot26_juanpan : TemplateMatch, IIc
    {
        public string data_dir_path => @"D:\desktop\xray数据\smt\sot26_juanpan";
        public double[] region_coord = new[] { 162, 1140.5, MathExp.Rad(-91), 122 / 2.0, 256 / 2.0 };
        public int[] contrast = new[] { 20, 41, 8 };
        public int mincontrast = 3;

        public Size size => new(region_coord[3] * 2, region_coord[4] * 2);
        public xray_sot26_juanpan()
        {
            var img_temp = new Mat(@$"{data_dir_path}\REEL-X-RAY-ALL__1__002_SOT26-01_0000.jpg", ImreadModes.Grayscale);

            HOperatorSet.GenRectangle2(out HObject ModelRegion, region_coord[0], region_coord[1], region_coord[2], region_coord[3], region_coord[4]);
            var dis = CreateNccModel(img_temp, ModelRegion, 0, 180);
            Cv2.ImShow("dis", dis);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
        }
    }
    #endregion

}
