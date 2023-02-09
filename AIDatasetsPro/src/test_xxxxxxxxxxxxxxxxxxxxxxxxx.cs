using OpenCvSharp;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_xxxxxxxxxxxxxxxxxxxxxxxxx : ConsoleTestBase
    {
        public override void RunTest()
        {
            //var img_files = new DirectoryInfo(@"..\..\..\data").GetFiles();
            //img_files = img_files.Where(f => f.FullName.EndsWith(".jpg") || f.FullName.EndsWith(".bmp") || f.FullName.EndsWith(".png")).ToArray();

            //foreach (var f in img_files)
            //{

            //}
            var scale_start = 0.7;
            var scale_end = 1 / 0.7;

            var ss = scale_end - scale_start;
            var scale = scale_start + ss * new Random().NextDouble();

            var p = Console.ReadLine().Trim();
            p = p.Replace("\"", "");

            var img = new Mat(p, ImreadModes.Unchanged);
            img = img.Resize(new Size(), scale_end, scale_end);
            var name = Path.GetFileNameWithoutExtension(p);
            img.ImSave(p.Replace(name, work.Work.Now));

        }
    }
}
