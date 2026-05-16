using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using LenovoBatteryTray.Battery;

namespace LenovoBatteryTray.Utilities
{
    internal static class TrayIconFactory
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon CreateIcon(LenovoBatteryMode mode)
        {
            using (var bitmap = CreateTrayBadgeBitmap(mode, 32))
            {
                var handle = bitmap.GetHicon();

                try
                {
                    using (var icon = Icon.FromHandle(handle))
                    {
                        return (Icon)icon.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        public static Image CreateMenuImage(LenovoBatteryMode mode)
        {
            return CreateBatteryBitmap(mode, 18);
        }

        public static Image CreateRefreshImage()
        {
            var bitmap = CreateTransparentBitmap(18);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var backgroundBrush = new SolidBrush(Color.FromArgb(70, 108, 184)))
            using (var arrowPen = new Pen(Color.White, 1.9f))
            using (var arrowBrush = new SolidBrush(Color.White))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var background = CreateRoundedRectangle(new RectangleF(2, 2, 14, 14), 4))
                {
                    graphics.FillPath(backgroundBrush, background);
                }

                arrowPen.StartCap = LineCap.Round;
                arrowPen.EndCap = LineCap.Round;
                arrowPen.LineJoin = LineJoin.Round;

                graphics.DrawLine(arrowPen, 5, 7, 11, 7);
                graphics.FillPolygon(arrowBrush, new[]
                {
                    new Point(11, 4),
                    new Point(15, 7),
                    new Point(11, 10)
                });

                graphics.DrawLine(arrowPen, 13, 11, 7, 11);
                graphics.FillPolygon(arrowBrush, new[]
                {
                    new Point(7, 8),
                    new Point(3, 11),
                    new Point(7, 14)
                });
            }

            return bitmap;
        }

        public static Image CreateLanguageImage()
        {
            var bitmap = CreateTransparentBitmap(18);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var backgroundBrush = new SolidBrush(Color.FromArgb(96, 96, 104)))
            using (var letterBrush = new SolidBrush(Color.White))
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var background = CreateRoundedRectangle(new RectangleF(2, 2, 14, 14), 4))
                {
                    graphics.FillPath(backgroundBrush, background);
                }

