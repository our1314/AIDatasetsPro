using OpenCvSharp;
using our1314;

namespace AIDatasetsPro.src
{
    internal class test_查看语义分割数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            #region 参数设置
            // 生成图像的总数
            var cnt_sum = 10;

            // 每张背景图的贴图数量
            var cnt_perimg = 1;

            //var colors = new[] { new Scalar(0, 0, 1), new Scalar(0, 128, 0), new Scalar(128, 0, 0),
            //                         new Scalar(0, 128, 128), new Scalar(128, 0, 128), new Scalar(128, 128, 0),
            //                         new Scalar(128, 128, 128)};

            var colors = new[] { 1 };
            #endregion


            Console.WriteLine("给定目标文件夹路径，其中包含前景图(forexxx.png，四通道图像（透明）)和背景图(格式任意)，可以多个");
            Console.WriteLine("输入包含前景图和背景图的文件夹路径：");

            var path_root = Console.ReadLine().Trim();

            // 创建相关目录
            //var path_root = @$"{path}\out\train1";
            var path_images = @$"{path_root}\images";
            var path_masks = @$"{path_root}\masks";



            #region 随机验证
            {
                var image_files = Directory.GetFiles(path_images);
                var image_masks = Directory.GetFiles(path_masks);

                for (int i = 0; i < image_files.Length; i++)
                {
                    var rand_index = i;//new Random().Next(0, image_files.Length);
                    var img = new Mat(image_files[rand_index], ImreadModes.Color);
                    var mask = new Mat(image_masks[rand_index], ImreadModes.Color);
                    mask.SetTo(Scalar.Red, mask);
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
