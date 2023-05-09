using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_xray毛刺_maskcopy : ConsoleTestBase
    {
        public override void RunTest()
        {
            var src_files = Directory.GetFiles("D:\\work\\files\\deeplearn_datasets\\test_datasets\\xray_毛刺_roi", "*.png", SearchOption.TopDirectoryOnly).ToList();
            src_files = src_files.Where(f => Path.GetFileName(f).StartsWith("back")).ToList();

            var img_files = Directory.GetFiles("D:\\desktop\\XRay毛刺检测\\TO252样品图片\\TO252编带好品\\ROI\\删除多余像素\\train", "*.jpg", SearchOption.TopDirectoryOnly);

            foreach(var f_img in img_files)
            {
                var s = src_files.Where(p => Path.GetFileNameWithoutExtension(f_img).EndsWith(Path.GetFileNameWithoutExtension(p))).ToList();
                var f_src = s.First();
                var src = new Mat(f_src, ImreadModes.Grayscale);
                var img = new Mat(f_img, ImreadModes.Grayscale);
                var mask = getmask(src);
                //Cv2.CopyTo(src, img, mask);
                img.SetTo(Scalar.Black,mask);

                Cv2.ImShow("img", img);
                Cv2.WaitKey();
            }
        }

        Mat getmask(Mat img)
        {
            var mask = img.Threshold(0, 255, ThresholdTypes.Otsu);

            //Cv2.ImShow("m1", mask);
            //Cv2.WaitKey();

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
            //Cv2.ImShow("m2", mask);
            //Cv2.WaitKey();

            Cv2.MorphologyEx(mask, mask, MorphTypes.Dilate, kernel);//
            //Cv2.ImShow("m3", mask);
            //Cv2.WaitKey();

            return mask;
        }
    }
}
