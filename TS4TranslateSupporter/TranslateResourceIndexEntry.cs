using System;
using System.IO;
using s4pi.Interfaces;

namespace TS4TranslateSupporter
{
    public class TranslateResourceIndexEntry : AResourceIndexEntry
    {
        public override UInt32 Chunkoffset { get; set; }
        public override UInt32 Filesize { get; set; }
        public override UInt32 Memsize { get; set; }
        public override UInt16 Compressed { get; set; }
        public override UInt16 Unknown2 { get; set; }

        public override Stream Stream { get; }

        public override Boolean IsDeleted { get; set; }

        public override Int32 RecommendedApiVersion { get; }

        public override Boolean Equals(IResourceIndexEntry other)
        {
           return base.Equals( (Object)other );
        }

        public override Int32 GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
