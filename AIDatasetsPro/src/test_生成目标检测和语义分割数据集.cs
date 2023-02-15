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
            var cnt_sum = 100;

            // 每张背景图的贴图数量
            var cnt_perimg = 1;


            for (int i = 0; i < cnt_sum; i++)
            {
                var result_labels = "";
                // 0、随机获取背景图
                var index_back = new Random().Next(files_fore.Length);
                var back = new Mat(files_back[index_back].FullName, ImreadModes.Color);
                var back_mask = back.EmptyClone().CvtColor(ColorConversionCodes.BGR2GRAY);
                var color_mask = back.EmptyClone();

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
                    color_mask.SetTo(new Scalar(0, 0, 128), back_mask);

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

                {
                    back = back.Resize(new Size(), 0.3, 0.3);
                    var maxlen = Math.Max(back.Width, back.Height);
                    var padup = (maxlen - back.Height) / 2;
                    var padleft = (maxlen - back.Width) / 2;
                    back = back.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant);

                    color_mask = color_mask.Resize(new Size(), 0.3, 0.3, InterpolationFlags.Nearest);
                    maxlen = Math.Max(color_mask.Width, color_mask.Height);
                    padup = (maxlen - color_mask.Height) / 2;
                    padleft = (maxlen - color_mask.Width) / 2;
                    color_mask = color_mask.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant);
                }

                back.ImSave(@$"{path_images}\{name}.jpg");
                result_labels.Trim().StrSave(@$"{path_labels}\{name}.txt");
                //back_mask.ImSave(@$"{path_masks}\{name}.png");
                color_mask.ImSave(@$"{path_masks}\{name}.png");

                //6、显示
                CV.ImShow("dis", back);
                Cv2.WaitKey(1);
            }

            Cv2.DestroyAllWindows();

            var files_list = new DirectoryInfo(path_images).GetFiles().ToList();
            string train = "", val = "";
            var thr = (int)(files_list.Count * 0.7);
            for (int i = 0; i < files_list.Count; i++)
            {
                if (i < thr)
                    train += Path.GetFileNameWithoutExtension(files_list[i].Name) + "\r\n";
                else
                    val += Path.GetFileNameWithoutExtension(files_list[i].Name) + "\r\n";
            }

            File.WriteAllText(@$"{path}\out\train.txt", train.Trim());
            File.WriteAllText(@$"{path}\out\val.txt", val.Trim());


            #region 随机验证
            {
                var image_files = Directory.GetFiles(path_images);
                var image_masks = Directory.GetFiles(path_masks);

                for (int i = 0; i < 5; i++)
                {
                    var rand_index = new Random().Next(0, image_files.Length);
                    var img = new Mat(image_files[rand_index], ImreadModes.Color);
                    var mask = new Mat(image_masks[rand_index], ImreadModes.Color);
                    mask.SetTo(Scalar.Red, mask);

                    img = img * 0.5 + mask * 0.5;
                    CV.ImShow("dis", img);
                    Cv2.WaitKey(200);
                }
            }
            #endregion

            Cv2.DestroyAllWindows();
        }
    }
}
