using OpenCvSharp;
using work.cv;

namespace test_卷盘数据集处理
{
    internal class 将图像按比例缩放到1024并补零
    {

        public static void run()
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
