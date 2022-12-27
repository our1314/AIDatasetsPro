Console.WriteLine("输入文件夹路径：");
var path = Console.ReadLine();
var a = new DirectoryInfo(path).GetFiles();
var files = a.Select(p => Path.GetFileNameWithoutExtension(p.Name)).ToArray();
var f = string.Join("\r\n", files);
File.WriteAllText($"{path}\\files.txt", f.Trim());