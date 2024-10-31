using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using System.Windows.Controls;
using System.ComponentModel;

using Microsoft.Xna.Framework;

namespace SprueKit
{
    public static class UtilityExt
    {
        static string[,] SymmetricTerms = new string[,]
        {
            { "left", "right" },
            { "Left", "Right" },
            { "L.", "R." },
            { "l.", "r." }
        };
        public static string SymmetricVersion(this string str)
        {
            string ret = str;
            for (int i = 0; i < SymmetricTerms.GetLength(0); ++i)
            {
                if (ret.Contains(SymmetricTerms[i, 0]))
                    ret = ret.Replace(SymmetricTerms[i, 0], SymmetricTerms[i, 1]);
                if (ret.Contains(SymmetricTerms[i, 1]))
                    ret = ret.Replace(SymmetricTerms[i, 1], SymmetricTerms[i, 0]);
            }
            return ret;
        }

        public static string Localize(this string str)
        {
            return str;
        }

        public static T Random<T>(this List<T> src, Random r = null)
        {
            if (src.Count == 0)
                return default(T);
            if (r == null)
                r = new Random();
            return src[r.Next(0, src.Count)];
        }

        public static T Random<T>(this T[] src, Random r = null)
        {
            if (src.Length == 0)
                return default(T);
            if (r == null)
                r = new Random();
            return src[r.Next(0, src.Length)];
        }

        public static object BasicClone(this object src)
        {
            object ret = Activator.CreateInstance(src.GetType());
            ret.MatchTo(src);
            return ret;
        }

        /// <summary>
        /// Simple property matching between objects
        /// </summary>
        public static void MatchTo(this object dest, object src)
        {
            if (dest.GetType() != src.GetType())
                return;

            var props = dest.GetType().GetProperties();
            foreach (var pi in props)
            {
                if (pi.GetType() == typeof(PropertyChangedEventHandler))
                    continue;
                if (pi.CanWrite)
                {
                    // handle field cloning, we have a few special fields that aren't trivial
                    if (pi.PropertyType == typeof(SprueKit.Data.ResponseCurve))
                        pi.SetValue(dest, ((SprueKit.Data.ResponseCurve)pi.GetValue(src)).Clone());
                    else if (pi.PropertyType == typeof(SprueKit.Data.ColorCurves))
                        pi.SetValue(dest, ((SprueKit.Data.ColorCurves)pi.GetValue(src)).Clone());
                    else if (pi.PropertyType == typeof(SprueKit.Data.ColorRamp))
                        pi.SetValue(dest, ((SprueKit.Data.ColorRamp)pi.GetValue(src)).Clone());
                    else
                    pi.SetValue(dest, pi.GetValue(src));
                }
            }
        }

        /// <summary>
        /// Convert a list of URIs into a list of strings.
        /// </summary>
        /// <param name="uris">List of uris to transform</param>
        /// <returns></returns>
        public static List<string> ToStringList(this List<Uri> uris)
        {
            List<string> ret = new List<string>();
            foreach (var uri in uris)
                ret.Add(uri.ToString());
            return ret;
        }

        /// <summary>
        /// Convert a set of strings into Uris
        /// </summary>
        /// <param name="strs">list of strings to convert</param>
        /// <returns></returns>
        public static List<Uri> ToUriList(this List<string> strs)
        {
            List<Uri> ret = new List<Uri>();
            foreach (var str in strs)
                ret.Add(new Uri(str));
            return ret;
        }

        /// <summary>
        /// Convert a list of URIs into a list of strings.
        /// </summary>
        /// <param name="uris">List of uris to transform</param>
        /// <returns></returns>
        public static List<string> ToStringList(this ObservableCollection<Uri> uris)
        {
            List<string> ret = new List<string>();
            foreach (var uri in uris)
                ret.Add(uri.ToString());
            return ret;
        }

