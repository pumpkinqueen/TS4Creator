using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TS4TranslateSupporter
{
    public class XmlDictionary : List<Entry>
    {
        const String DICTIONARY = "Dictionary";
        const String ENTRY = "Entry";
        const String A = "A";
        const String B = "B";
        const String NAMESPACE = "Namespace";

        public System.Byte LanguageA { get; }
        public System.Byte LanguageB { get; }

        public TranslateTo To { get; }

        public XmlDictionary(System.Byte languageA, System.Byte languageB, TranslateTo to)
        {
            LanguageA = languageA;
            LanguageB = languageB;
            To = to;
        }

       static  Boolean IsEquals(StringEntryIdent a,StringEntryIdent b)
        {
            if( a == null || b ==null )
                return false;

           UInt64 _ia =  a.Instance << ( 4 * 2 );
           UInt64 _ib =  b.Instance << ( 4 * 2 );
            return a.KeyHash == b.KeyHash
                && a.ResourceType == b.ResourceType
                && a.ResourceGroup == b.ResourceGroup
                && _ia == _ib;
        }

        static Boolean HasIdent(Entry a)
        {
            return a != null
                    && a.Ident != null
                    && a.Ident.KeyHash == 0
                    && a.Ident.ResourceType == 0
                    && a.Ident.ResourceGroup == 0
                    && a.Ident.Instance == 0;
        }

        Boolean FindText(String sourceText,IEnumerable<Entry> query, out String text)
        {
            text = String.Empty;

            foreach( var item in query )
            {
                String _from = To == TranslateTo.B ? item.A : item.B;
                String _to = To == TranslateTo.A ? item.A : item.B;
                if( sourceText == _from )
                {
                    text = _to;
                    return true;
                }
            }

            return false;
        }

        public Boolean TryFind(String sourceText, String package, StringEntryIdent ident, out String text)
        {
            var _q = this.Where( (o) => { return IsEquals( o.Ident, ident ) && package.Equals( o.Package, StringComparison.CurrentCultureIgnoreCase ); } );
            if( FindText( sourceText, _q, out text ) )
                return true;

            _q = this.Where( (o) => { return o.Ident == null && package.Equals( o.Package, StringComparison.CurrentCultureIgnoreCase ); } );
            if( FindText( sourceText, _q, out text ) )
                return true;

            _q = this.Where( (o) => { return o.Ident == null && o.Package == null; } );
            if( FindText( sourceText, _q, out text ) )
                return true;

            return false;
        }

        public void SaveAs(String path)
        {
            var _la = "0x" + Convert.ToString( LanguageA, 16 );
            var _lb = "0x" + Convert.ToString( LanguageB, 16 );
            var _x_root = new XElement( DICTIONARY, new XAttribute( A, _la ), new XAttribute( B, _lb ) );

            foreach( var _kvp in this )
            {
                var _a = _kvp.A;
                var _b = _kvp.B;

                var _ns = _kvp.Package;
                if( _kvp.Ident != null )
                {
                    var _ident = _kvp.Ident.ToString();
                    _ns += "|" + _ident;
                }

                var _xe = new XElement( ENTRY,
                    new XAttribute( A, _a ), 
                    new XAttribute( B, _b ), 
                    new XAttribute( NAMESPACE, _ns ) );
                _x_root.Add( _xe );
            }

            var _xml = new XDocument( _x_root );
            _xml.Save( path );
        }

        public static XmlDictionary FromFile(String xmlDictionaryPath, TranslateTo to)
        {
            XmlDictionary xmlDictionary;
            var _xe = XDocument.Load( xmlDictionaryPath )?.Element( DICTIONARY )?? null;

            if( _xe ==null )
                throw new ApplicationException( "Xml读取失败。" );

            String _as =  _xe.Attribute( A )?.Value??String.Empty ;
            Byte _a = Convert.ToByte( _as, 16 );
            String _bs =  _xe.Attribute( B )?.Value ??String.Empty;
            Byte _b = Convert.ToByte( _bs, 16 );

            if( String.IsNullOrEmpty(_as) || String.IsNullOrEmpty( _bs ) )
                throw new ApplicationException( "语言代码不能为空。" );

            xmlDictionary = new XmlDictionary( _a, _b, to );
            var _enumerable = _xe.Elements( ENTRY );
            foreach( var item in _enumerable )
            {
                String _ia = item.Attribute( A )?.Value ?? String.Empty;
                String _ib = item.Attribute( B )?.Value ?? String.Empty;

                if( String.IsNullOrEmpty( to == TranslateTo.A ? _ib : _ia ) )
                    continue;

                StringEntryIdent _ident = null;
                String _package = null;
                String _ns = item.Attribute( NAMESPACE )?.Value ?? String.Empty;
                if( !String.IsNullOrEmpty( _ns ) )
                {
                    String[] _sp = _ns.Split( '|' );
                    if( _sp.Length >= 1 )
                    {
                        if( _sp[0].ToLower().IndexOf( ".package" ) != -1 )
                        {
                            _package = _sp[0];

                            if( _sp.Length == 2 )
                            {
                                try
                                {
                                    _ident = _sp[1].ToStringEntryIdent();
                                }
                                catch( Exception )
                                {
                                    _ident = null;
                                }
                            }
                        }
                    }
                }

                Entry _entry = new Entry( _ia, _ib, _ident, _package );
                xmlDictionary.Add( _entry );
            }

            return xmlDictionary;
        }

    }
}
