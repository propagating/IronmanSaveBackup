using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IronmanSaveBackup
{
    class ValidationRules
    {
        public class StringToIntValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultrueInfo)
            {
                int i;
                if (int.TryParse(value.ToString(), out i))
                {
                    return new ValidationResult(true, null);
                }

                return new ValidationResult(false, "Please enter a valid integer value.");
            }
        }

        public class StringToBoolValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                bool b;
                if (bool.TryParse(value.ToString(), out b))
                {
                    return new ValidationResult(true, null);
                }
                return new ValidationResult(false, $"Please enter either 'True' or 'False' (without quotations).");
            }
        }
    }
}
