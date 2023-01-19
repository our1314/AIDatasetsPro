using OpenCvSharp;

var img_ext = new[] { ".png", ".jpg", ".bmp", ".jif" };
var target_ext = ".jpg";

Console.WriteLine($"将路径下的图像全部改为{target_ext}格式，输入文件夹路径：(可以拖拽!)");
var path = Console.ReadLine();
var files = new DirectoryInfo(path).GetFiles();
files = files.Where(f => img_ext.Contains(f.Extension) && f.Extension != target_ext).ToArray();

foreach (var f in files)
{
    var img = new Mat(f.FullName, ImreadModes.Unchanged);
    img.SaveImage(f.FullName.Replace(f.Extension, target_ext));
}
