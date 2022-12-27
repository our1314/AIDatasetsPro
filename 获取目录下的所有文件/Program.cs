Console.WriteLine("输入文件夹路径：(可以拖拽!)");
var path = Console.ReadLine();
var a = new DirectoryInfo(path).GetFiles();
var files = a.Select(p => p.Name).ToArray();
var f = string.Join("\r\n", files);
File.WriteAllText($"{AppContext.BaseDirectory}\\files.txt", f.Trim());
Console.WriteLine(f.Trim());
Console.WriteLine(@$"files.txt文件已保存在{AppContext.BaseDirectory}");

Console.ReadKey();