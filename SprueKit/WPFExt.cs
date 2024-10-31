using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SprueKit
{
    public static class StringUtils
    {
        public static string Extract(this string str, char open, char close)
        {
            int startidx = str.IndexOf(open);
            int endidx = str.IndexOf(close);
            if (startidx < endidx)
                return str.Substring(startidx + 1, endidx - startidx - 1);
            return null;
        }

        public static string[] SubArray(this string[] data, int index, int length)
        {
            string[] result = new string[length];
            System.Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ToSpaceQuoted(this string str)
        {
            if (str.Contains(' '))
                return String.Format("\"{0}\"", str);
            return str;
        }
    }

    public static class ClipboardUtil
    {
        public static void SetText(string text)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(100);
            }
        }

        public static void SetDataObject(object o, bool copy)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetDataObject(o, copy);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    public static class WPFExt
    {
        static Dictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();

        // Dumps the cache as a 2-line pair of 'key' and value
        // A task can then be used to load these bastard images in the background
        public static string DumpImageCache()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in imageCache)
            {
                sb.AppendLine(kvp.Key);
                sb.AppendLine(kvp.Value.UriSource.ToString());
            }
            return sb.ToString();
        }

        static char[] linesplits = new char[] { '\n', '\r' };
        public static void PrefetchIcons()
        {
            Task.Run(() =>
            {
                string embeddedText = WPFExt.GetEmbeddedFile("SprueKit.IconManifest.txt");
                string[] lines = embeddedText.Split(linesplits, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i += 2)
                    WPFExt.GetEmbeddedImage(lines[i + 1], true, lines[i]);
            });
        }

        /// <summary>
        /// Retrieves an embedded image resource. Uses caching behind the scenes to avoid repetitive loads.
        /// </summary>
        /// <param name="resource">Path to the image, excluding pack->component portion</param>
        /// <returns>BitmapImage if found, null otherwise</returns>
        public static BitmapImage GetEmbeddedImage(string resource, bool noExcept = false, string aliasAs = null)
        {
            BitmapImage ret = null;
            if (imageCache.TryGetValue(resource, out ret))
            {
                if (aliasAs != null)
                    imageCache[aliasAs] = ret;
                return ret;
            }

            try
            {
                lock (imageCache)
                {
                    ret = new BitmapImage();
                    ret.BeginInit();
                    if (resource.StartsWith("pack://"))
                        ret.UriSource = new Uri(resource, UriKind.RelativeOrAbsolute);
                    else
                        ret.UriSource = new Uri(string.Format("pack://application:,,,/{1};component/{0}", resource, App.AppName), UriKind.RelativeOrAbsolute);
                    ret.EndInit();
                    ret.Freeze();
                    imageCache[resource] = ret;
                    if (aliasAs != null)
                        imageCache[aliasAs] = ret;
                }
            }
            catch (Exception ex)
            {
                if (!noExcept)
                    ErrorHandler.inst().PublishError(string.Format("Unable to load image: {0}", resource), 3);
                return null;
            }

            return ret;
        }

        public static BitmapSource GetBitmapImage(this System.Drawing.Bitmap img)
        {
            if (img == null)
                return null;
            var ret = img.ToBitmapSource();
            img.Dispose();
            return ret;
        }

        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        public static BitmapSource BitmapFromBase64(string base64)
        {
            byte[] data = Convert.FromBase64String(base64);
            using (var memStream = new System.IO.MemoryStream(data))
            {
                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memStream);
                return bmp.GetBitmapImage();
            }
        }

        public static string ToBase64(this System.Drawing.Bitmap img)
        {
            using (var memStream = new System.IO.MemoryStream())
            {
                img.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                string base64 = Convert.ToBase64String(memStream.ToArray());
                return base64;
            }
        }

        public static string GetEmbeddedFile(string file)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(file))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }                
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().PublishError(string.Format("Unable to load file: {0}", file), 3);
                return null;
            }
        }

        public static float VerticalMiddle(this Rect r)
        {
            return (float)(r.Top + (r.Height) * 0.5f);
        }

        public static float HorizontalMiddle(this Rect r)
        {
            return (float)(r.Left + (r.Width) * 0.5f);
        }

        public static void DrawTriangle(this DrawingContext context, Pen pen, Point where, float width, float height)
        {
            StreamGeometry geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                ctx.BeginFigure(new Point(where.X - width, where.Y), true, true);
                ctx.PolyLineTo(new Point[] {
                    new Point(where.X + width, where.Y),
                    new Point(where.X, where.Y + height)
                }, true, true);
            }
            context.DrawGeometry(pen.Brush, pen, geo);
        }

        public static Point Normalize(this Rect r, Point pt)
        {
            return new Point
            {
                X = (pt.X - r.X) / (r.Width),
                Y = (pt.Y - r.Y) / (r.Height),
            };
        }

        public static Point Denormalize(this Rect r, Point pt)
        {
            return new Point
            {
                X = (pt.X * (r.Width) + r.X),
                Y = (pt.Y * (r.Height) + r.Y)
            };
        }

        public static float NormalizeWidth(this Rect r, float x)
        {
            return (float)((x - r.X) / (r.Width));
        }

        public static float DenormalizeWidth(this Rect r, float x)
        {
            return (float)(x * (r.Width) + r.X);
        }

        public static void Write(this BinaryWriter writer, Uri uri)
        {
            if (uri == null)
                writer.Write("");
            else
                writer.Write(uri.ToString());
        }

        public static Uri ReadUri(this BinaryReader reader)
        {
            string uri = reader.ReadString();
            if (!string.IsNullOrEmpty(uri))
                return new Uri(uri);
            return null;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }

    public static class CanvasExt
    {
        public static Point GetTopLeft(UIElement elem)
        {
            return new Point(Canvas.GetLeft(elem), Canvas.GetTop(elem));
        }

        public static Matrix GetScalingMatrix(this Canvas canvas)
        {
            MatrixTransform scaleTrans = canvas.LayoutTransform as MatrixTransform;
            if (scaleTrans != null)
                return scaleTrans.Matrix;
            return new Matrix(1, 0, 1, 0, 0, 0);
        }

        public static float Distance(this System.Windows.Point a, System.Windows.Point b)
        {
            var vec = System.Windows.Point.Subtract(b, a);
            return (float)vec.Length;
        }
    }
}