                graphics.DrawString("A", font, letterBrush, new RectangleF(2, 2, 14, 14), format);
            }

            return bitmap;
        }

        public static Image CreateStartupImage()
        {
            var bitmap = CreateTransparentBitmap(18);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(Color.FromArgb(78, 142, 88)))
            using (var pen = new Pen(Color.FromArgb(42, 92, 52), 1.4f))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillEllipse(brush, 4, 3, 10, 12);
                graphics.DrawLine(pen, 9, 14, 9, 17);
                graphics.DrawLine(pen, 9, 11, 13, 7);
            }

            return bitmap;
        }

        public static Image CreateExitImage()
        {
            var bitmap = CreateTransparentBitmap(18);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var pen = new Pen(Color.FromArgb(180, 60, 58), 2.2f))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawLine(pen, 5, 5, 13, 13);
                graphics.DrawLine(pen, 13, 5, 5, 13);
            }

            return bitmap;
        }

        private static Bitmap CreateBatteryBitmap(LenovoBatteryMode mode, int size)
        {
            var bitmap = CreateTransparentBitmap(size);
            var scale = size / 32f;
            var fillColor = GetModeColor(mode);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var outlinePen = new Pen(Color.FromArgb(230, 38, 42, 48), Math.Max(1.3f, 2f * scale)))
            using (var terminalBrush = new SolidBrush(Color.FromArgb(230, 38, 42, 48)))
            using (var fillBrush = new SolidBrush(fillColor))
            using (var symbolPen = new Pen(Color.White, Math.Max(1.4f, 2.2f * scale)))
            using (var symbolBrush = new SolidBrush(Color.White))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var body = new RectangleF(4 * scale, 9 * scale, 21 * scale, 14 * scale);
                var terminal = new RectangleF(25 * scale, 13 * scale, 3 * scale, 6 * scale);
                var fill = new RectangleF(8 * scale, 12 * scale, 14 * scale, 8 * scale);

                using (var bodyPath = CreateRoundedRectangle(body, 4 * scale))
                using (var terminalPath = CreateRoundedRectangle(terminal, 1.8f * scale))
                using (var fillPath = CreateRoundedRectangle(fill, 2.5f * scale))
                {
                    graphics.FillPath(terminalBrush, terminalPath);
                    graphics.DrawPath(outlinePen, bodyPath);
                    graphics.FillPath(fillBrush, fillPath);
                }

                DrawModeSymbol(graphics, mode, scale, symbolPen, symbolBrush);
            }

            return bitmap;
        }

        private static Bitmap CreateTrayBadgeBitmap(LenovoBatteryMode mode, int size)
        {
            var bitmap = CreateTransparentBitmap(size);
            var scale = size / 32f;

            using (var graphics = Graphics.FromImage(bitmap))
            using (var backgroundBrush = new SolidBrush(GetModeColor(mode)))
            using (var outlinePen = new Pen(Color.FromArgb(210, 20, 24, 30), Math.Max(1f, 1.8f * scale)))
            using (var symbolPen = new Pen(Color.White, Math.Max(2.4f, 4.2f * scale)))
            using (var symbolBrush = new SolidBrush(Color.White))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var badge = new RectangleF(2.5f * scale, 2.5f * scale, 27f * scale, 27f * scale);

                using (var badgePath = CreateRoundedRectangle(badge, 7.5f * scale))
                {
                    graphics.FillPath(backgroundBrush, badgePath);
                    graphics.DrawPath(outlinePen, badgePath);
                }

                DrawBadgeSymbol(graphics, mode, scale, symbolPen, symbolBrush);
            }

            return bitmap;
        }

        private static void DrawBadgeSymbol(Graphics graphics, LenovoBatteryMode mode, float scale, Pen pen, Brush brush)
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;

            switch (mode)
            {
                case LenovoBatteryMode.Normal:
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(9 * scale, 16.5f * scale),
                        new PointF(14 * scale, 21 * scale),
                        new PointF(23 * scale, 11 * scale)
                    });
                    break;
                case LenovoBatteryMode.Storage:
                    using (var path = CreateRoundedRectangle(new RectangleF(10 * scale, 9 * scale, 4.8f * scale, 14 * scale), 1.8f * scale))
                    {
                        graphics.FillPath(brush, path);
                    }

                    using (var path = CreateRoundedRectangle(new RectangleF(17.2f * scale, 9 * scale, 4.8f * scale, 14 * scale), 1.8f * scale))
                    {
                        graphics.FillPath(brush, path);
                    }

                    break;
                case LenovoBatteryMode.Quick:
                    graphics.FillPolygon(brush, new[]
                    {
                        new PointF(17 * scale, 6.5f * scale),
                        new PointF(8.5f * scale, 18 * scale),
                        new PointF(14.5f * scale, 18 * scale),
                        new PointF(12.2f * scale, 26 * scale),
                        new PointF(24 * scale, 13.5f * scale),
                        new PointF(17.8f * scale, 13.5f * scale)
                    });
                    break;
                default:
                    using (var font = new Font("Segoe UI", 20 * scale, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        graphics.DrawString("?", font, brush, new RectangleF(0, 0, 32 * scale, 31 * scale), format);
                    }

                    break;
            }
        }

        private static void DrawModeSymbol(Graphics graphics, LenovoBatteryMode mode, float scale, Pen pen, Brush brush)
        {
            switch (mode)
            {
                case LenovoBatteryMode.Normal:
                    graphics.DrawLine(pen, 15 * scale, 13 * scale, 15 * scale, 19 * scale);
                    graphics.DrawLine(pen, 12 * scale, 16 * scale, 18 * scale, 16 * scale);
                    break;
                case LenovoBatteryMode.Storage:
                    graphics.FillRectangle(brush, 12 * scale, 13 * scale, 2.5f * scale, 7 * scale);
                    graphics.FillRectangle(brush, 16 * scale, 13 * scale, 2.5f * scale, 7 * scale);
                    break;
                case LenovoBatteryMode.Quick:
                    graphics.FillPolygon(brush, new[]
                    {
                        new PointF(15 * scale, 11 * scale),
                        new PointF(10 * scale, 17 * scale),
                        new PointF(14 * scale, 17 * scale),
                        new PointF(12 * scale, 22 * scale),
                        new PointF(20 * scale, 15 * scale),
                        new PointF(16 * scale, 15 * scale)
                    });
                    break;
                default:
                    using (var font = new Font("Segoe UI", 10 * scale, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        graphics.DrawString("?", font, brush, 12.2f * scale, 10.6f * scale);
                    }

                    break;
            }
        }

        private static Color GetModeColor(LenovoBatteryMode mode)
        {
            switch (mode)
            {
                case LenovoBatteryMode.Normal:
                    return Color.FromArgb(52, 152, 92);
                case LenovoBatteryMode.Storage:
                    return Color.FromArgb(48, 125, 181);
                case LenovoBatteryMode.Quick:
                    return Color.FromArgb(235, 151, 42);
                default:
                    return Color.FromArgb(132, 137, 145);
            }
        }

        private static Bitmap CreateTransparentBitmap(int size)
        {
            return new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        private static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2f;

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
