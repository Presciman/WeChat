using Gma.QrCodeNet.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WeChatDemo
{
    static class PathRender
    {
        /// <summary>
        /// 以矩形来填充矩阵点
        /// </summary>
        /// <param name="QrMatrix">算出来的矩阵</param>
        /// <param name="xScale">isRandom？随机取值的最小值：每个点的宽度</param>
        /// <param name="yScale">isRandom？随机取值的最大值：每个点的高度</param>
        /// <param name="isRandom">是否随机大小</param>
        /// <returns>返回路径填充材质</returns>
        public static StreamGeometry DrawRectGeometry(BitMatrix QrMatrix, double xScale, double yScale, bool isRandom)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;

            StreamGeometry qrCodeStream = new StreamGeometry();
            qrCodeStream.FillRule = FillRule.EvenOdd;

            if (QrMatrix == null)
                return qrCodeStream;


            using (StreamGeometryContext qrCodeCtx = qrCodeStream.Open())
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (QrMatrix[x, y])
                        {
                            if (isRandom)
                                qrCodeCtx.DrawRectGeometry(x, y, (double)(new Random(x + y + Environment.TickCount).Next((int)(xScale * 100), (int)(yScale * 100))) / 100, (double)(new Random(x + y + Environment.TickCount).Next((int)(xScale * 100), (int)(yScale * 100))) / 100);
                            else
                                qrCodeCtx.DrawRectGeometry(x, y, xScale, yScale);
                        }
                    }
                }
            }

            qrCodeStream.Freeze();

            return qrCodeStream;
        }

        /// <summary>
        /// 以圆点来填充矩阵点
        /// </summary>
        /// <param name="QrMatrix">算出来的矩阵</param>
        /// <param name="xScale">isRandom？随机取值的最小值：每个点的宽度</param>
        /// <param name="yScale">isRandom？随机取值的最大值：每个点的高度</param>
        /// <param name="isRandom">是否随机大小</param>
        /// <returns>返回路径填充材质</returns>
        public static StreamGeometry DrawEllipseGeometry(BitMatrix QrMatrix, double xScale, double yScale, bool isRandom)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;

            StreamGeometry qrCodeStream = new StreamGeometry();
            qrCodeStream.FillRule = FillRule.EvenOdd;

            if (QrMatrix == null)
                return qrCodeStream;

            using (StreamGeometryContext qrCodeCtx = qrCodeStream.Open())
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (QrMatrix[x, y])
                        {
                            if (isRandom)
                                qrCodeCtx.DrawEllipseGeometry(x, y, (double)(new Random(x + y + Environment.TickCount).Next((int)(xScale * 100), (int)(yScale * 100))) / 100, (double)(new Random(x + y + Environment.TickCount).Next((int)(xScale * 100), (int)(yScale * 100))) / 100);
                            else
                                qrCodeCtx.DrawEllipseGeometry(x, y, xScale, yScale);
                        }
                    }
                }
            }


            qrCodeStream.Freeze();

            return qrCodeStream;
        }

        /// <summary>
        /// 以自定义图形来填充矩阵点
        /// </summary>
        /// <param name="QrMatrix">算出来的矩阵</param>
        /// <param name="xScale">isRandom？随机取值的最小值：每个点的宽度</param>
        /// <param name="yScale">isRandom？随机取值的最大值：每个点的高度</param>
        /// <param name="isRandomSize">是否随机大小</param>
        /// <returns>返回路径填充材质</returns>
        public static void DrawCustomGeometry(BitMatrix QrMatrix, ref Grid drawGrid, Path pathGeo, double xScale, double yScale, bool isRandomSize, bool isRandomColor)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;
            drawGrid.Width = drawGrid.Height = width * pathGeo.Width;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (QrMatrix[x, y])
                    {
                        Path newPath = new Path();//创建一个路径，代表一点
                        newPath.StrokeThickness = 0;
                        newPath.Stretch = Stretch.UniformToFill;//填充方式s
                        newPath.HorizontalAlignment = HorizontalAlignment.Left;
                        newPath.VerticalAlignment = VerticalAlignment.Top;
                        newPath.Data = pathGeo.Data;
                        newPath.RenderTransformOrigin = new Point(0.5, 0.5);
                        TranslateTransform newTTF = new TranslateTransform(x * pathGeo.Width, y * pathGeo.Height);
                        newPath.RenderTransform = newTTF;
                        if (isRandomSize)//如果随机大小
                        {
                            newPath.Width = newPath.Height = pathGeo.Width * (double)(new Random(x + y + Environment.TickCount).Next((int)(xScale * 100), (int)(yScale * 100))) / 100;
                        }
                        else
                        {
                            newPath.Width = pathGeo.Width * xScale;
                            newPath.Height = pathGeo.Height * yScale;
                        }
                        if (isRandomColor)//如果随机颜色
                            newPath.Fill = new SolidColorBrush(GetRandomColor());
                        else
                            newPath.Fill = Brushes.Black;


                        drawGrid.Children.Add(newPath);
                    }
                }
            }


        }

        internal static void DrawRectGeometry(this StreamGeometryContext ctx, double X, double Y, double Width, double Height)
        {
            ctx.BeginFigure(new Point(X, Y), true, true);
            ctx.LineTo(new Point(X, Y + Height), true, true);
            ctx.LineTo(new Point(X + Width, Y + Height), true, true);
            ctx.LineTo(new Point(X + Width, Y), true, true);
        }

        internal static void DrawEllipseGeometry(this StreamGeometryContext ctx, double X, double Y, double Width, double Height)
        {
            X = X * 2;
            Y = Y * 2;
            Height = Height * 2;
            Width = Width * 2;

            ctx.BeginFigure(new Point(X, Y + Height / 2), true, true);
            ctx.ArcTo(new Point(X + Width, Y + Height / 2), new Size(Width / 2, Height / 2), 90, false, SweepDirection.Clockwise, true, true);
            ctx.ArcTo(new Point(X, Y + Height / 2), new Size(Width / 2, Height / 2), 90, false, SweepDirection.Clockwise, true, true);

        }

        public static Color GetRandomColor()
        {
            Random randomNum_1 = new Random(Guid.NewGuid().GetHashCode());
            System.Threading.Thread.Sleep(randomNum_1.Next(1));
            int int_Red = randomNum_1.Next(255);

            Random randomNum_2 = new Random((int)DateTime.Now.Ticks);
            int int_Green = randomNum_2.Next(255);

            Random randomNum_3 = new Random(Guid.NewGuid().GetHashCode());

            int int_Blue = randomNum_3.Next(255);
            int_Blue = (int_Red + int_Green > 380) ? int_Red + int_Green - 380 : int_Blue;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;


            return GetDarkerColor(Color.FromArgb(Convert.ToByte(255), Convert.ToByte(int_Red), Convert.ToByte(int_Green), Convert.ToByte(int_Blue)));
        }

        //获取加深颜色
        public static Color GetDarkerColor(Color color)
        {
            const int max = 255;
            int increase = new Random(Guid.NewGuid().GetHashCode()).Next(30, 255); //还可以根据需要调整此处的值


            int r = Math.Abs(Math.Min(color.R - increase, max));
            int g = Math.Abs(Math.Min(color.G - increase, max));
            int b = Math.Abs(Math.Min(color.B - increase, max));


            return Color.FromArgb(Convert.ToByte(255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
    }
}
