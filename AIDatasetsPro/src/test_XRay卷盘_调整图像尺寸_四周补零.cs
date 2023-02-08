using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using work.cv;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_XRay卷盘_调整图像尺寸_四周补零 : ConsoleTestBase
    {
        public override void RunTest()
        {
            var images = new DirectoryInfo(@"D:\桌面\xray-juanpan_已标注数据集\jsons").GetFiles();
            images = images.Where(f => f.Extension == ".jpg").ToArray();

            var files_xml = new DirectoryInfo(@"D:\桌面\xray-juanpan_已标注数据集\jsons").GetFiles();
            files_xml = files_xml.Where(f => f.Extension == ".xml").ToArray();

            //Mat P = Mat.Eye(3, 3, MatType.CV_64FC1);
            //P.Set<double>(0, 2, 200);
            //P.Set<double>(1, 2, 200);

            Mat dis = new Mat();
            foreach (var f in files_xml)
            {
                XmlDocument doc = new XmlDocument();
                var xml = File.ReadAllText(f.FullName);
                doc.LoadXml(xml);
                string json = JsonConvert.SerializeXmlNode(doc);
                dynamic aa = JsonConvert.DeserializeObject<dynamic>(json);

                double width = aa["annotation"]["size"]["width"];
                double height = aa["annotation"]["size"]["height"];
                aa["annotation"]["size"]["width"] = 1024;
                aa["annotation"]["size"]["height"] = 1024;
                var yololabel = "";
                var dotalabel = "";

                var xmlName = Path.GetFileNameWithoutExtension(f.FullName);
                var imgfile = images.First(f => f.FullName.Contains(xmlName));
                var src = new Mat(imgfile.FullName, ImreadModes.Unchanged);

                src = src.CopyMakeBorder(200, 200, 200, 200, BorderTypes.Constant, 0);
                var k = 1024f / Math.Max(src.Width, src.Height);
                src = src.Resize(new Size(), k, k);
                src = src.CopyMakeBorder(0, 1024 - src.Height, 0, 0, BorderTypes.Constant, 0);//填充至1024
                dis = src.CvtColor(ColorConversionCodes.GRAY2BGR);

                var dota_pts = new Point2d[4];
                foreach (var a in aa["annotation"]["object"])
                {
                    var cls = 0d;//a["name"] == "ng" ? 0d : 1d;
                    var bbox = a.robndbox;

                    double cx = bbox["cx"];
                    double cy = bbox["cy"];
                    double w = bbox["w"];
                    double h = bbox["h"];
                    double angle = bbox["angle"];

                    cx = (cx + 200) * k;
                    cy = (cy + 200) * k;
                    w = w * k;
                    h = h * k;

                    bbox["cx"] = cx;
                    bbox["cy"] = cy;
                    bbox["w"] = w;
                    bbox["h"] = h;


                    var pts = new Mat(2, 4, MatType.CV_64F, new double[,]
                    {
                        { -w/2d,w/2d,w/2d,-w/2d },
                        { h/2d,h/2d,-h/2d,-h/2d }
                    });

                    {
                        double x1 = cx - w / 2;
                        double y1 = cy - h / 2;
                        double x2 = x1 + w;
                        double y2 = y1 + h;

                        var theta = angle;
                        var rot = new Mat(2, 2, MatType.CV_64F, new double[,]
                        {
                            { Math.Cos(theta),-Math.Sin(theta) },
                            { Math.Sin(theta), Math.Cos(theta) }
                        });
                        Mat rot_pp = rot * pts;
                        Mat center = new Mat(2, 1, MatType.CV_64F, new double[,] { { cx }, { cy } }) * Mat.Ones(1, 4, MatType.CV_64FC1);
                        Mat rot_pts = rot * pts + center;

                        var pts_ = new Point[4];
                        for (int i = 0; i < dota_pts.Length; i++)
                        {
                            var xx = rot_pts.Get<double>(0, i);
                            var yy = rot_pts.Get<double>(1, i);
                            dota_pts[i] = new Point2d(xx, yy);
                            pts_[i] = new Point(xx, yy);
                        }

                        Cv2.Polylines(dis, new[] { pts_ }, true, Scalar.Red, 3);
                        dis.Circle(new Point(cx, cy), 10, Scalar.Red, -1);
                    }

                    if (w < h)
                    {
                        var tmp = w;
                        w = h;
                        h = tmp;
                    }
                    yololabel += $"{cls} {cx / width} {cy / height} {w / width} {h / height} {angle / Math.PI * 180d}\r\n";
                    dotalabel += $"{dota_pts[0].X} {dota_pts[0].Y} {dota_pts[1].X} {dota_pts[1].Y} {dota_pts[2].X} {dota_pts[2].Y} {dota_pts[3].X} {dota_pts[3].Y} {cls} {0}\r\n";
                }

                Cv2.ImShow("dis", dis);
                Cv2.WaitKey();

                src.ImSave(imgfile.FullName.Replace(".jpg", ".png"));
            }
        }
    }
}
