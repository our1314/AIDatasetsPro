using OpenCvSharp;

//var searchimage = @"D:\work\proj\xray\test_learn_python\image_classification\cnn_imgcls\run\train\exp_xray_sot23_juanpan\val_fail_img\1.png";
//var searchpath = @"D:\work\files\deeplearn_dataset\x-ray\cls-dataset\sot23_juanpan\train\ok";

Console.WriteLine("输入搜索图像的路径：");
var searchimage = Console.ReadLine();
Console.WriteLine("输入搜索文件夹路径：");
var searchpath = Console.ReadLine();


//1、被搜索图像
var temp_src = new Mat(searchimage, ImreadModes.Unchanged);
var temp = temp_src.Clone();

//2、搜索
var images = new DirectoryInfo(searchpath).GetFiles("*", searchOption: SearchOption.AllDirectories);
images = images.OrderBy((p) =>
{
    var img = new Mat(p.FullName, ImreadModes.Unchanged);
    Mat diff = new Mat();

    img = img.Resize(temp.Size());
    img.ConvertTo(img, MatType.CV_8S);
    temp.ConvertTo(temp, MatType.CV_8S);

    Cv2.Subtract(temp, img, diff);
    Mat abs = diff.Abs();
    var sum = abs.Sum();
    return sum.Val0;
}).ToArray();

//3、最相似的图像
var f1 = new Mat(images.First().FullName, ImreadModes.Unchanged);
f1 = f1.Resize(temp_src.Size());

//4、显示
var dis = new Mat();
Cv2.HConcat(temp_src, f1, dis);
dis = dis.CopyMakeBorder(0, 40, 0, 0, BorderTypes.Constant);
dis.PutText(images[0].FullName, new Point(0, dis.Height - 10), HersheyFonts.HersheyComplex, 0.3, Scalar.Red, 1); ;
Cv2.ImShow(images[0].Name, dis);
Cv2.WaitKey();