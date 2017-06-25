using System;
using TS4TranslateSupporter;

namespace xmlConnect
{
    class Program
    {
        static void Main(String[] args)
        {
            if( args.Length < 2 )
                throw new ArgumentException( "参数错误。参数格式应为：[-abConnect] [path] " );

            if( !args[0].Equals( "-abConnect", StringComparison.CurrentCultureIgnoreCase ) )
                throw new ArgumentException( "参数错误。参数格式应为：[-abConnect] [path] " );

            String _path = args[1];

            var _xmlEn = XmlDictionary.FromFile( _path + "\\XMLDictionary_En.xml", TranslateTo.B );
            var _xmlCn = XmlDictionary.FromFile( _path + "\\XMLDictionary_Cn.xml", TranslateTo.A );

            var _newXml = new XmlDictionary( _xmlEn.LanguageA, _xmlEn.LanguageB, TranslateTo.B );

            foreach( var _cn in _xmlCn )
            {
                foreach( var _en in _xmlEn )
                {
                   var _cnIt = _cn.Ident.Instance << ( 4 * 2 );
                   var _enIt = _en.Ident.Instance << ( 4 * 2 );

                    if( _cnIt == _enIt 
                        && _cn.Ident.KeyHash == _en.Ident.KeyHash
                        && _cn.Ident.ResourceGroup == _en.Ident.ResourceGroup )
                    {
                        StringEntryIdent _ident = null;
                        Entry _entry = new Entry( _en.A, _cn.A, _ident, "" );
                        _newXml.Add( _entry );

                        break;
                    }
                }
            }

            _newXml.SaveAs( _path + "\\XMLDictionary.xml" );

            Console.WriteLine( "按任意键退出..." );
            Console.ReadLine();
        }
    }
}
