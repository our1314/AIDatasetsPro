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
    internal class test_XRay卷盘_XML转DOTA : ConsoleTestBase
    {
        public override void RunTest()
        {
            var dir = @"\\192.168.11.12\自动化\刘林\temp\1";
            var files_xml = new DirectoryInfo(dir).GetFiles();
            files_xml = files_xml.Where(f => f.Extension == ".xml").ToArray();
            double k = 1024d / 1536d;

            var files_images = new DirectoryInfo(dir).GetFiles();
            files_images = files_images.Where(f => f.Extension == ".jpg" || f.Extension == ".png" || f.Extension == ".bmp").ToArray();

            foreach (var f in files_xml)
            {
                XmlDocument doc = new XmlDocument();
                var xml = File.ReadAllText(f.FullName);
                doc.LoadXml(xml);
                string json = JsonConvert.SerializeXmlNode(doc);
                dynamic aa = JsonConvert.DeserializeObject<dynamic>(json);

                double width = aa["annotation"]["size"]["width"];
                double height = aa["annotation"]["size"]["height"];
                var yololabel = "";
                var dotalabel = "";

                var xmlName = Path.GetFileNameWithoutExtension(f.FullName);
                var imgfile = files_images.First(f => Path.GetFileNameWithoutExtension(f.FullName) == xmlName);
                var src = new Mat(imgfile.FullName, ImreadModes.Unchanged);
                src = src.Resize(new Size(), k, k);
                src = src.CopyMakeBorder(0, 1024 - src.Height, 0, 0, BorderTypes.Constant);
                var mask = src.Threshold(240, 255, ThresholdTypes.Binary);
                src = src.SetTo(0, mask);

                var dis = src.Clone();
                dis = dis.Channels() == 1 ? dis.CvtColor(ColorConversionCodes.GRAY2BGR) : dis;

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

                    cx *= k;
                    cy *= k;
                    w *= k;
                    h *= k;

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

                    //if (w < h)
                    //{
                    //    var tmp = w;
                    //    w = h;
                    //    h = tmp;
                    //}
                    //yololabel += $"{cls} {cx / width} {cy / height} {w / width} {h / height} {angle / Math.PI * 180d}\r\n";

                    dotalabel += $"{dota_pts[0].X} {dota_pts[0].Y} {dota_pts[1].X} {dota_pts[1].Y} {dota_pts[2].X} {dota_pts[2].Y} {dota_pts[3].X} {dota_pts[3].Y} {cls} {0}\r\n";
                }

                //src.ImWrite(f.FullName.Replace(".xml", ".png"));

                Cv2.ImShow("dis", dis);
                Cv2.WaitKey(1);

                var path_images = Path.Join(dir, $@"out\images");
                var path_labels = Path.Join(dir, $@"out\labels");
                Directory.CreateDirectory(path_images);
                Directory.CreateDirectory(path_labels);

                File.WriteAllText(Path.Join(path_labels, f.Name.Replace(".xml", ".txt")), dotalabel);
                src.ImSave(Path.Join(path_images, f.Name.Replace(".xml", ".png")));
            }
        }
    }
}
