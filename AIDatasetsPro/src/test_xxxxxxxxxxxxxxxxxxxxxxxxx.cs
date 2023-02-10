using OpenCvSharp;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_xxxxxxxxxxxxxxxxxxxxxxxxx : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入图像文件：");
            var path = Console.ReadLine().Replace("\"", "");
            var src = new Mat(path, ImreadModes.Grayscale);

            for (int i = 0; i < 50; i++)
            {
                var scale_start = 0.7;
                var scale_end = 1 / 0.7;

                var scale = scale_start + (scale_end - scale_start) * new Random().NextDouble();

                var img = src.Resize(new Size(), scale, scale);
                img = img - 100;
                var name = Path.GetFileNameWithoutExtension(path);
                img.ImSave(path.Replace(name, work.Work.Now));
            }
        }
    }
}
