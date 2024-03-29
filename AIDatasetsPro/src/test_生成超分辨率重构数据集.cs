﻿using NumSharp;
using OpenCvSharp;
using our1314;
using TorchSharp;
using static our1314.work;

namespace AIDatasetsPro.src
{
    internal class test_生成超分辨率重构数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            double width = 300d;
            double step = 60d;

            for (; ; )
            {
                int pt_cnt = new Random().Next(3, 4);
                var x = np.arange(pt_cnt) * 60d; x = x.reshape(x.size, 1); //x[1, 0] = new Random().Next((int)step, (int)((pt_cnt - 1) * step));
                var y = torch.rand(pt_cnt, 1, torch.ScalarType.Float64) * width - width / 2;

                var mx = x.ToMat();
                var my = y.ToMat();
                var pts = new Mat();
                Cv2.HConcat(mx, my, pts);

                var X = Spline.QuadraticSpline(pts);
                Spline.GetQuadraticSplineCoord(pts, X, out double[] xx, out double[] yy);

                var x_min = xx.Min();
                var x_max = xx.Max();
                var y_min = yy.Min();
                var y_max = yy.Max();

                Figure figure2 = new Figure("二次样条曲线", 900, 700, 3);
                //figure2.Color = cur_max < 0.007 ? Scalar.Green : Scalar.Red;
                int key = figure2.Plot(xx, yy);

                if (key == (int)ConsoleKey.Escape)
                    return;
            }

            {
                #region 参数设置
                // 生成图像的总数
                var cnt_sum = 30;

                // 每张背景图的贴图数量
                var cnt_perimg = 1;

                //var colors = new[] { new Scalar(0, 0, 1), new Scalar(0, 128, 0), new Scalar(128, 0, 0),
                //                         new Scalar(0, 128, 128), new Scalar(128, 0, 128), new Scalar(128, 128, 0),
                //                         new Scalar(128, 128, 128)};

                var colors = new[] { 1 };

                //var train_val_test = new[] { 0.5, 0.2, 0.3 };
                #endregion


                Console.WriteLine("给定目标文件夹路径，其中包含前景图(forexxx.png，四通道图像（透明）)和背景图(格式任意)，可以多个");
                Console.WriteLine("输入包含前景图和背景图的文件夹路径：");

                var path = Console.ReadLine().Trim();

                // 创建相关目录
                var path_root = @$"{path}\out\test";
                var path_images = @$"{path_root}\images";
                var path_labels = @$"{path_root}\labels";
                var path_masks = @$"{path_root}\masks";

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
                    var black = back.EmptyClone().CvtColor(ColorConversionCodes.BGR2GRAY).SetTo(0);//与背景图同样尺寸的黑色图像

                    for (int j = 0; j < cnt_perimg; j++)
                    {
                        //1、随机获取一张前景图
                        var random_index = new Random().Next(files_fore.Length);
                        var fore = new Mat(files_fore[random_index].FullName, ImreadModes.Unchanged);

                        //2、将四通道的前景图拆分出bgr图像和mask图像
                        var fore_chls = fore.Split();
                        var bgr = new Mat();
                        Cv2.Merge(fore_chls.Take(3).ToArray(), bgr);
                        var mask = fore_chls.Last();

                        //3、生成随机坐标
                        var row = new Random().Next(0, back.Rows - fore.Rows);//不会生成最大值
                        var col = new Random().Next(0, back.Cols - fore.Cols);
                        var rect = new Rect(col, row, fore.Cols, fore.Rows);

                        //4、生成目标图像和同尺寸的mask图像
                        {
                            //bgr.CopyTo(back[rect], mask);//将前景图贴在背景图上
                            //black[rect].SetTo(colors[j], mask);//
                        }

                        {
                            back = back.CvtColor(ColorConversionCodes.BGR2GRAY);
                            black[rect].SetTo(colors[j], mask);//

                            back = back - 85 * black;
                            //Cv2.AddWeighted(back, 1, black*-85, 0.7, 0, back);
                            back = back.GaussianBlur(new Size(3, 3), 7);

                        }
                        //5、计算yolo标签
                        double x1 = col;
                        double y1 = row;
                        double w = fore.Cols;
                        double h = fore.Rows;

                        var cx = x1 + w / 2.0;//贴图中心坐标
                        var cy = y1 + h / 2.0;//贴图中心坐标

                        var label_yolo = $"0 {(cx / back.Width).ToString("F12")} {(cy / back.Height).ToString("F12")} {(w / back.Width).ToString("F12")} {(h / back.Height).ToString("F12")}";
                        gen_yolo_labels += label_yolo + "\r\n";
                    }

                    //5、保存
                    #region 保存前Resize至目标尺寸
                    {
                        var f = 256d / Math.Max(back.Width, back.Height);

                        back = back.Resize(new Size(), f, f);
                        //var maxlen = Math.Max(back.Width, back.Height);
                        //var padup = (maxlen - back.Height) / 2;
                        //var padleft = (maxlen - back.Width) / 2;
                        //back = back.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant);

                        black = black.Resize(new Size(), f, f, InterpolationFlags.Nearest);
                        //maxlen = Math.Max(black.Width, black.Height);
                        //padup = (maxlen - black.Height) / 2;
                        //padleft = (maxlen - black.Width) / 2;
                        //black = black.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant);
                    }

                    var gray = 0;
                    {
                        back = back.CvtColor(ColorConversionCodes.BGR2GRAY);


                        var k = 2;
                        var data = new Mat();
                        back.ConvertTo(data, MatType.CV_32F);
                        data = data.Reshape(0, data.Width * data.Height);
                        var label = new Mat();
                        Cv2.Kmeans(data, k, label, new TermCriteria(CriteriaTypes.MaxIter, 1000, 0.0001), attempts: 10, KMeansFlags.RandomCenters);

                        //2、将聚类结果转换为图像形状
                        label = label.Reshape(0, back.Rows);

                        var cls1 = new Mat(back.Size(), MatType.CV_16S, 0);
                        var cls2 = new Mat(back.Size(), MatType.CV_16S, 0);

                        Mat mask1 = label - 0;
                        mask1 = mask1.Abs();
                        mask1 = mask1.ConvertScaleAbs();

                        back.CopyTo(cls1, mask1);

                        Mat mask2 = label - 1;
                        mask2 = mask2.Abs();
                        mask2 = mask2.ConvertScaleAbs();

                        back.CopyTo(cls2, mask2);
                        cls1.FindNonZero().GetArray(out byte[] a1);
                        cls2.FindNonZero().GetArray(out byte[] a2);
                        var m1 = back.Mean(mask1);
                        var m2 = cls2.Mean(mask2);
                        Cv2.ImShow("cls1", cls1);
                        Cv2.WaitKey();

                        Cv2.ImShow("cls2", cls2);
                        Cv2.WaitKey();
                        gray = (int)Math.Max(m1.Val0, m2.Val0);

                    }

                    {
                        var padup = (256 - back.Height) / 2;
                        var padleft = (256 - back.Width) / 2;
                        back = back.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant, gray);

                        padup = (256 - black.Height) / 2;
                        padleft = (256 - black.Width) / 2;
                        black = black.CopyMakeBorder(padup, padup, padleft, padleft, BorderTypes.Constant, gray);
                    }
                    #endregion

