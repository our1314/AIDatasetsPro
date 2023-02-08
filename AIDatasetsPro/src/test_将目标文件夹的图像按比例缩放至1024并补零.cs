using OpenCvSharp;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_将目标文件夹的图像按比例缩放至1024并补零 : ConsoleTestBase
    {
        public override void RunTest()
        {
            var dir = @"D:\桌面\JP";
            double k = 1024d / 1536d;

            var files_images = new DirectoryInfo(dir).GetFiles();
            files_images = files_images.Where(f => f.Extension == ".jpg" || f.Extension == ".png" || f.Extension == ".bmp").ToArray();

            foreach (var file in files_images)
            {
                var src = new Mat(file.FullName, ImreadModes.Unchanged);
                src = src.Resize(new Size(), k, k);
                src = src.CopyMakeBorder(0, 1024 - src.Height, 0, 0, BorderTypes.Constant);
                //var mask = src.Threshold(240, 255, ThresholdTypes.Binary);
                //src = src.SetTo(0, mask);
                src.ImSave($@"{dir}\out\{Path.GetFileNameWithoutExtension(file.FullName)}.png");
            }
        }
    }
}
