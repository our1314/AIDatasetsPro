// See https://aka.ms/new-console-template for more information
Console.WriteLine("输入label路径：");

var labels_path = Console.ReadLine();

var files = new DirectoryInfo(labels_path).GetFiles();
files = files.Where(f => f.Extension == ".txt" && f.Name != "classes.txt").ToArray();

foreach (var f in files)
{
    var txt = File.ReadAllText(f.FullName).Trim();
    var lines = txt.Split(new[] { "\n" ,"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
    for (int j = 0; j < lines.Length; j++)
    {
        var d = lines[j].Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (d.Length < 5) continue;

        if (d[0] == "24")
        {
            d[0] = "";
            d[1] = "";
            d[2] = "";
            d[3] = "";
            d[4] = "";
            lines[j] = string.Join(" ", d);
        }
        else
        {
            d[0] = "0";
            lines[j] = string.Join(" ", d);
        }

    }
    lines = lines.Where(l => l.Trim() != "").ToArray();
    txt = string.Join("\n", lines).Trim();
    File.WriteAllText(f.FullName, txt);
}