        /// <summary>
        /// Convert a set of strings into Uris
        /// </summary>
        /// <param name="strs">list of strings to convert</param>
        /// <returns></returns>
        public static void FillUriList(this List<string> strs, ObservableCollection<Uri> ret)
        {
            foreach (var str in strs)
                ret.Add(new Uri(str));
        }

        public static void ClearTreeSelection(this TreeView view)
        {
            foreach (var item in view.Items)
                ClearTreeSelection(view.ItemContainerGenerator, item);
        }

        public static void ClearTreeSelection(ItemContainerGenerator generator, object item)
        {
            var cont = generator.ContainerFromItem(item) as TreeViewItem;
            if (cont != null)
            {
                cont.IsSelected = false;
                if (cont.HasItems)
                {
                    foreach (var subItem in cont.Items)
                        ClearTreeSelection(cont.ItemContainerGenerator, subItem);
                }
            }
        }

        public static void SetSelected(this TreeView view, object target)
        {
            foreach (var item in view.Items)
                SetSelected(view.ItemContainerGenerator, item, target);
        }

        static void SetSelected(ItemContainerGenerator generator, object item, object target)
        {
            var cont = generator.ContainerFromItem(item) as TreeViewItem;
            if (cont != null)
            {
                cont.IsSelected = item == target;
                if (cont.HasItems)
                {
                    foreach (var subItem in cont.Items)
                        ClearTreeSelection(cont.ItemContainerGenerator, subItem);
                }
            }
        }

        public static void MatchSelection(this TreeView view, SelectionContext context)
        {
            foreach (var item in view.Items)
                MatchSelection(view.ItemContainerGenerator, item, context);
        }

        static void ZebraStripe(this ItemContainerGenerator gtor, object item, ref int idx)
        {
            var cont = gtor.ContainerFromItem(item) as TreeViewItem;
            if (cont != null)
            {
                if (idx % 2 == 0)
                    (cont as TreeViewItem).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(25,25,25));
                else
                    (cont as TreeViewItem).Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                ++idx;
                if (cont.HasItems && cont.IsExpanded)
                {
                    foreach (var subItem in cont.Items)
                        cont.ItemContainerGenerator.ZebraStripe(subItem, ref idx);
                }
            }
        }

        public static void ZebraStripe(this TreeView view)
        {
            int idx = 0;
            foreach (var item in view.Items)
                view.ItemContainerGenerator.ZebraStripe(item, ref idx);
        }

        static void MatchSelection(ItemContainerGenerator generator, object item, SelectionContext context)
        {
            var cont = generator.ContainerFromItem(item) as TreeViewItem;
            if (cont != null)
            {
                cont.IsSelected = context.Selected.Contains(item);
                if (cont.HasItems)
                {
                    foreach (var subItem in cont.Items)
                        MatchSelection(cont.ItemContainerGenerator, subItem, context);
                }
            }
        }

        public delegate void TreeItemDel(TreeViewItem itemView, object itemData);

        public static void ForEachItem(this TreeView view, TreeItemDel del)
        {
            foreach (var item in view.Items)
                ForEachItem(view.ItemContainerGenerator, item, del);
        }

        static void ForEachItem(ItemContainerGenerator generator, object item, TreeItemDel del)
        {
            var cont = generator.ContainerFromItem(item) as TreeViewItem;
            if (cont != null)
            {
                del(cont, item);
                if (cont.HasItems)
                {
                    foreach (var subItem in cont.Items)
                        ForEachItem(cont.ItemContainerGenerator, subItem, del);
                }
            }
        }

        public static System.Drawing.Bitmap GenerateBitmap(this Data.ColorRamp ramp, int w, int h)
        {
            var bmp = new System.Drawing.Bitmap(Math.Max(w, 1), Math.Max(h, 1));
            for (int x = 0; x < w; ++x)
            {
                float frac = ((float)x) / ((float)w);
                var color = ramp.Get(frac).ToDrawingColor();
                for (int y = 0; y < h; ++y)
                    bmp.SetPixel(x, y, color);
            }
            return bmp;
        }

