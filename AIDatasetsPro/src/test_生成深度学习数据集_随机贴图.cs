using AIDatasetsPro.core;
using OpenCvSharp;
using System.Threading.Tasks;
using work.ai;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_生成深度学习数据集_随机贴图 : ConsoleTestBase
    {
        enum 数据集类型
        {
            目标检测,
            图像分割,
            超分辨率重构
        }
        enum 贴图方式
        {
            直接贴图,
            图像融合
        }
        public override void RunTest()
        {
            Console.WriteLine("给定目标文件夹路径，其中包含前景图(forexxx.png，四通道图像（透明）)和背景图(格式任意)，可以多个");
            Console.WriteLine("输入包含前景图和背景图的文件夹路径：");

            #region 1、参数设置
            // 生成图像的总数
            var cnt_sum = 200;

            // 每张背景图的最大贴图数量
            var cnt_perimg_max = 1;

            //var colors = new[] { new Scalar(0, 0, 1), new Scalar(0, 128, 0), new Scalar(128, 0, 0),
            //                     new Scalar(0, 128, 128), new Scalar(128, 0, 128), new Scalar(128, 128, 0),
            //                     new Scalar(128, 128, 128) };

            var colors = new[] { 255 };

            var train_val_test = new[] { 0.5, 0.2, 0.3 };

            数据集类型 type_数据集类型 = 数据集类型.目标检测;
            贴图方式 type_贴图方式 = 贴图方式.图像融合;
            #endregion

            var path = Console.ReadLine().Trim();//读取贴图文件路径

            // 目录
            var path_root = @$"{path}\out1";
            var path_images = @$"{path_root}\train";//@$"{path_root}\images"
            var path_labels = @$"{path_root}\train";//@$"{path_root}\labels"
            var path_masks = @$"{path_root}\masks";//@$"{path_root}\masks"

            // 提取前景图和背景图的文件名
            var files = new DirectoryInfo(path).GetFiles();
            var files_fore = files.Where(f => f.Name.Contains("fore")).ToArray();
            var files_back = files.Where(f => f.Name.Contains("back")).ToArray();

            for (int i = 0; i < cnt_sum; i++)
            {
                var gen_seg_image = new Mat();
                var gen_seg_mask = new Mat();
                var gen_yolo_labels = "";

                // 0、随机获取一张背景图
                var index_back = new Random().Next(files_back.Length);
                var back = new Mat(files_back[index_back].FullName, ImreadModes.Color);//背景图像
                
                //对背景图进行增强
                {
                    //var r = new Random();
                    //var a = r.Next(-5, 5);
                    //back = CV.RotImage(back, a, InterpolationFlags.Linear, BorderTypes.Reflect101);
                    //Cv2.ImShow("dis", back);
                    //Cv2.WaitKey();
                }
                var back_src = back.Clone();


                var black = back.EmptyClone().CvtColor(ColorConversionCodes.BGR2GRAY).SetTo(0);//与背景图同样尺寸的黑色图像
                var file_name = Path.GetFileNameWithoutExtension(files_back[index_back].FullName);

                for (int j = 0; j < new Random().Next(1, cnt_perimg_max + 1); j++)
                {
                    //1、随机获取一张前景图
                    var random_index = new Random().Next(files_fore.Length);
                    var fore = new Mat(files_fore[random_index].FullName, ImreadModes.Unchanged);

                    //对前景图进行数据增强
                    {
                        var r = new Random();
                        var a = r.Next(1,2);
                        var ra = r.Next(a * 90 - 20, a * 90 + 20);
                        fore = CV.RotImage(fore, ra, InterpolationFlags.Linear, BorderTypes.Constant, Scalar.White);

                        var scale = r.NextDouble() * 0.4 + 0.7;
                        Cv2.Resize(fore, fore, new Size(), scale, scale);

                        var rr = new Rect(0, 0, Math.Min(fore.Width, back.Width), Math.Min(fore.Height, back.Height));
                        fore = fore[rr];
                    }

                    //2、将四通道的前景图拆分出bgr图像和mask图像
                    var fore_chls = fore.Split();
                    var bgr = new Mat();
                    Cv2.Merge(fore_chls.Take(3).ToArray(), bgr);
                    var mask = fore_chls.Last();

                    //3、生成随机坐标
                    var row = new Random().Next(0, back.Rows - fore.Rows);//不会生成最大值 var row = new Random().Next(112, 328);
                    var col = new Random().Next(0, back.Cols - fore.Cols);
                    var rect = new Rect(col, row, fore.Cols, fore.Rows);

                    var back_clone = new Mat();

                    //4、贴图生成目标图像
                    if (type_贴图方式 == 贴图方式.直接贴图)
                    {
                        bgr.CopyTo(back[rect], mask);//直接将前景图贴在背景图上，png第四通道作为mask
                    }
                    else if (type_贴图方式 == 贴图方式.图像融合)
                    {
                        bgr = bgr.Channels() == 1 ? bgr : bgr.CvtColor(ColorConversionCodes.BGR2GRAY);
                        back = back.Channels() == 1 ? back : back.CvtColor(ColorConversionCodes.BGR2GRAY);
                        back_clone = back.Clone();

                        var bgr_not = new Mat();
                        Cv2.BitwiseNot(bgr, bgr_not);//因为当前图像需要的前景是黑色的，因此先进行反色处理
                        var aaa = -new Random().NextDouble() * 0.1 + -0.1;
                        //Cv2.AddWeighted(back[rect], 1, bgr_not, -0.1, 0, back[rect]);
                        Cv2.AddWeighted(back[rect], 1, bgr_not, aaa, 0, back[rect]);
                    }

                    //5、生成标签
                    if(type_数据集类型 == 数据集类型.目标检测)
                    {
                        //将前景图贴上去，再把mask贴上去，再计算boundingbox获取真实label
                        black[rect].SetTo(255, mask);
                        var mask1 = getmask1(back);
                        var xxx = black.SetTo(0, mask1);

                        var rect1 = Cv2.BoundingRect(xxx);
                        if (rect1.X == 0 || rect1.Y == 0 || rect1.Width == 0 || rect1.Height == 0) continue;

                        var (x1, y1, w, h) = (rect1.X, rect1.Y, (double)rect1.Width, (double)rect1.Height);
                        var (cx, cy) = (x1 + w / 2.0, y1 + h / 2.0);//贴图中心坐标
                        var label_yolo = $"0 {(cx / back.Width):F6} {(cy / back.Height):F6} {(w / back.Width):F6} {(h / back.Height):F6}";
                        gen_yolo_labels += label_yolo + "\r\n";

                        //var labels = new Mat();
                        //var xx = Cv2.ConnectedComponentsEx(xxx, PixelConnectivity.Connectivity4);

                        //foreach(var ll in xx.Blobs)
                        //{
                        //    if (ll.Rect.X==0) continue;
                        //    var rect1 = ll.Rect;//

                        //    var (x1, y1, w, h) = (rect1.X, rect1.Y, (double)rect1.Width, (double)rect1.Height);
                        //    var (cx, cy) = (x1 + w / 2.0, y1 + h / 2.0);//贴图中心坐标
                        //    var label_yolo = $"0 {(cx / back.Width):F6} {(cy / back.Height):F6} {(w / back.Width):F6} {(h / back.Height):F6}";
                        //    gen_yolo_labels += label_yolo + "\r\n";
                        //}


                        //Cv2.ImShow("xxx", xxx);
                        //Cv2.WaitKey();

                        //生成yolo标签
                        //var (x1, y1, w, h) = (col, row, (double)fore.Cols, (double)fore.Rows);
                        //var (cx, cy) = (x1 + w / 2.0, y1 + h / 2.0);//贴图中心坐标
                        //var label_yolo = $"0 {(cx / back.Width):F6} {(cy / back.Height):F6} {(w / back.Width):F6} {(h / back.Height):F6}";
                        //gen_yolo_labels += label_yolo + "\r\n";
                    }
                    else if(type_数据集类型 == 数据集类型.图像分割)
                    {
                        //生成图像分割标签
                        black[rect].SetTo(colors[j], mask);//生成mask图像
                    }
                }

                //5、保存
                var name = work.Work.Now;
                if (type_数据集类型 == 数据集类型.目标检测)
                {
                    //保存前将多余区域覆盖
                    {
                        back = back.Channels() == 1 ? back : back.CvtColor(ColorConversionCodes.BGR2GRAY);
                        back_src = back_src.Channels() == 1 ? back_src : back_src.CvtColor(ColorConversionCodes.BGR2GRAY);

                        var mask = getmask(back);
                        back_src.CopyTo(back, mask);

                        //back.SetTo(Scalar.Black, mask);//将不需要的区域置为黑色
                        
                        
                    }

                    back.ImSave(@$"{path_images}\{name}_{file_name}.jpg");//图像文件
                    gen_yolo_labels.Trim().StrSave(@$"{path_labels}\{name}_{file_name}.txt");//标签文件yolo

                    var file_classes = @$"{path_labels}\classes.txt";//没有此文件labelImg会崩掉
                    if (!File.Exists(file_classes))
                    {
                        File.WriteAllText(file_classes, "0");
                    }

                    #region 生成VOC数据集需要的train.txt,val.txt
                    {
                        //var files_list = new DirectoryInfo(path_images).GetFiles().ToList();
                        //string train = "", val = "";
                        //var thr = (int)(files_list.Count * 0.7);
                        //for (int j = 0; j < files_list.Count; j++)
                        //{
                        //    if (j < thr)
                        //        train += Path.GetFileNameWithoutExtension(files_list[j].Name) + "\r\n";
                        //    else
                        //        val += Path.GetFileNameWithoutExtension(files_list[j].Name) + "\r\n";
                        //}

                        //File.WriteAllText(@$"{path_root}\train.txt", train.Trim());
                        //File.WriteAllText(@$"{path_root}\val.txt", val.Trim());
                    }
                    #endregion
                }
                else if (type_数据集类型 == 数据集类型.图像分割)
                {
                    back.ImSave(@$"{path_images}\{name}.jpg");//图像文件
                    black.ImSave(@$"{path_masks}\{name}.png");//标签文件
                }
                else if (type_数据集类型 == 数据集类型.超分辨率重构)
                {
                    var sigma = 7;//方差
                    back.ImSave(@$"{path_labels}\{name}.png");
                    Mat aaa = back.GaussianBlur(new Size(2 * sigma + 1, 2 * sigma + 1), 7, 7);
                    aaa.ImSave(@$"{path_images}\{name}.png");
                }

                //6、显示
                var dis = back.Clone();
                CV.ImShow("dis", dis);
                Cv2.WaitKey(1);
            }
            Cv2.DestroyAllWindows();

            #region 随机验证
            if (type_数据集类型 == 数据集类型.目标检测)
            {
                var list_images = new DirectoryInfo(path_images).GetFiles().Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".bmp").ToList();
                var list_labels = new DirectoryInfo(path_labels).GetFiles().Where(f => f.Extension == ".txt").ToList();

                for (int i = 0; i < 5; i++)
                {
                    var rand_index = new Random().Next(0, list_images.Count);
                    var img = new Mat(list_images[rand_index].FullName, ImreadModes.Color);
                    var label = File.ReadAllText(list_labels[rand_index].FullName);

                    var xx = Base.yolostr2doublearray(label);
                    for (int j = 0; j < xx.Count; j++)
                    {
                        var (_, x0, y0, w, h) = xx[j];
                        (x0, y0, w, h) = (x0 * img.Width, y0 * img.Height, w * img.Width, h * img.Height);

                        var pt1 = new Point(x0 - w / 2d, y0 - h / 2d);
                        var pt2 = new Point(x0 + w / 2d, y0 + h / 2d);
                        img.Rectangle(pt1, pt2, Scalar.Red, 3);
                    }

                    CV.ImShow("dis", img);
                    Cv2.WaitKey(500);
                }
            }
            else if (type_数据集类型 == 数据集类型.图像分割)
            {
                var list_images = new DirectoryInfo(path_images).GetFiles().Where(f => f.Extension == ".png" || f.Extension == ".jpg" || f.Extension == ".bmp").Select(f=>f.FullName).ToList();
                var list_masks = Directory.GetFiles(path_masks);

                for (int i = 0; i < 5; i++)
                {
                    var rand_index = new Random().Next(0, list_images.Count);
                    var img = new Mat(list_images[rand_index], ImreadModes.Color);
                    var mask = new Mat(list_masks[rand_index], ImreadModes.Color);
                    mask.SetTo(Scalar.Red, mask);
                    //Cv2.MinMaxLoc(mask, out double min, out double max);

                    img = img * 0.5 + mask * 0.5;
                    CV.ImShow("dis", img);
                    Cv2.WaitKey(500);
                }
            }
            #endregion

            Cv2.DestroyAllWindows();
        }

        Mat getmask(Mat img)
        {
            var mask = img.Threshold(0, 255, ThresholdTypes.Otsu);
            Cv2.BitwiseNot(mask, mask);

            var kernel = new Mat(7, 7, MatType.CV_8UC1, new byte[,]
            {
                { 0,0,1,1,1,0,0 },
                { 0,1,1,1,1,1,0 },
                { 1,1,1,1,1,1,1 },
                { 1,1,1,1,1,1,1 },
                { 1,1,1,1,1,1,1 },
                { 0,1,1,1,1,1,0 },
                { 0,0,1,1,1,0,0 }
            });

            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);//闭运算
            Cv2.MorphologyEx(mask, mask, MorphTypes.Erode, kernel);//腐蚀
            Cv2.MorphologyEx(mask, mask, MorphTypes.Dilate, kernel);
            return mask;
        }
        Mat getmask1(Mat img)
        {
            var mask = img.Threshold(0, 255, ThresholdTypes.Otsu);

            Cv2.BitwiseNot(mask, mask);

            var kernel = new Mat(7, 7, MatType.CV_8UC1, new byte[,]
            {
                { 0,0,1,1,1,0,0 },
                { 0,1,1,1,1,1,0 },
                { 1,1,1,1,1,1,1 },
                { 1,1,1,1,1,1,1 },
                { 1,1,1,1,1,1,1 },
                { 0,1,1,1,1,1,0 },
                { 0,0,1,1,1,0,0 }
            });

            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);//闭运算

            Cv2.MorphologyEx(mask, mask, MorphTypes.Dilate, kernel);//腐蚀
            Cv2.MorphologyEx(mask, mask, MorphTypes.Erode, kernel);//膨胀

            return mask;
        }
    }
}
