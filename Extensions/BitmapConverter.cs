using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ProjectReinforced.Recording;

namespace ProjectReinforced.Extensions
{
    public static class BitmapConverter
    {
        public static byte[] ToArray(this Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                bitmap.PixelFormat);
            byte[] byteData = new byte[bitmapData.Stride * bitmap.Height];

            Marshal.Copy(bitmapData.Scan0, byteData, 0, byteData.Length);
            bitmap.UnlockBits(bitmapData);

            return byteData;
        }

        public static Bitmap ToBitmap(this byte[] data, int width, int height, PixelFormat pixelFormat)
        {
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            BitmapData bmpData =
                bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public static OpenCvSharp.Size ToOpenCvSharpSize(this Size size)
        {
            return new OpenCvSharp.Size(size.Width, size.Height);
        }
    }
}