        public static System.Drawing.Bitmap GenerateBitmap(this Data.ColorCurves ramp, int w, int h)
        {
            var bmp = new System.Drawing.Bitmap(Math.Max(w, 1), Math.Max(h, 1));
            for (int x = 0; x < w; ++x)
            {
                float frac = ((float)x) / ((float)w);
                int r = (int)(ramp.R.GetValue(frac) * 255);
                int g = (int)(ramp.R.GetValue(frac) * 255);
                int b = (int)(ramp.R.GetValue(frac) * 255);
                int a = (int)(ramp.R.GetValue(frac) * 255);
                var color = System.Drawing.Color.FromArgb(a, r, g, b);
                for (int y = 0; y < h; ++y)
                    bmp.SetPixel(x, y, color);
            }
            return bmp;
        }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265358979323846264338327950288f;
        public const float HALF_PI = PI * 0.5f;

        public const float EPSILON = 0.000001f;
        public const float LARGE_EPSILON = 0.00005f;
        public const float MIN_NEARCLIP = 0.01f;
        public const float MAX_FOV = 160.0f;
        public const float LARGE_VALUE = 100000000.0f;
        public const float DEGTORAD = PI / 180.0f;
        public const float DEGTORAD_2 = PI / 360.0f;    // M_DEGTORAD / 2.f
        public const float RADTODEG = 1.0f / DEGTORAD;

        public static float Pow(float val, float exp)
        {
            return (float)Math.Pow(val, exp);
        }

        public static float Abs(float val)
        {
            return (float)Math.Abs(val);
        }

        public static float Log(float val)
        {
            return (float)Math.Log(val);
        }

        public static float Sqrt(float val)
        {
            return (float)Math.Sqrt(val);
        }

        public static float Sin(float val)
        {
            return (float)Math.Sin(val);
        }

        public static float Cos(float val)
        {
            return (float)Math.Cos(val);
        }

        public static float Tan(float val)
        {
            return (float)Math.Tan(val);
        }

        public static float Asin(float val)
        {
            return (float)Math.Asin(val);
        }

        public static float Acos(float val)
        {
            return (float)Math.Acos(val);
        }

