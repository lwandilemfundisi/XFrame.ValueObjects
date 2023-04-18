using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using XFrame.Common.Extensions;

namespace XFrame.ValueObjects.XmlValueObjects
{
    public class XmlValueObjectRepository : IXmlValueObjectRepository
    {
        private bool disposed;
        private const string ValueObjectPathFormat = @"{0}.ValueObjects.Mapping.{1}.xml";
        private static object loadValueObjectsLock = new object();
        private readonly IDictionary<Type, IList<XmlValueObject>> valueObjectCache = new Dictionary<Type, IList<XmlValueObject>>();
        private static Type IntType = typeof(int);
        private static Type NullableIntType = typeof(int?);
        private static Type BooleanType = typeof(bool);
        private static Type NullableBooleanType = typeof(bool?);
        private static Type DecimalType = typeof(decimal);
        private static Type NullableDecimalType = typeof(decimal?);
        private static Type TypeType = typeof(Type);
        private static Type ValueObjectCultureType = typeof(List<XmlValueObjectCulture>);
        private static Type ValueObjectResourcePathAttributeType = typeof(ValueObjectResourcePathAttribute);
        private const string ValueObjectCultureInfo = "ValueObjectCultureInfo";

        #region Constructors

        #endregion

        #region IValueObjectRepository Members

        /// <summary>
        /// Find all the value objects for <see cref="Type"/>
        /// </summary>
        public XmlValueObject[] FindAll(Type type)
        {
            return LoadValueObjectsFromCache(type).ToArray();
        }

        /// <summary>
        /// Find all the value objects for <see cref="T"/>
        /// </summary>
        public T[] FindAll<T>() where T : XmlValueObject
        {
            var valueObjects = FindAll(typeof(T));
            return valueObjects.Cast<T>().ToArray();
        }

        /// <summary>
        /// Finds a value object instance based on <paramref name="type"/>
        /// and the <paramref name="code"/>
        /// </summary>
        public XmlValueObject Find(Type type, string code)
        {
            if (code.IsNullOrEmpty())
            {
                return null;
            }
            return LoadValueObjectsFromCache(type).FirstOrDefault(v => string.Compare(v.Code, code, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Find a value object based on the code.
        /// </summary>
        public T Find<T>(string code) where T : XmlValueObject
        {
            return (T)Find(typeof(T), code);
        }

        public IEnumerable<T> FindAll<T>(string codes) where T : XmlValueObject
        {
            if (codes.IsNotNullOrEmpty())
            {
                var splitCodes = codes.Split(',').Where(c => c.IsNotNullOrEmpty());

                if (splitCodes.HasItems())
                {
                    foreach (var code in splitCodes)
                    {
                        yield return Find<T>(code);
                    }
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads value objects from cache
        /// </summary>
        private IList<XmlValueObject> LoadValueObjectsFromCache(Type valueObjectType)
        {
            if (disposed)
            {
                throw new XmlValueObjectException(XmlValueObjectMessages.XmlValueObjectError, new ObjectDisposedException(XmlValueObjectMessages.XmlValueObjectRepositoryDisposed));
            }

            if (valueObjectCache.Keys.Contains(valueObjectType))
            {
                return valueObjectCache[valueObjectType];
            }
            else
            {
                lock (loadValueObjectsLock)
                {
                    if (valueObjectCache.Keys.Contains(valueObjectType))
                    {
                        return valueObjectCache[valueObjectType];
                    }
                    else
                    {
                        var valueObjects = LoadFromXml(valueObjectType);
                        valueObjectCache.Add(valueObjectType, valueObjects);
                        return valueObjects;
                    }
                }
            }
        }

        /// <summary>
        /// Loads value objects definitions contained in xml resource files
        /// </summary>
        private static IList<XmlValueObject> LoadFromXml(Type valueObjectType)
        {
            var newValueObjectList = new SortedList<string, XmlValueObject>();

            var resourceName = ValueObjectPathFormat.FormatInvariantCulture(valueObjectType.Namespace, valueObjectType.Name);

            var resourceStream = valueObjectType.Assembly.GetManifestResourceStream(resourceName);

            if (resourceStream.IsNull())
            {
                var pathAttribute = valueObjectType.GetCustomAttributes(ValueObjectResourcePathAttributeType, false).FirstOrDefault() as ValueObjectResourcePathAttribute;

                if (pathAttribute.IsNotNull())
                {
                    resourceStream = valueObjectType.Assembly.GetManifestResourceStream(pathAttribute.Path);
                }
            }

            if (resourceStream == null)
            {
                throw new XmlValueObjectException(XmlValueObjectMessages.XmlValueObjectResourceNotFound.FormatInvariantCulture(resourceName, valueObjectType.Name));
            }

            //load data from xml. This logic assumes the filename of the xml file is the same
            //as that of the full class
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(resourceStream);

            foreach (XmlNode valueObjectNode in xmlDocument.LastChild.ChildNodes)
            {
                var valueObject = (XmlValueObject)Activator.CreateInstance(valueObjectType);

                foreach (XmlNode propertyNode in valueObjectNode.ChildNodes)
                {
                    var propertyInfo = valueObjectType.GetProperty(propertyNode.Name);

                    if (propertyInfo == null)
                    {
                        throw new XmlValueObjectException(XmlValueObjectMessages.XmlValueObjectError, new MissingMemberException(string.Format(CultureInfo.InvariantCulture, XmlValueObjectMessages.XmlValueObjectXmlMappingError, propertyNode.Name, valueObjectType.FullName)));
                    }

                    else if (propertyNode.Name == ValueObjectCultureInfo)
                    {
                        SetValue(valueObject, propertyInfo, propertyNode.InnerXml);
                    }
                    else
                    {
                        SetValue(valueObject, propertyInfo, propertyNode.InnerText);
                    }
                }

                newValueObjectList.Add(valueObject.GenerateKey(), valueObject);
            }

            return newValueObjectList.Values.OrderBy(v => v.Text).ToList();
        }

        private static void SetValue(XmlValueObject target, PropertyInfo info, string value)
        {
            if (info.PropertyType == BooleanType
             || info.PropertyType == NullableBooleanType)
            {
                info.SetValue(target, value.AsBool().GetValueOrDefault(), null);
            }
            else if (info.PropertyType == IntType
                  || info.PropertyType == NullableIntType)
            {
                info.SetValue(target, value.AsInt(), null);
            }
            else if (info.PropertyType == DecimalType
              || info.PropertyType == NullableDecimalType)
            {
                info.SetValue(target, value.AsDecimal(), null);
            }
            else if (info.PropertyType == TypeType)
            {
                info.SetValue(target, Type.GetType(value), null);
            }
            else if (info.PropertyType == ValueObjectCultureType)
            {
                var serializer = new XmlSerializer(typeof(XmlValueObjectCulture));

                using (StringReader reader = new StringReader(value))
                {
                    XmlValueObjectCulture resultingMessage = (XmlValueObjectCulture)serializer.Deserialize(reader);
                    target.XmlValueObjectCultureInfo.Add(resultingMessage);
                }
            }
            else
            {
                info.SetValue(target, value, null);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                valueObjectCache.Clear();
                disposed = true;
            }
        }

        #endregion
    }
}
