using System;
using System.ComponentModel;
using System.Globalization;

namespace Versatile
{
    
    
        public class NuGetv2TypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var stringValue = value as string;
                NuGetv2 semVer;
                if (stringValue != null && NuGetv2.TryParse(stringValue, out semVer))
                {
                    return semVer;
                }
                return null;
            }
        }
    
}