                    //var name = work.work.Now;
                    //back.ImSave(@$"{path_images}\{name}.jpg");
                    //gen_yolo_labels.Trim().StrSave(@$"{path_labels}\{name}.txt");
                    //black.ImSave(@$"{path_masks}\{name}.png");

                    //6、显示
                    var dis = back.Clone();
                    //dis.PutText($"{black.Channels()}", new Point(0, 100), HersheyFonts.Italic, 2, Scalar.Red, 3);
                    work.ImShow("dis", dis);
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

                File.WriteAllText(@$"{path_root}\train.txt", train.Trim());
                File.WriteAllText(@$"{path_root}\val.txt", val.Trim());

                #region 随机验证
                {
                    var image_files = Directory.GetFiles(path_images);
                    var image_masks = Directory.GetFiles(path_masks);

                    for (int i = 0; i < 5; i++)
                    {
                        var rand_index = new Random().Next(0, image_files.Length);
                        var img = new Mat(image_files[rand_index], ImreadModes.Color);
                        var mask = new Mat(image_masks[rand_index], ImreadModes.Color);
                        mask.SetTo(OpenCvSharp.Scalar.Red, mask);
                        //Cv2.MinMaxLoc(mask, out double min, out double max);

                        img = img * 0.5 + mask * 0.5;
                        work.ImShow("dis", img);
                        Cv2.WaitKey(200);
                    }
                }
                #endregion

                Cv2.DestroyAllWindows();
            }
        }
    }
}
