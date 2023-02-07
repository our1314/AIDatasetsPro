using OpenCvSharp;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_生成目标检测和语义分割数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("给定目标文件夹路径，其中包含前景图(forexxx.png，四通道图像（透明）)和背景图(格式任意)，可以多个");
            Console.WriteLine("输入包含前景图和背景图的文件夹路径：");

            var path = Console.ReadLine();

            // 创建相关目录
            var path_images = @$"{path}\out\images";
            var path_labels = @$"{path}\out\labels";
            var path_masks = @$"{path}\out\masks";
            Directory.CreateDirectory(path_images);
            Directory.CreateDirectory(path_labels);
            Directory.CreateDirectory(path_masks);

            // 提取前景图和背景图的文件名
            var files = new DirectoryInfo(path).GetFiles();
            var files_fore = files.Where(f => f.Name.Contains("fore")).ToArray();
            var files_back = files.Where(f => f.Name.Contains("back")).ToArray();

            // 生成图像的总数
            var cnt_sum = 10;

            // 每张背景图的贴图数量
            var cnt_perimg = 1;


            for (int i = 0; i < cnt_sum; i++)
            {
                var result_labels = "";
                // 0、随机获取背景图
                var index_back = new Random().Next(files_fore.Length);
                var back = new Mat(files_back[index_back].FullName, ImreadModes.Color);
                var back_mask = back.EmptyClone().CvtColor(ColorConversionCodes.BGR2GRAY);

                for (int j = 0; j < cnt_perimg; j++)
                {
                    //1、随机获取前景图
                    var index_fore = new Random().Next(files_fore.Length);
                    var fore = new Mat(files_fore[index_fore].FullName, ImreadModes.Unchanged);

                    //2、将四通道的前景图拆分出bgr图像和mask图像
                    var fore_chls = fore.Split();
                    var bgr = new Mat();
                    Cv2.Merge(fore_chls.Take(3).ToArray(), bgr);
                    var mask = fore_chls.Last();

                    //3、生成随机坐标，并贴图
                    var row = new Random().Next(0, back.Rows - fore.Rows);//不会生成最大值
                    var col = new Random().Next(0, back.Cols - fore.Cols);

                    var rect = new Rect(col, row, fore.Cols, fore.Rows);

                    bgr.CopyTo(back[rect], mask);//将前景图贴在背景图上
                    mask.CopyTo(back_mask[rect], mask);//将mask贴在同样尺寸的黑色图像上

                    //4、计算label
                    double x1 = col;
                    double y1 = row;
                    double w = fore.Cols;
                    double h = fore.Rows;

                    var cx = x1 + w / 2.0;//贴图中心坐标
                    var cy = y1 + h / 2.0;//贴图中心坐标

                    var label_yolo = $"0 {(cx / back.Width).ToString("F12")} {(cy / back.Height).ToString("F12")} {(w / back.Width).ToString("F12")} {(h / back.Height).ToString("F12")}";
                    result_labels += label_yolo + "\r\n";
                }

                //5、保存
                var name = work.Work.Now;
                back.ImSave(@$"{path_images}\{name}.png");
                result_labels.Trim().StrSave(@$"{path_labels}\{name}.txt");
                back_mask.ImSave(@$"{path_masks}\{name}.png");

                //6、显示
                var dis = back.Clone();
                var max = Math.Max(dis.Width, dis.Height);
                var dsize = 1000.0;
                if (max > dsize)
                {
                    double fx = dsize / max;
                    dis = dis.Resize(new Size(), fx, fx);
                }
                Cv2.ImShow("dis", dis);
                Cv2.WaitKey(1);
            }

        }
    }
}