        public static float Atan(float d)
        {
            return (float)Math.Atan(d);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Exp(float v)
        {
            return (float)Math.Exp(v);
        }

        public static float Wrap(float val, float min, float max)
        {
            while (val > max)
                val -= max;
            while (val < min)
                val += max;
            return val;
        }

        public static int Wrap(int val, int min, int max)
        {
            while (val > max)
                val -= max;
            while (val < min)
                val += max;
            return val;
        }

        public static float Clamp01(float val)
        {
            return Math.Max(0.0f, Math.Min(val, 1.0f));
        }

        public static int Clamp(int val, int min, int max)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        public static bool Between(float val, float min, float max)
        {
            return val >= min && val <= max;
        }

        public static float Clamp(float val, float min, float max)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static float Hypot(float a, float b)
        {
            return Mathf.Sqrt(a * a + b * b);
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t));
        }

        public static float SmoothMin(float a, float b, float k = 0.4f)
        {
            float h = Clamp(0.5f + 0.5f * (b - a) / k, 0.0f, 1.0f);
            return Lerp(b, a, h) - k * h * (1.0f - h);
        }

        public static float Normalize(float val, float min, float max)
        {
            return (val - min) / (max - min);
        }

        public static double Normalize(double val, double min, double max)
        {
            return (val - min) / (max - min);
        }

        public static float Denormalize(float val, float min, float max)
        {
            return val * (max - min) + min;
        }

        public static float ConvertSpace(float val, float srcMin, float srcMax, float destMin, float destMax)
        {
            return ((val - srcMin) / (srcMax - srcMin)) * (destMax - destMin) + destMin;
        }

        public static float ConvertSpaceZeroRelative(float val, float srcMax, float destMax)
        {
            return (val / srcMax) * destMax;
        }
    }

    /// <summary>
    /// Extension methods for the string data type
    /// </summary>
    public static class ConventionBasedFormattingExtensions
    {
        public static string SplitCamelCase(this string input)
        {
            StringBuilder ret = new StringBuilder();

            bool lastWasLower = false;
            for (int i = 0; i < input.Length; ++i)
            {
                if (char.IsUpper(input[i]) || char.IsDigit(input[i]))
                {
                    if (lastWasLower)
                    {
                        ret.Append(' ');
                        ret.Append(input[i]);
                    }
                    else if (i > 0 && i < input.Length - 1 && char.IsLower(input[i + 1]))
                    {
                        ret.Append(' ');
                        ret.Append(input[i]);
                    }
                    else
                        ret.Append(input[i]);
                    lastWasLower = false;
                }
                else if (char.IsSymbol(input[i]))
                    ret.Append(input[i]);
                else
                {
                    ret.Append(input[i]);
                    lastWasLower = true;
                }
            }
            return ret.ToString();
            //return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }

    public static class GeneralUtility
    {
        public static System.Boolean FileInUse(System.String file)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                if (!System.IO.File.Exists(file))
                    return false;
                using (System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }

        public static Commands.IQuickActionSource FindActionSource(DependencyObject depObj)
        {
            if (depObj == null)
                return null;
            if (depObj is Commands.IQuickActionSource)
                return depObj as Commands.IQuickActionSource;

            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(depObj);

            //we've reached the end of the tree
            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            Commands.IQuickActionSource parent = parentObject as Commands.IQuickActionSource;
            if (parent != null)
                return parent;
            else
                return FindActionSource(parentObject);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem FindItemsControlObject<childItem>(this ItemsControl ctrl, object item) where childItem : DependencyObject
        {
            ContentPresenter cp = ctrl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
            if (cp == null)
                return null;
            childItem ret = FindVisualChild<childItem>(cp);
            return ret;
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
                return child;

            return null;
        }

        public static void Navigate(System.Windows.FrameworkElement aRelTo, string aURI)
        {
            var frame = GetFrame(aRelTo);
            if (frame != null)
                frame.Source = new Uri(aURI, UriKind.Relative);
        }

        public static void Navigate(string aName, System.Windows.FrameworkElement aRelTo, string aURI)
        {
            var frame = GetFrame(aName, aRelTo);
            if (frame != null)
                frame.Source = new Uri(aURI, UriKind.Relative);
        }

        public static void Navigate(System.Windows.FrameworkElement aRelTo, Uri aURI)
        {
            var frame = GetFrame(aRelTo);
            if (frame != null)
                frame.Source = aURI;
        }

        public static void Navigate(string aName, System.Windows.FrameworkElement aRelTo, Uri aURI)
        {
            var frame = GetFrame(aName, aRelTo);
            if (frame != null)
                frame.Source = aURI;
        }

        public static void CreateNavigation(ModernWindow aWnd)
        {
            if (aWnd.MenuLinkGroups == null || aWnd.MenuLinkGroups.Count == 0)
            {
                aWnd.MenuLinkGroups = new FirstFloor.ModernUI.Presentation.LinkGroupCollection();
                LinkGroup grp = new LinkGroup();
                aWnd.MenuLinkGroups.Add(grp);
                grp.DisplayName = "Project";
                grp.Links.Add(new Link
                {
                    DisplayName = "Summary",
                    Source = new Uri("/Dashboard/ProjectDashboard.xaml", UriKind.Relative)
                });
                grp.Links.Add(new Link
                {
                    DisplayName = "Manage",
                    Source = new Uri("/Dashboard/ManageProject.xaml", UriKind.Relative)
                });
                grp = new LinkGroup();
                aWnd.MenuLinkGroups.Add(grp);
                grp.DisplayName = "data";
                grp.Links.Add(new Link
                {
                    DisplayName = "Design",
                    Source = new Uri("/DataPane/DataTablesPage.xaml", UriKind.Relative)
                });
                grp.Links.Add(new Link
                {
                    DisplayName = "Extend",
                    Source = new Uri("/Extend/ExtendMaster.xaml#0", UriKind.Relative)
                });
                grp = new LinkGroup();
                aWnd.MenuLinkGroups.Add(grp);
                grp.DisplayName = "Transmogrify";
                grp.Links.Add(new Link
                {
                    DisplayName = "Code",
                    Source = new Uri("/Transmog/Transmog.xaml#code", UriKind.Relative)
                });
                grp.Links.Add(new Link
                {
                    DisplayName = "Data",
                    Source = new Uri("/Transmog/Transmog.xaml#data", UriKind.Relative)
                });
                grp = new LinkGroup();
                aWnd.MenuLinkGroups.Add(grp);
                grp.DisplayName = "Help";
                grp.Links.Add(new Link
                {
                    DisplayName = "",
                    Source = new Uri("/Help/HelpPage.xaml#basic", UriKind.Relative)
                });
            }
        }

        public static void RemoveNavigation(ModernWindow aWnd)
        {
            if (aWnd.MenuLinkGroups.Count > 0)
                aWnd.MenuLinkGroups.Clear();
        }

        public static ModernFrame GetFrame(System.Windows.FrameworkElement aRelTo)
        {
            var frame = NavigationHelper.FindFrame(null, aRelTo);
            if (frame != null)
                return frame;
            return null;
        }

        public static ModernFrame GetFrame(string aName, System.Windows.FrameworkElement aRelTo)
        {
            var frame = NavigationHelper.FindFrame(aName, aRelTo);
            if (frame != null)
                return frame;
            return null;
        }

        public static string GetResourceTextFile(string filename, object ctx)
        {
            string result = string.Empty;

            using (Stream stream = ctx.GetType().Assembly.
                       GetManifestResourceStream(filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        public static Stream GetResourceFileStream(String nameSpace, String filePath)
        {
            String pseduoName = filePath.Replace('\\', '.');
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(nameSpace + "." + pseduoName);
        }

        public static string ToExtendedName(this Type t)
        {
            return string.Format("{0}, {1}", t.FullName, t.Assembly.GetName().Name);
        }

        static Dictionary<string, Type> typeTable_;
        public static Type ToType(this string str)
        {
            if (typeTable_ == null)
            {
                typeTable_ = new Dictionary<string, Type>();
                typeTable_["bool"] = typeof(Boolean);
                typeTable_["Boolean"] = typeof(Boolean);
                typeTable_["int"] = typeof(int);
                typeTable_["Int32"] = typeof(int);
                typeTable_["uint"] = typeof(uint);
                typeTable_["UInt32"] = typeof(uint);
                typeTable_["float"] = typeof(float);
                typeTable_["Single"] = typeof(float);
                typeTable_["double"] = typeof(double);
                typeTable_["Double"] = typeof(double);

                // XNA Types
                typeTable_["Vector2"] = typeof(Microsoft.Xna.Framework.Vector2);
                typeTable_["Vector3"] = typeof(Microsoft.Xna.Framework.Vector3);
                typeTable_["Vector4"] = typeof(Microsoft.Xna.Framework.Vector4);
                typeTable_["Quaternion"] = typeof(Microsoft.Xna.Framework.Quaternion);
                typeTable_["Color"] = typeof(Microsoft.Xna.Framework.Color);
                typeTable_["Matrix"] = typeof(Microsoft.Xna.Framework.Matrix);

                // Modeling types
                typeTable_["SprueModel"] = typeof(SprueKit.Data.SprueModel);
                typeTable_["SimplePiece"] = typeof(SprueKit.Data.SimplePiece);
                typeTable_["ChainPiece"] = typeof(SprueKit.Data.ChainPiece);
                typeTable_["ChainBone"] = typeof(SprueKit.Data.ChainPiece.ChainBone);
                typeTable_["MarkerPiece"] = typeof(SprueKit.Data.MarkerPiece);
                typeTable_["ModelPiece"] = typeof(SprueKit.Data.ModelPiece);

                // Texturing types
                typeTable_["BoxTextureComponent"] = typeof(SprueKit.Data.BoxTextureComponent);
                typeTable_["ColorCubeTextureComponent"] = typeof(SprueKit.Data.ColorCubeTextureComponent);
                typeTable_["CylinderTextureComponent"] = typeof(SprueKit.Data.CylinderTextureComponent);
                typeTable_["DecalStripTextureComponent"] = typeof(SprueKit.Data.DecalStripTextureComponent);
                typeTable_["DecalTextureComponent"] = typeof(SprueKit.Data.DecalTextureComponent);
                typeTable_["DomeTextureComponent"] = typeof(SprueKit.Data.DomeTextureComponent);
                typeTable_["GradientTextureComponent"] = typeof(SprueKit.Data.GradientTextureComponent);
                typeTable_["Material"] = typeof(SprueKit.Data.Material);

            }

            Type ret = null;
            if (typeTable_.TryGetValue(str, out ret))
                return ret;
            return null;
        }

        public static void Increment<T>(this Dictionary<T, int> dict, T value)
        {
            int val = 1;
            if (dict.TryGetValue(value, out val))
                val += 1;
            else
                val = 1;
            dict[value] = val;
        }

        public static byte SetFlag(this byte val, byte flags)
        {
            return (byte)(val | flags);
        }

        public static byte UnsetFlag(this byte val, byte flags)
        {
            return (byte)(val & ~flags);
        }

        public static bool HasFlag(this byte val, byte flags)
        {
            return (val & flags) != 0;
        }

        public static float ToFloat(this byte val) { return val / 255.0f; }
        public static byte ToByte(this float val) { return (byte)(val * 255.0f); }
    }

    public static class BitmapExt
    {
        public static System.Drawing.Bitmap BuildStrip(this List<System.Drawing.Bitmap> bitmaps, int dim)
        {
            var outBmp = new System.Drawing.Bitmap(dim * bitmaps.Count, dim);

            for (int i = 0; i < bitmaps.Count; ++i)
            {
                var subBmp = bitmaps[i];
                for (int y = 0; y < subBmp.Height; ++y)
                    for (int x = 0; x < subBmp.Width; ++x)
                        outBmp.SetPixel(x + i * dim, y, subBmp.GetPixel(x, y));
            }
            return outBmp;
        }

        static float Clamp(float val, float min, float max)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        static System.Drawing.Color Mul(System.Drawing.Color lhs, float val)
        {
            return System.Drawing.Color.FromArgb(
                (byte)(lhs.A * val),
                (byte)(lhs.R * val),
                (byte)(lhs.G * val), 
                (byte)(lhs.B * val));
        }

        static System.Drawing.Color Add(System.Drawing.Color lhs, System.Drawing.Color rhs)
        {
            return System.Drawing.Color.FromArgb(
                (byte)(lhs.A + rhs.A),
                (byte)(lhs.R + rhs.R), 
                (byte)(lhs.G + rhs.G), 
                (byte)(lhs.B + rhs.B));
        }

        public static float Get(this float[] bmp, int x, int y, int width, int height)
        {
            return bmp[y * width + x];
        }

        public static float GetBilinear(this float[] bmp, float x, float y, int width, int height)
        {
            x = (float)(x - Math.Floor(x));
            y = (float)(y - Math.Floor(y));
            x = Clamp(x * width - 0.5f, 0.0f, (float)(width - 1));
            y = Clamp(y * height - 0.5f, 0.0f, (float)(height - 1));

            int xI = (int)x;
            int yI = (int)y;

            float xF = (float)(x - Math.Floor(x));
            float yF = (float)(y - Math.Floor(y));

            int xA = Mathf.Wrap(xI, 0, width - 1);
            int yA = Mathf.Wrap(yI, 0, height - 1);
            int xB = Mathf.Wrap(xI + 1, 0, width - 1);
            int yB = Mathf.Wrap(yI, 0, height - 1);
            int xC = Mathf.Wrap(xI, 0, width - 1);
            int yC = Mathf.Wrap(yI + 1, 0, height - 1);
            int xD = Mathf.Wrap(xI + 1, 0, width - 1);
            int yD = Mathf.Wrap(yI + 1, 0, height - 1);
            float topValue =                 
                    (bmp.Get(xA, yA, width, height) * (1.0f - xF)) +
                    (bmp.Get(xB, yB, width, height) * xF);

            float bottomValue = 
                (bmp.Get(xC, yC, width, height) * (1.0f - xF)) +
                (bmp.Get(xD, yD, width, height) * xF);
            return topValue * (1.0f - yF) + bottomValue * yF;
        }

        public static System.Drawing.Color GetPixelBilinear(this System.Drawing.Bitmap bmp, float x, float y)
        {
            x = (float)(x - Math.Floor(x));
            y = (float)(y - Math.Floor(y));
            x = Clamp(x * bmp.Width - 0.5f, 0.0f, (float)(bmp.Width - 1));
            y = Clamp(y * bmp.Height - 0.5f, 0.0f, (float)(bmp.Height - 1));

            int xI = (int)x;
            int yI = (int)y;

            float xF = (float)(x - Math.Floor(x));
            float yF = (float)(y - Math.Floor(y));

            int xA = Mathf.Wrap(xI, 0, bmp.Width - 1);
            int yA = Mathf.Wrap(yI, 0, bmp.Height - 1);
            int xB = Mathf.Wrap(xI + 1, 0, bmp.Width - 1);
            int yB = Mathf.Wrap(yI, 0, bmp.Height - 1);
            int xC = Mathf.Wrap(xI, 0, bmp.Width - 1);
            int yC = Mathf.Wrap(yI + 1, 0, bmp.Height - 1);
            int xD = Mathf.Wrap(xI + 1, 0, bmp.Width - 1);
            int yD = Mathf.Wrap(yI + 1, 0, bmp.Height - 1);
            System.Drawing.Color topValue = Add(
                Mul(
                    bmp.GetPixel(xA, yA), (1.0f - xF)), 
                Mul(
                    bmp.GetPixel(xB, yB), xF));

            System.Drawing.Color bottomValue = Add(
                Mul(
                    bmp.GetPixel(xC, yC), (1.0f - xF)), 
                Mul(
                    bmp.GetPixel(xD, yD), xF));
            return Add(Mul(topValue, 1.0f - yF), Mul(bottomValue, yF));
        }

        public static System.Drawing.Color GetPixelBilinearX(this System.Drawing.Bitmap bmp, float x, int y)
        {
            x = (float)(x - Math.Floor(x));
            x = Clamp(x * bmp.Width - 0.5f, 0.0f, (float)(bmp.Width - 1));

            int xI = (int)x;
            float xF = (float)(x - Math.Floor(x));
            int xA = Mathf.Wrap(xI, 0, bmp.Width - 1);
            int xB = Mathf.Wrap(xI + 1, 0, bmp.Width - 1);
            int xC = Mathf.Wrap(xI, 0, bmp.Width - 1);
            int xD = Mathf.Wrap(xI + 1, 0, bmp.Width - 1);
            return Add(
                Mul(
                    bmp.GetPixel(xA, y), (1.0f - xF)),
                Mul(
                    bmp.GetPixel(xB, y), xF));
        }
    }

    public class PingPong
    {
        float min = 0.0f;
        float max = 1.0f;
        float value = 0.0f;

        float mod(float num, float div)
        {
            float ratio = num / div;
            return div * (ratio - (float)Math.Floor(ratio));
        }

        public PingPong(float a, float b) {
            min = (a < b ? a : b);
            max = (a > b ? a : b);
            value = min;
        }

        public float Range() { return max-min; }

        public float CycleLength() { return 2 * Range(); }

        public float Normalize(float val)
        {
            float state = mod(val - min, CycleLength());
            if (state > Range())
                state = CycleLength() - state;
            return state + min;
        }

        public float Add(float val)
        {
            value += val;
            return Normalize(value);
        }
    }
}