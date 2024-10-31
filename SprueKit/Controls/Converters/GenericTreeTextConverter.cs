using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SprueKit.Controls.Converters
{
    public class GenericTreeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GenericTreeObject)
            {
                object dataObject = ((GenericTreeObject)value).DataObject;

                if (dataObject is Data.ModelPiece)
                {
                    var mdl = dataObject as Data.ModelPiece;
                    return mdl.ModelFile.ModelFile.AbsolutePath;
                }
                else if (dataObject is Data.SprueModel)
                {
                    return "Voxel mesh";
                }
                return dataObject.ToString();
            }
            else
            {
                if (value is Data.ModelPiece)
                {
                    var mdl = value as Data.ModelPiece;
                    return mdl.ModelFile.ModelFile.AbsolutePath;
                }
                else if (value is Data.SprueModel)
                {
                    return "Voxel mesh";
                }
                else if (value is Data.MeshData)
                {
                    return "Mesh";
                }
                else if (value is SprueBindings.ModelData)
                {
                    return "Model";
                }
                else if (value is SprueBindings.MeshData)
                {
                    string txt = ((SprueBindings.MeshData)value).Name;
                    if (!string.IsNullOrWhiteSpace(txt))
                        return txt;
                    return "< unnamed mesh >";
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
