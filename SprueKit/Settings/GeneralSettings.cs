using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace SprueKit.Settings
{

    [Serializable]
    public class GeneralSettings
    {
        public static string[] CorePaths =
        {
            "StockAssets"
        };

        public GeneralSettings()
        {
        }

        [Description("These folders will appear in the resource browser panel")]
        public Data.UriList AssetFolders { get; set; } = new Data.UriList();

        [Description("For lower spec machines, limits worker threads to 2. Requires restart")]
        public bool CapWorkerThreads { get; set; } = false;

        [Description("Websocket port to use for IPC")]
        public int IPCPort { get; set; } = 7879;

        /// <summary>
        /// Check if asset folders contains this uri
        /// </summary>
        public Uri CheckUri_Read(Uri input)
        {
            if (!input.IsAbsoluteUri)
                return null;
            foreach (var assetFolder in AssetFolders.Paths)
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(assetFolder);
                if (dirInfo.Exists && dirInfo.Name.ToLowerInvariant().Equals(input.Scheme))
                    return new Uri(System.IO.Path.Combine(assetFolder, input.Authority));
            }
            return null;
        }

        public Uri CheckUri_Save(Uri input)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(input.AbsolutePath);
            if (!fileInfo.Exists)
                return null;

            foreach (var assetFolder in AssetFolders.Paths)
            {
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(assetFolder);
                if (dirInfo.Exists && dirInfo.Name.ToLowerInvariant().Equals(input.Scheme) && fileInfo.Directory.Equals(dirInfo))
                {
                    return new Uri(string.Format("{0}://{1}", dirInfo.Name, fileInfo.Name));
                }
            }

            return null;
        }
    }

    [Serializable]
    public class ViewportSettings
    {
        [Description("Use flightstick style mouse control")]
        public bool InvertYAxis { get; set; } = false;

        [Description("Moves the camera forward along the viewing direction")]
        public System.Windows.Input.Key Forward { get; set; } = System.Windows.Input.Key.W;
        [Description("Moves the camera backward along the viewing direction")]
        public System.Windows.Input.Key Backward { get; set; } = System.Windows.Input.Key.S;
        [Description("Pans the camera to th eleft")]
        public System.Windows.Input.Key PanLeft { get; set; } = System.Windows.Input.Key.A;
        [Description("Pans the camera to the right")]
        public System.Windows.Input.Key PanRight{ get; set; } = System.Windows.Input.Key.D;
        [Description("Raises the camera vertically, maintains viewing direction")]
        public System.Windows.Input.Key PanUp { get; set; } = System.Windows.Input.Key.E;
        [Description("Lowers the camera vertically, maintains viewing direction")]
        public System.Windows.Input.Key PanDown { get; set; } = System.Windows.Input.Key.C;

        [Description("Resets the view to a neutral 3/4 axis")]
        public System.Windows.Input.Key ResetView { get; set; } = System.Windows.Input.Key.Home;
        [Description("Camera will pivot to look at the selected object or scene center")]
        public System.Windows.Input.Key LookAtSelection{ get; set; } = System.Windows.Input.Key.End;

        [Description("Sets the speed of camera movement when SHIFT is NOT pressed")]
        public float BaseMovementSpeed { get; set; } = 1.0f;
        [Description("Speed of camera movement when SHIFT key is held")]
        public float FastMovementSpeed { get; set; } = 3.0f;

        [Description("Position transforms will snap to this grid unit")]
        public float PositionSnap { get; set; } = 1.0f;
        [Description("Whether position transforms are to be snapped or not")]
        public bool PositionSnapActive { get; set; } = false;
        [Description("Rotation transforms will snap increments of this angle")]
        public float RotationSnap { get; set; } = 45.0f;
        [Description("Whether rotation transforms are to be snapped or not")]
        public bool RotationSnapActive { get; set; } = false;
    }
}
