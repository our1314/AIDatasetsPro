using OpenCvSharp;
using work;
using work.test;
using Scalar = OpenCvSharp.Scalar;

namespace AIDatasetsPro.src
{
    internal class test_生成XRay空洞检测数据集 : ConsoleTestBase
    {
        public override void RunTest()
        {
            //RandShape(3, 3, 3);
            Random rand = new Random();
            var files_background = Directory.GetFiles(@"D:\work\files\deeplearn_datasets\xray空洞检测\用于生成的数据");
            var files_forceground = Directory.GetFiles(@"D:\desktop\mask");

            for (int j = 0; j < 10; j++)
            {
                foreach (var f in files_background)
                {
                    var background = new Mat(f, ImreadModes.Color);

                    var image = background.Clone();
                    Mat label = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                    for (int i = 0; i < 15; i++)
                    {
                        #region 随机生成前景图像
                        var r = rand.Next(3, 20);
                        var width = 2 * r + 1;
                        var force = new Mat(width, width, MatType.CV_8UC1, Scalar.Black);
                        force.Circle(r - 1, r - 1, r, Scalar.White, -1);

                        //读取前景图
                        var idx = rand.Next(files_forceground.Length);
                        force = Cv2.ImRead(files_forceground[idx], ImreadModes.Unchanged);
                        var w = rand.Next(10, 10);
                        var h = (int)(w / (double)force.Width * force.Height);
                        force = force.Resize(new Size(w, h));
                        #endregion

                        var mask = force.Clone();
                        //force = force.CvtColor(ColorConversionCodes.GRAY2BGR);

                        var (x1, y1) = (rand.Next(0, label.Width - force.Width - 1), rand.Next(0, label.Height - force.Height - 1));
                        var roi = label[y1, y1 + force.Height, x1, x1 + force.Width];
                        //var tmp = new Mat();
                        //Cv2.AddWeighted(roi, 0.9, force, 0.1, 0, tmp);
                        mask.CopyTo(roi, mask);
                    }
                    var tmp = new Mat();
                    label = label.CvtColor(ColorConversionCodes.GRAY2BGR);
                    Cv2.AddWeighted(background, 0.88, label, 0.12, 0, tmp);
                    tmp.CopyTo(image, label);

                    Cv2.ImShow("dis", image);
                    Cv2.WaitKey(1);

                    var name = Utils.Now;
                    image.ImSave(@$"D:\work\files\deeplearn_datasets\xray空洞检测\空洞检测生成数据集\{name}.jpg");
                    label.ImSave(@$"D:\work\files\deeplearn_datasets\xray空洞检测\空洞检测生成数据集\{name}.png");
                }
            }
            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// 在极坐标系下随机生成多个角度和半径，再将坐标转换到笛卡尔坐标系
        /// </summary>
        public void RandShape(int agl_num, int pt_num, int agl_range)
        {
            var rand = new Random();
            var agl = from a in Enumerable.Range(0, 3) select rand.NextDouble() * 2 * Math.PI;
            agl = agl.ToArray();

            //var r = from a in Enumerable.Range(0, 3) select ;
        }
    }
}
