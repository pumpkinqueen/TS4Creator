using System;

namespace TS4TranslateSupporter
{
    public static class Extension
    {
        public static StringEntryIdent ToStringEntryIdent(this String s)
        {
            string[] sArray = s.Split( '-' );

            if( sArray.Length != 4 )
                throw new ApplicationException( "s格式错误。" );

            UInt32 _val0 = Convert.ToUInt32( sArray[0], 16 );
            UInt32 _val1 = Convert.ToUInt32( sArray[1], 16 );
            UInt64 _val2 = Convert.ToUInt64( sArray[2], 16 );
            UInt32 _val3 = Convert.ToUInt32( sArray[3], 16 );

            return new StringEntryIdent(_val0 , _val1, _val2, _val3 );
        }
    }
}
