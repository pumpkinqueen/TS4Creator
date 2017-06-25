using System;

namespace TS4TranslateSupporter
{
    public class StringEntryIdent
    {
        public UInt32 ResourceType { get; private set; }
        public UInt32 ResourceGroup { get; private set; }
        public UInt64 Instance { get; private set; }
        public UInt32 KeyHash { get; private set; }

        public StringEntryIdent(UInt32 resourceType,UInt32 resourceGroup,UInt64 instance,UInt32 keyHash)
        {
            ResourceType = resourceType;
            ResourceGroup = resourceGroup;
            Instance = instance;
            KeyHash = keyHash;
        }

        public override String ToString()
        {
            return ResourceType.ToString( "X8" ) + "-"
                    + ResourceGroup.ToString( "X8" ) + "-"
                    + Instance.ToString( "X16" ) + "-"
                    + KeyHash.ToString( "X8" );
        }
    }
}
