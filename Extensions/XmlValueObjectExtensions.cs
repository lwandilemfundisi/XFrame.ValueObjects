using XFrame.ValueObjects.XmlValueObjects;
using System.Linq;
using XFrame.Common.Extensions;

namespace XFrame.ValueObjects.Extensions
{
    public static class ValueObjectExtensions
    {
        public static bool IsIn(this XmlValueObject value, params XmlValueObject[] values)
        {
            if (value.IsNotNull())
            {
                foreach (ValueObject item in values)
                {
                    if (Equals(item, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string AsText(this XmlValueObject value)
        {
            if (value.IsNotNull())
            {
                return value.Text;
            }
            return string.Empty;
        }

        public static string AsCode(this XmlValueObject value)
        {
            return value.AsCode(string.Empty);
        }

        public static string AsCode(this XmlValueObject value, string defaultValue)
        {
            if (value.IsNotNull())
            {
                return value.Code;
            }
            return defaultValue;
        }

        public static string AsCultureSpecificText(this XmlValueObject value, string language)
        {
            if (value.IsNotNull() && value.XmlValueObjectCultureInfo.HasItems() && language.IsNotNullOrEmpty())
            {
                var culture = value.XmlValueObjectCultureInfo.FirstOrDefault(x => x.Language == language);

                if (culture.IsNotNull())
                {
                    return culture.Text;
                }
            }
            return value.AsText();
        }
    }
}
