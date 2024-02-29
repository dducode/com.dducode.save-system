using System.Runtime.InteropServices;

namespace SaveSystem.Internal.Diagnostic {

    [StructLayout(LayoutKind.Auto)]
    internal record ObjectMetadata {

        internal readonly string caller;
        internal readonly GCHandle handle;


        internal ObjectMetadata (
            string caller,
            GCHandle handle
        ) {
            this.caller = caller;
            this.handle = handle;
        }

    }

}