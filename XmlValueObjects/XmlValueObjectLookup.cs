using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XFrame.Common.Extensions;

namespace XFrame.ValueObjects.XmlValueObjects
{
    public class XmlValueObjectLookup<T, A> : XmlValueObjectLookup where T : XmlValueObject where A : XmlValueObjectLookup
    {
        private IList<T> allowedItems;
        private bool allowAllItems = true;

        #region Methods

        [DebuggerStepThrough]
        public static A Of()
        {
            return Activator.CreateInstance<A>();
        }

        protected T FindValueObject(string code)
        {
            var valueObject = Repository.Find<T>(code);
            if (valueObject.IsNotNull())
            {
                return (T)valueObject.Clone();
            }
            return null;
        }

        public void LoadAllowedItems(IEnumerable<T> allowedItems)
        {
            this.allowedItems = allowedItems.ToList();
        }

        #endregion

        #region Properties

        public bool AllowAllItems
        {
            get
            {
                return allowAllItems;
            }
            set
            {
                allowAllItems = value;
            }
        }

        public IEnumerable<T> AllowedItems
        {
            get
            {
                if (allowedItems.IsNull())
                {
                    if (AllowAllItems)
                    {
                        allowedItems = new List<T>();
                        foreach (var item in Repository.FindAll<T>())
                        {
                            var valueObject = (T)item.Clone();
                            allowedItems.Add(valueObject);
                        }
                    }
                    else
                    {
                        allowedItems = new List<T>();
                    }
                }
                return allowedItems;
            }
            internal set { }
        }

        #endregion
    }

    public class XmlValueObjectLookup
    {
        #region Constructors

        public XmlValueObjectLookup()
        {
        }

        #endregion

        #region Properties

        public static IXmlValueObjectRepository Repository
        {
            get
            {
                return XmlValueObjectRepositoryManager.Instance;
            }
        }

        #endregion
    }
}
