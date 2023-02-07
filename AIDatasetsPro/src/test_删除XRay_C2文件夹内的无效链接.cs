using work.test;

namespace AIDatasetsPro.src
{
    internal class test_删除XRay_C2文件夹内的无效链接 : ConsoleTestBase
    {
        public override void RunTest()
        {
            string path = @"\\172.17.40.125\xray";
            Console.WriteLine($"正在获取.lnk文件列表……");
            var files = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories);
            var count = files.Length;

            Console.WriteLine($".lnk文件数量为：{files.Length}");
            files = files.Where(f => f.Extension == ".lnk").ToArray();
            Array.ForEach(files, f =>
            {
                File.Delete(f.FullName);
                Console.WriteLine($"已删除：{f.Name}");
            });
            Console.WriteLine($"完成删除！数量为：{count}");
            Console.ReadKey();
        }
    }
}
