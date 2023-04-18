using System;
using System.Collections.Generic;

namespace XFrame.ValueObjects.XmlValueObjects
{
    public interface IXmlValueObjectRepository : IDisposable
    {
        T Find<T>(string code) where T : XmlValueObject;

        XmlValueObject Find(Type type, string code);

        T[] FindAll<T>() where T : XmlValueObject;

        IEnumerable<T> FindAll<T>(string codes) where T : XmlValueObject;

        XmlValueObject[] FindAll(Type type);
    }
}
