using HalconDotNet;
using OpenCvSharp;
using work;
using work.cv;
using work.math;

var ic = new xray_sod23lc();//new xray_sod523();//new xray_juanpan_ncc();//new xray_sot23e();// new xray_sod323();//new xray_juanpan_ncc();//new xray_juanpan();//new xray_sc88();//
var data_dir_path = ic.data_dir_path;
var img_files = new DirectoryInfo(data_dir_path).GetFiles();
img_files = img_files.Where(f => f.FullName.EndsWith(".jpg") || f.FullName.EndsWith(".bmp") || f.FullName.EndsWith(".png")).ToArray();

HOperatorSet.SetSystem("border_shape_models", "true");
var anchor = ic.size;//new Size(juanpan.size.Width,juanpan.size.Height+10);//

foreach (var f in img_files)
{
    Mat src = new Mat(f.FullName, ImreadModes.Grayscale);
    var dis = src.CvtColor(ColorConversionCodes.GRAY2BGR);
    //int border = 300;
    //Mat img = src.CopyMakeBorder(border, border, border, border, BorderTypes.Constant, 0);

    var result_match = ic.FindModel(src, 0.7, 0, out _, out _, MaxOverlap: 0);
    if (result_match == null) continue;

    var yolo_json = "";
    foreach (var p in result_match)
    {
        var x0 = (int)p[0];
        var y0 = (int)p[1];
        var angle = 0d;
        var scale = p[3];
        angle = angle * Math.PI / 180d;
        var anchor1 = new Size(anchor.Width * scale, anchor.Height * scale);
        CV.DrawRotateRect(ref dis, new Point(x0, y0), anchor1, angle, scale);

        #region 保存ROI
        {
            var roi = CV.GetRotateROI(src, new Point(x0, y0), anchor1, angle);
            roi.ImSave(@$"{data_dir_path}\ROI\{Work.Now}.png");
        }
        #endregion

        var cx = (double)x0 / src.Width;
        var cy = (double)y0 / src.Height;
        var w = (double)anchor1.Width / src.Width;
        var h = (double)anchor1.Height / src.Height;

        yolo_json += $"0 {cx} {cy} {w} {h}\r\n";
    }
    yolo_json = yolo_json.Trim();

    var save_path = f.FullName.Replace(".jpg", ".txt");
    File.WriteAllText(save_path, yolo_json);

    Cv2.ImShow("dis", dis);
    Cv2.WaitKey(1);
}

//在目录下保存classes.txt，LabelImg需要此文件。
File.WriteAllText(@$"{data_dir_path}\classes.txt", "0");


class xray_juanpan : work.cv.TemplateMatch
{
    public string data_dir_path = "D:\\桌面\\新建文件夹";
    public static double[] region_coord = new[] { 216.825, 777.231, 456.29, 803.118 };
    public static int[] contrast = new[] { 40, 73, 4 };
    public static int mincontrast = 3;
    public Size size = new(268, 104);//new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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
class xray_juanpan_ncc : work.cv.TemplateMatch
{
    public static double[] region_coord = new[] { 320.551, 1006.42, MathExp.Rad(95.7475), 112.918, 45.7898 };
    //public static int[] contrast = new[] { 12, 24, 4 };
    //public static int mincontrast = 5;

    public string data_dir_path = "D:/桌面/juanpan1";
    public Size size = new(region_coord[3] * 2, region_coord[4] * 2);
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
class xray_sc88 : work.cv.TemplateMatch
{
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
class xray_sc70 : work.cv.TemplateMatch
{
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
class xray_sod323 : TemplateMatch
{
    public string data_dir_path = "D:/桌面/新建文件夹/sod323-111";
    public static double[] region_coord = new[] { 177.491, 236.672, 330.402, 344.531 };
    public static int[] contrast = new[] { 12, 24, 4 };
    public static int mincontrast = 5;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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
class xray_sot23e : TemplateMatch
{
    public string data_dir_path = "D:\\桌面\\SOT23E-1237";
    public static double[] region_coord = new[] { 316.355, 556.537, 503.409, 662.145 };
    public static int[] contrast = new[] { 19, 26, 4 };
    public static int mincontrast = 3;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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
class 锡膏检测 : TemplateMatch
{
    public string data_dir_path = "\\\\192.168.11.12\\自动化\\刘林\\temp\\aaaaa";
    public static double[] region_coord = new[] { 226.32, 207.482, 302.414, 265.998 };
    //public static int[] contrast = new[] { 17, 21, 4 };
    //public static int mincontrast = 4;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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
class xray_sod523 : TemplateMatch
{
    public string data_dir_path = @"D:\work\files\deeplearn_datasets\x-ray\obj-det\sod523";
    public static double[] region_coord = new[] { 294.994, 540.787, 412.348, 616.143 };
    public static int[] contrast = new[] { 15, 27, 4 };
    public static int mincontrast = 3;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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

class xray_sod723 : TemplateMatch
{
    public string data_dir_path = @"D:\work\files\deeplearn_datasets\x-ray\obj-det\sod723";
    public static double[] region_coord = new[] { 184.549, 106.406, 254.157, 218.014 };
    public static int[] contrast = new[] { 16, 39, 4 };
    public static int mincontrast = 3;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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

class xray_sod23lc : TemplateMatch
{
    public string data_dir_path = @"//192.168.11.10/Public/HuangRX/X-RAY/银浆焊 sot23lc/SOT23LC1237";
    public static double[] region_coord = new[] { 344.305, 289.684, 593.502, 431.701 };
    public static int[] contrast = new[] { 12, 21, 8 };
    public static int mincontrast = 3;

    public Size size = new(region_coord[3] - region_coord[1], region_coord[2] - region_coord[0]);
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