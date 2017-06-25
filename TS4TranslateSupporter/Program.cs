using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;

namespace TS4TranslateSupporter
{
    class Program
    {
        static void Main(String[] args)
        {
            if( args.Length < 4 )
                throw new ArgumentException( "参数错误。参数格式应为：[-a2b|-b2a] [dictionary path] [input path] [output path]" );

            TranslateTo _to = args[0].Equals( "-a2b", StringComparison.CurrentCultureIgnoreCase ) ? TranslateTo.B : TranslateTo.A;
            String _dictPath = Path.GetFullPath( args[1] );
            var _xmlDict = XmlDictionary.FromFile( _dictPath, _to );

            String _inputPath = Path.GetFullPath( args[2] );

            var _in_file_paths = new List<FileInfo>();
            ListFiles( new DirectoryInfo( _inputPath ), _in_file_paths );

            String _outputPath = Path.GetFullPath( args[3] );

            var _log = new List<String>();
            var _outDict = new XmlDictionary( _xmlDict.LanguageA, _xmlDict.LanguageB, _xmlDict.To );
            Int32 _changedCount = 0;
            foreach( var path in _in_file_paths )
            {
                //String _outPath = path.DirectoryName.Replace( _inputPath, _outputPath );
                //Translate( path, _outPath, _xmlDict );

                try
                {
                    String _outPath = path.DirectoryName.Replace( _inputPath, _outputPath );
                    Translate( path, _outPath, _xmlDict );
                }
                catch( EntryNotFoundException ex )
                {
                    _outDict.AddRange( ex.Dict );
                    _changedCount += ex.ChangedCount;
                }
                catch( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                    _log.Add( ex.Message );
                }
            }

            if( _log.Count != 0 )
            {
                StreamWriter log = new StreamWriter( _outputPath + "\\TS4TSLog.txt", true );

                log.WriteLine( "时间：" + System.DateTime.Now.ToLongTimeString() );

                log.WriteLine( "已汉化条目：" + _changedCount );
                log.WriteLine( "未汉化条目：" + _outDict.Count );

                for( Int32 i = 0; i < _log.Count; i++ )
                {
                    log.WriteLine( _log[i] );
                }

                log.Close();
            }

            if( _outDict.Count != 0 )
            {
                _outDict.SaveAs( _outputPath + "\\XMLDictionary.xml" );
            }

            Console.WriteLine( "按任意键退出..." );
            Console.ReadLine();
        }

        static Byte GetLanguageCode(UInt64 instance)
        {
            return (Byte)( instance >> ( 4 * 14 ));
        }

        static List<StringEntry> ReadStbl(String infileName, Stream _fs, IResourceIndexEntry item)
        {
            var _stbl = new List<StringEntry>();
            BinaryReader _binary_reader = new BinaryReader( _fs );

            if( _binary_reader.ReadByte() != 83
                || _binary_reader.ReadByte() != 84
                || ( _binary_reader.ReadByte() != 66 || _binary_reader.ReadByte() != 76 ) )
            {
                _fs.Dispose();
                throw new ApplicationException( "这个不是 STBL 文件。" );
            }

            switch( _binary_reader.ReadByte() )
            {
                case 2:
                    {
                        _fs.Dispose();
                        throw new ApplicationException( "这个是《模拟人生3》的 STBL 文件" );
                    }

                case 5:
                    {
                        Int32 _num9 = _binary_reader.ReadUInt16();
                        UInt32 _num10 = _binary_reader.ReadUInt32();
                        Int32 _num11 = _binary_reader.ReadUInt16();
                        Int32 _num12 = ( Int32 )_binary_reader.ReadUInt32();
                        Int64 _num13 = _binary_reader.ReadUInt32();
                        Int64 _num14 = 0;

                        for( Int32 i = 0; i < _num10; ++i )
                        {
                            UInt32 _key = _binary_reader.ReadUInt32();
                            Int32 _num1 = _binary_reader.ReadByte();
                            Int32 _count = _binary_reader.ReadUInt16();
                            String _val = Encoding.UTF8.GetString( _binary_reader.ReadBytes( _count ) );
                            _num14 = _count + _num14 + 1L;

                            StringEntry _entry = new StringEntry
                            {
                                Ident = new StringEntryIdent( item.ResourceType, item.ResourceGroup, item.Instance, _key ),
                                Text = _val
                            };
                            _stbl.Add( _entry );
                        }

                        if( _num14 != _num13 )
                        {
                            _fs.Dispose();
                            throw new ApplicationException( "STBL 文件读取失败。\n" + _num14.ToString() + "\n" + _num10.ToString() );
                        }

                        _fs.Dispose();
                    }
                    break;

                default:
                    {
                        _fs.Dispose();
                        throw new ApplicationException( "未知版本的 SBTL 文件。" );
                    }
            }

            return _stbl;
        }

