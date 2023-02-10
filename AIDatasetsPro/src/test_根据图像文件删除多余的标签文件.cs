using work.test;

namespace AIDatasetsPro.src
{
    internal class test_根据图像文件删除多余的标签文件 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入数据路径（图像与标签在一个文件夹）：");
            var path = Console.ReadLine().Trim();
            var files = new DirectoryInfo(path).GetFiles();

            var images = files.Where(f => f.Extension == ".png" || f.Extension == ".png" || f.Extension == ".png").ToList();
            var labels = files.Where(f => f.Extension == ".txt" && f.Name != "classes.txt").ToList();

            var 多余文件 = labels.Where(f => !images.Any(i => Path.GetFileNameWithoutExtension(i.Name) == Path.GetFileNameWithoutExtension(f.Name))).ToList();
            多余文件.ForEach(f =>
            {
                Console.WriteLine($"已删除：{f.Name}");
                File.Delete(f.FullName);
            });
            Console.ReadKey();
        }
    }
}
