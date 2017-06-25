using System;

namespace TS4TranslateSupporter
{
    public class EntryNotFoundException : Exception
    {
        XmlDictionary m_dict;
        Int32 m_changed;
        public XmlDictionary Dict => m_dict;
        public Int32 ChangedCount => m_changed;

        public EntryNotFoundException(XmlDictionary dict,Int32 changed)
        {
            m_dict = dict;
            m_changed = changed;
        }
    }
}
