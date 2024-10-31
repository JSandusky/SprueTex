using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SprueKit.Data
{
    
    public class FloatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            float dummy = 0.0f;
            if (!float.TryParse(value.ToString(), out dummy))
                return new ValidationResult(false, "Invalid value");
            return new ValidationResult(true, null);
        }
    }

}
