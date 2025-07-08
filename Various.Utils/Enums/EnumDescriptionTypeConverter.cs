using System;
using System.ComponentModel;
using System.Globalization;

namespace Various.Utils.Enums
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type)
        {

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    var fieldInfo = value.GetType().GetField(value.ToString());

                    if (fieldInfo != null)
                    {
                        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                        return (attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].Description)) ? attributes[0].Description : value.ToString();
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
