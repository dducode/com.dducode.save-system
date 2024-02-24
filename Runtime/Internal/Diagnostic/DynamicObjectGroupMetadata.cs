using System;
using System.Runtime.InteropServices;

namespace SaveSystem.Internal.Diagnostic {

    [StructLayout(LayoutKind.Auto)]
    internal record DynamicObjectGroupMetadata {

        internal readonly string caller;
        internal readonly Type objectsType;
        internal readonly GCHandle handle;
        internal int objectsCount;


        internal DynamicObjectGroupMetadata (
            string caller,
            Type objectsType,
            GCHandle handle
        ) {
            this.caller = caller;
            this.objectsType = objectsType;
            this.handle = handle;

            objectsCount = 0;
        }

    }

}