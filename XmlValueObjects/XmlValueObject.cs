using System;
using System.Collections.Generic;
using System.Globalization;
using XFrame.Common.Extensions;

namespace XFrame.ValueObjects.XmlValueObjects
{
    public abstract class XmlValueObject : ValueObject
    {
        #region Constructors

        public XmlValueObject()
        {
            XmlValueObjectCultureInfo = new List<XmlValueObjectCulture>();
        }

        #endregion

        #region Properties

        public string Code { get; set; }

        public string Text { get; set; }

        public List<XmlValueObjectCulture> XmlValueObjectCultureInfo { get; set; }

        public static IXmlValueObjectRepository Repository
        {
            get
            {
                return XmlValueObjectRepositoryManager.Instance;
            }
        }

        #endregion

        #region ValueObject vs. String

        public static bool operator ==(XmlValueObject left, string right)
        {
            return ValueObjectEqualsString(left, right);
        }

        public static bool operator !=(XmlValueObject left, string right)
        {
            return !ValueObjectEqualsString(left, right);
        }

        #endregion

        #region Methods

        public virtual bool IsAcceptableCode(string code)
        {
            return Code == code;
        }

        public XmlValueObject Clone()
        {
            var valueObject = OnCreateClone();
            valueObject.Code = Code;
            valueObject.Text = Text;
            valueObject.XmlValueObjectCultureInfo = XmlValueObjectCultureInfo;
            return OnClone(valueObject);
        }

        public static IEnumerable<T> AsEnumerable<T>() where T : XmlValueObject
        {
            return Repository.FindAll<T>();
        }

        #endregion

        #region Virtual Methods

        protected virtual XmlValueObject OnCreateClone()
        {
            return (XmlValueObject)Activator.CreateInstance(GetType(), null);
        }

        protected virtual XmlValueObject OnClone(XmlValueObject valueObject)
        {
            return valueObject;
        }

        public override string ToString()
        {
            return Text;
        }

        public virtual string GenerateKey()
        {
            return Code;
        }

        public override bool Equals(object obj)
        {
            var valueObject = obj as XmlValueObject;
            if (null != valueObject)
            {
                return ValueObjectsEqual(this, valueObject);
            }
            if (obj is string)
            {
                return ValueObjectEqualsString(this, Convert.ToString(obj, CultureInfo.InvariantCulture));
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (Code.IsNotNullOrEmpty())
            {
                return Code.GetHashCode();
            }
            return base.GetHashCode();
        }

        #endregion

        #region Private Methods

        private static bool ValueObjectEqualsString(XmlValueObject left, string right)
        {
            if (ReferenceEquals(left, null) && string.IsNullOrEmpty(right))
            {
                return true;
            }
            if (ReferenceEquals(left, null) || string.IsNullOrEmpty(right))
            {
                return false;
            }

            return left.Code.AsString() == right.AsString();
        }

        private static bool ValueObjectsEqual(XmlValueObject left, XmlValueObject right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Code.AsString() == right.Code.AsString();
        }

        #endregion
    }
}