        static void StringListToStbl(Stream ms, List<StringEntry> stringList)
        {
            Byte[] _header = new Byte[5] { 83, 84, 66, 76, 5 };
            Byte _zero8 = 0;
            UInt16 _zero16 = 0;
            UInt32 _count = ( UInt32 )stringList.Count;

            var _text_size = new List<UInt16>();
            UInt32 _data_size = 0;

            foreach( var _str in stringList )
            {
                UInt16 _size = ( UInt16 )Encoding.UTF8.GetByteCount( _str.Text );
                _text_size.Add( _size );
                _data_size = _size + _data_size + 1U;
            }

            var _br = new BinaryWriter( ms );
            _br.Write( _header );
            _br.Write( _zero16 );
            _br.Write( _count );
            _br.Write( _zero16 );
            _br.Write( _zero16 );
            _br.Write( _zero16 );
            _br.Write( _data_size );

            for( Int32 i = 0; i < _count; ++i )
            {
                _br.Write( stringList[i].Ident.KeyHash );
                _br.Write( _zero8 );
                _br.Write( _text_size[i] );
                _br.Write( stringList[i].Text .ToCharArray() );
            }
        }

        static void Translate(FileInfo inFile, String outPath, XmlDictionary dict)
        {
            if( !File.Exists( inFile.FullName ) )
                throw new FileNotFoundException( inFile.FullName );

            String _outFile = outPath + "\\" + inFile.Name;
            if( !Directory.Exists( outPath ) )
            {
                Directory.CreateDirectory( outPath );
            }

            IPackage _package = Package.OpenPackage( -1, inFile.FullName );

            Byte _targetCode = 0;
            var _stblIndexEntry = new List<KeyValuePair<IResourceIndexEntry, IResourceIndexEntry>>();
            {
                List<IResourceIndexEntry> _resourceEntry = _package.GetResourceList;
                var _stblRes = _resourceEntry.Where( (o) => { return o.ResourceType == 0x220557DA; } );

                Byte _sourceCode = dict.To == TranslateTo.B ? dict.LanguageA : dict.LanguageB;
                IEnumerable<IResourceIndexEntry> _stblScoure = _stblRes.Where( (o) => { return GetLanguageCode( o.Instance ) == _sourceCode; } );

                _targetCode = dict.To == TranslateTo.A ? dict.LanguageA : dict.LanguageB;
                IEnumerable<IResourceIndexEntry> _stblTarget = _stblRes.Where( (o) => { return GetLanguageCode( o.Instance ) == _targetCode; } );

                foreach( var s in _stblScoure )
                {
                    Boolean hasTarget = false;
                    UInt64 _is = s.Instance << ( 4 * 2 );
                    foreach( var t in _stblTarget )
                    {
                        UInt64 _it = t.Instance << ( 4 * 2 );
                        if( s.ResourceType == t.ResourceType
                            && s.ResourceGroup == t.ResourceGroup
                            && _is == _it )
                        {
                            hasTarget = true;
                            _stblIndexEntry.Add( new KeyValuePair<IResourceIndexEntry, IResourceIndexEntry>( s, t ) );
                            break;
                        }
                    }

                    if(!hasTarget )
                    {
                        _stblIndexEntry.Add( new KeyValuePair<IResourceIndexEntry, IResourceIndexEntry>( s, null ) );
                    }
                }
            }

            if( _stblIndexEntry.Count() == 0 )
            {
                _package.SaveAs( _outFile );
                throw new ApplicationException( $"源语言不存在，文件未修改。文件：{_outFile}" );
            }
            else
            {
                var _newDict = new XmlDictionary( dict.LanguageA, dict.LanguageB, dict.To );

                Int32 _changedCount=0;
                foreach( var item in _stblIndexEntry )
                {
                    IResource res = WrapperDealer.GetResource( -1, _package, item.Key, false );

                    Stream _fs = res.Stream;
                    List<StringEntry> _stringList = ReadStbl( inFile.Name, _fs, item.Key );

                    foreach( var str in _stringList )
                    {
                        if( dict.TryFind( str.Text,inFile.Name,str.Ident, out String s ) )
                        {
                            str.Text = s;
                            _changedCount++;
                        }
                        else
                        {
                            Entry _entry = new Entry( str.Text, str.Text, str.Ident, inFile.Name );
                            _newDict.Add( _entry );
                        }
                    }

                    var _ms = new MemoryStream();
                    StringListToStbl( _ms, _stringList);

                    if( item.Value == null )
                    {
                        var _rk = new TranslateResourceIndexEntry();
                        _rk.ResourceGroup = item.Key.ResourceGroup;
                        _rk.ResourceType = item.Key.ResourceType;
                        _rk.Instance = item.Key.Instance;
                        _rk.Instance = _rk.Instance << ( 4 * 2 );
                        _rk.Instance = _rk.Instance >> ( 4 * 2 );
                        _rk.Instance = _rk.Instance + ( ( ( UInt64 )_targetCode ) << ( 4 * 14 ) );

                        _package.AddResource( _rk, _ms, true );
                    }
                    else
                    {
                        var _st = new StblResource.StblResource( 0, _ms );
                        _package.ReplaceResource( item.Value, _st );
                    }

                    _package.SaveAs( _outFile );

                }

                if( _newDict.Count != 0 )
                {
                    throw new EntryNotFoundException( _newDict , _changedCount );
                }
            }
        }

        static void ListFiles(FileSystemInfo info, List<FileInfo> packages)
        {
            if( !info.Exists ) return;
            DirectoryInfo dir = info as DirectoryInfo;

            if( dir == null ) return;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for( Int32 i = 0; i < files.Length; i++ )
            {
                if( files[i] is FileInfo file )
                {
                    if( file.Extension.Equals( ".package", StringComparison.CurrentCultureIgnoreCase ) )
                    {
                        packages.Add( file );
                    }
                }
                else
                {
                    ListFiles( files[i], packages );
                }
            }
        }
    }
}
