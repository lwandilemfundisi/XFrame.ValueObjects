using System;
using XFrame.Common.Extensions;

namespace XFrame.ValueObjects.XmlValueObjects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ValueObjectResourcePathAttribute : Attribute
    {
        #region Constructors

        public ValueObjectResourcePathAttribute(string path)
        {
            if(path.IsNullOrEmpty()) throw new ArgumentNullException("path is required");
            Path = path;
        }

        #endregion

        #region Properties

        public string Path { get; private set; }

        #endregion
    }
}
