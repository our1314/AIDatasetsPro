using AIDatasetsPro.core;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_将yolo标签全改为0 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入label路径：");

            var labels_path = Console.ReadLine();

            var files = new DirectoryInfo(labels_path).GetFiles();
            files = files.Where(f => f.Extension == ".txt" && f.Name != "classes.txt").ToArray();

            foreach (var f in files)
            {
                var txt = File.ReadAllText(f.FullName).Trim();
                var lines = Base.yolostr2doublearray(txt);
                for (int i = 0; i < lines.Count; i++)
                {
                    var (label, x0, y0, w, h) = lines[i];
                    lines[i] = (0, x0, y0, w, h);
                }
                txt = Base.doublearray2yolostr(lines);
                File.WriteAllText(f.FullName, txt);
                Console.WriteLine(f.FullName);
            }
        }
    }
}
