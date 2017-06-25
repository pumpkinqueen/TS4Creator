using System;

namespace TS4TranslateSupporter
{

    public class Entry
    {
        public String A { get; private set; }
        public String B { get; private set; }
        public StringEntryIdent Ident { get; private set; }
        public String Package { get; private set; }

        public Entry(String a , String b, StringEntryIdent ident, String package)
        {
            A = a;
            B = b;
            Ident = ident;
            Package = package;
        }
    }
}
