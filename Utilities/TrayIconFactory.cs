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
            using (var bitmap = CreateBatteryBitmap(mode, 32))
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
            using (var pen = new Pen(Color.FromArgb(70, 108, 184), 2f))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(70, 108, 184)))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawArc(pen, 4, 4, 10, 10, 35, 285);
                graphics.FillPolygon(arrowBrush, new[]
                {
                    new Point(13, 3),
                    new Point(16, 3),
                    new Point(15, 6)
                });
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
