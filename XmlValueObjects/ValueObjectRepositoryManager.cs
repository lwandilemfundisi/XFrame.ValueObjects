namespace XFrame.ValueObjects.XmlValueObjects
{
    public static class XmlValueObjectRepositoryManager
    {
        private static IXmlValueObjectRepository instance;
        private static object mutex = new object();

        public static IXmlValueObjectRepository Instance
        {
            get
            {
                lock (mutex)
                {
                    if (instance == null)
                    {
                        instance = new XmlValueObjectRepository();
                    }
                }
                return instance;
            }
        }
    }
}
