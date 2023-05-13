using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static work.math.MathExp;
using static System.Math;

namespace AIDatasetsPro.core
{
    class SelectRect
    {
        #region 属性
        private (Point, Point) _rect1 = default((Point, Point));
        public (Point, Point) Rect1
        {
            get
            {
                Mat pts = H1 * H2 * wh2pts(size);
                pts.Row(0).MinMaxIdx(out double minval_x, out double maxval_x);
                pts.Row(1).MinMaxIdx(out double minval_y, out double maxval_y);
                var p1 = new Point(minval_x, minval_y);
                var p2 = new Point(maxval_x, maxval_y);
                return (p1, p2);
            }
        }

        private (Point, Size, double) _rect2 = default((Point, Size, double));
        public (Point, Size, double) Rect2
        {
            get
            {
                return default((Point, Size, double));
            }
        }

        private Mat _src = new Mat();
        public Mat Src
        {
            get => _src;
            set
            {
                _src = value;
                update?.Invoke(value);
            }
        }

        /// <summary>
        /// 绑定事件可显示绘图动画
        /// </summary>
        public event Action<Mat> update;
        #endregion

        #region 函数
        void Init()
        {
            size = default(Size);
            update?.Invoke(Src);//触发事件
        }
        public void MouseDown(Point pt)
        {
            if (size != default(Size) && is_mouse_on_rect(pt) != Position.None)
            {
                //选择框调整
                pt1 = pt;
                position = is_mouse_on_rect(pt);//获取鼠标在选择框上的位姿，在中心或这是线上
                action = Action.ModifyRect;
                theta = Math.Atan2(H2.Get<double>(1, 0), H2.Get<double>(0, 0));
            }
            else
            {
                //绘制新的选择框
                pt1 = pt;
                action = Action.DrawRect;
            }
        }
        public void MouseMove(Point pt)
        {
            if (action == Action.DrawRect)
            {
                pt2 = pt;

                //计算中点坐标
                var x0 = (pt2.X + pt1.X) / 2.0;
                var y0 = (pt2.Y + pt1.Y) / 2.0;

                H1 = SE2(x0, y0, 0);
                H2 = SE2(0, 0, 0);

                double w = Abs(pt2.X - pt1.X);
                double h = Abs(pt2.Y - pt1.Y);
                size = new Size(w, h);

                var pts = H1 * H2 * wh2pts(size);
                drawing(pts, size);
                
            }
            else if (action == Action.ModifyRect)
            {
                switch (position)
                {
                    #region 整体平移 只调整H1
                    case Position.center:
                        {
                            H1 = SE2(pt.X, pt.Y, 0);
                            var pts = H1 * H2 * wh2pts(size);
                            drawing(pts, size);
                        }
                        break;
                    #endregion

                    #region 角点旋转 只调整H2
                    case Position.pt1:
                    case Position.pt2:
                    case Position.pt3:
                    case Position.pt4:
                        {
                            var center = cal_center(H1 * H2 * wh2pts(size));
                            var t1 = Atan2(pt1.Y - center.Y, pt1.X - center.X);
                            var t2 = Atan2(pt.Y - center.Y, pt.X - center.X);
                            var tt = t2 - t1;
                            H2 = SE2(0, 0, theta + tt);
                            var pts = H1 * H2 * wh2pts(size);
                            drawing(pts, size);
                        }
                        break;
                    #endregion

                    #region 调整宽高 只调整wh，
                    case Position.line1_center:
                    case Position.line3_center:
                    case Position.line1:
                    case Position.line3:
                        {
                            //将鼠标的坐标转换到选择框坐标系下
                            Mat p1 = (H1 * H2).Inv() * new Mat(3, 1, MatType.CV_64FC1, new double[] { pt.X, pt.Y, 1d });
                            var y = p1.Get<double>(1);
                            var scale_h = Math.Abs(2 * y) / size.Height;
                            size = new Size(size.Width, scale_h * size.Height);
                            var pts = H1 * H2 * wh2pts(size);
                            drawing(pts, size);
                        }
                        break;
                    case Position.line2_center:
                    case Position.line4_center:
                    case Position.line2:
                    case Position.line4:
                        {
                            //将鼠标的坐标转换到选择框坐标系下
                            Mat p1 = (H1 * H2).Inv() * new Mat(3, 1, MatType.CV_64FC1, new double[] { pt.X, pt.Y, 1d });
                            var x = p1.Get<double>(0);
                            var scale_w = Math.Abs(2 * x) / size.Width;
                            size = new Size(scale_w * size.Width, size.Height);
                            var pts = H1 * H2 * wh2pts(size);
                            drawing(pts, size);
                        }
                        break;
                        #endregion
                }
            }
        }
        public void MouseUp(Point pt)
        {
            //if (action == Action.DrawRect)
            //{
            //    pt2 = pt;
            //    mouseState = MouseState.Up;

            //    //init_rect_pts_org(pt1, pt2);

            //    //drawing(H1 * H2 * rect_pts_org);

            //}
        }
        public void Clear()
        {
            Init();
        }
        private void drawing(Mat pts, Size size)
        {
            var pt1 = new Point(pts.Get<double>(0, 0), pts.Get<double>(1, 0));
            var pt2 = new Point(pts.Get<double>(0, 1), pts.Get<double>(1, 1));
            var pt3 = new Point(pts.Get<double>(0, 2), pts.Get<double>(1, 2));
            var pt4 = new Point(pts.Get<double>(0, 3), pts.Get<double>(1, 3));

            var dis = Src.Clone();
            dis.Polylines(new[] { new[] { pt1, pt2, pt3, pt4 } }, true, Scalar.Red, line_width);

            var center = cal_center(pts);

            var f = 2.0;
            dis.Circle(center, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(pt1, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(pt2, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(pt3, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(pt4, (int)(f * line_width), Scalar.Red, -1);

            var c1 = cal_center(pt1, pt2);
            var c2 = cal_center(pt2, pt3);
            var c3 = cal_center(pt3, pt4);
            var c4 = cal_center(pt4, pt1);

            dis.Circle(c1, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(c2, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(c3, (int)(f * line_width), Scalar.Red, -1);
            dis.Circle(c4, (int)(f * line_width), Scalar.Red, -1);

            dis.PutText($"{size.Width},{size.Height}", new Point(0, 30), HersheyFonts.Italic, 1, Scalar.Red, 1);
            update?.Invoke(dis);//触发事件
        }

        private Position is_mouse_on_rect(Point p)
        {
            Mat pts = H1 * H2 * wh2pts(size);

            var pt1 = new Point(pts.Get<double>(0, 0), pts.Get<double>(1, 0));
            var pt2 = new Point(pts.Get<double>(0, 1), pts.Get<double>(1, 1));
            var pt3 = new Point(pts.Get<double>(0, 2), pts.Get<double>(1, 2));
            var pt4 = new Point(pts.Get<double>(0, 3), pts.Get<double>(1, 3));

            var center = cal_center(pts);

            if (cal_pt2pt(p, center) <= 3 * line_width) return Position.center;

            if (cal_pt2pt(p, pt1) <= 3 * line_width) return Position.pt1;
            if (cal_pt2pt(p, pt2) <= 3 * line_width) return Position.pt2;
            if (cal_pt2pt(p, pt3) <= 3 * line_width) return Position.pt3;
            if (cal_pt2pt(p, pt4) <= 3 * line_width) return Position.pt4;

            if (cal_pt2pt(p, cal_center(pt1, pt2)) <= 2 * line_width) return Position.line1_center;
            if (cal_pt2pt(p, cal_center(pt2, pt3)) <= 2 * line_width) return Position.line2_center;
            if (cal_pt2pt(p, cal_center(pt3, pt4)) <= 2 * line_width) return Position.line3_center;
            if (cal_pt2pt(p, cal_center(pt4, pt1)) <= 2 * line_width) return Position.line4_center;

            if (cal_pt2line_distance(pt1, pt2, p) <= line_width) return Position.line1;
            if (cal_pt2line_distance(pt2, pt3, p) <= line_width) return Position.line2;
            if (cal_pt2line_distance(pt3, pt4, p) <= line_width) return Position.line3;
            if (cal_pt2line_distance(pt4, pt1, p) <= line_width) return Position.line4;

            return Position.None;
        }
        private double cal_pt2line_distance(Point pt1, Point pt2, Point p)
        {
            var X1 = pt1.X;
            var Y1 = pt1.Y;
            var X2 = pt2.X;
            var Y2 = pt2.Y;

            var x0 = p.X;
            var y0 = p.Y;

            //此两点式转一般是没有除法，可避免了除数为0的问题。 https://blog.csdn.net/madbunny/article/details/43955883
            var A = Y2 - Y1;
            var B = X1 - X2;
            var C = X2 * Y1 - X1 * Y2;

            var line = new Mat(3, 1, MatType.CV_64FC1, new[] { A, B, C });
            var pt = new Mat(3, 1, MatType.CV_64FC1, new[] { x0, y0, 1d });

            Mat a = line.T() * pt;
            var v = a.Norm();

            //点到直线的距离公式
            var d = Math.Abs(A * x0 + B * y0 + C) / Math.Sqrt(A * A + B * B);

            return d;
        }
        private double cal_pt2pt(Point pt1, Point pt2)
        {
            double x1 = pt1.X;
            double y1 = pt1.Y;
            double x2 = pt2.X;
            double y2 = pt2.Y;

            var d = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            return d;
        }
        private Point cal_center(Point pt1, Point pt2)
        {
            var pt = new Point((pt1.X + pt2.X) / 2.0, (pt1.Y + pt2.Y) / 2.0);
            return pt;
        }
        private Point cal_center(Mat pts)
        {
            var x0 = pts.Row(0).Sum().Val0 / 4.0;
            var y0 = pts.Row(1).Sum().Val0 / 4.0;
            return new Point(x0, y0);
        }
        private Mat wh2pts(Size size)
        {
            double w = size.Width;
            double h = size.Height;

            var pts = new Mat(3, 4, MatType.CV_64FC1, new double[,]
            {
                    { -w/2,w/2,w/2,-w/2 },
                    { h/2,h/2,-h/2,-h/2 },
                    { 1,1,1,1 }
            });

            return pts;
        }
        #endregion

        #region 属性
        enum Action
        {
            DrawRect,
            ModifyRect
        }
        enum MouseState
        {
            Down = 0,
            Up
        }
        enum Position
        {
            None,
            pt1,
            pt2,
            pt3,
            pt4,
            line1_center,
            line2_center,
            line3_center,
            line4_center,
            line1,
            line2,
            line3,
            line4,
            center
        }
        Action action;
        int line_width = 2;
        Position position;
        Point pt1, pt2;
        double theta;
        Mat H1 = new Mat();
        Mat H2 = new Mat();
        Size size = new Size();
        //Mat Src;
        #endregion
        public SelectRect()
        {
            Init();
        }
    }
}
