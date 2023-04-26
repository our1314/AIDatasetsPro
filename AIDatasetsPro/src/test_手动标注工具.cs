using AIDatasetsPro.core;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.test;

namespace AIDatasetsPro.src
{
    internal class test_手动标注工具 : ConsoleTestBase
    {
        public override void RunTest()
        {
            Console.WriteLine("输入图像路径：");
            var filepath = Console.ReadLine();
            filepath = filepath.Replace("\"", "").Trim();//去掉引号

            var win = new Window("dis", WindowFlags.AutoSize);
            var src = new Mat(filepath);

            var select = new SelectRect();
            select.update += (img) =>
            {
                win.ShowImage(img);
                Cv2.WaitKey(1);
            };
            select.Src = src;

            win.SetMouseCallback((@event, x, y, flags, userData) =>
            {
                if (flags == MouseEventFlags.LButton)
                {
                    switch (@event)
                    {
                        case MouseEventTypes.LButtonDown:
                            {
                                select.MouseDown(new Point(x, y));
                            }
                            break;
                        case MouseEventTypes.MouseMove:
                            {
                                select.MouseMove(new Point(x, y));
                            }
                            break;
                        case MouseEventTypes.LButtonUp:
                            {
                                select.MouseUp(new Point(x, y));
                            }
                            break;
                    }
                }

                if (flags == MouseEventFlags.RButton)
                {
                    select.Clear();
                }
            });

            while (true)
            {
                var ch = (char)Cv2.WaitKey();
                if (ch == ' ')//空格键
                {
                    var (p1, p2) = select.Rect1;
                    var x = src[p1.Y, p2.Y, p1.X, p2.X];
                    Cv2.ImShow("x", x);

                    Console.WriteLine($"xyxy:{p1.X},{p1.Y},{p2.X},{p2.Y}");
                }
                if(ch==27)//esc
                {
                    return;
                }
            }
        }
    }
}
