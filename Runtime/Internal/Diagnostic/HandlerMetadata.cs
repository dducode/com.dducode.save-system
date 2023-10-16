using System;
using System.Runtime.InteropServices;

namespace SaveSystem.Internal.Diagnostic {

    [StructLayout(LayoutKind.Auto)]
    internal readonly struct HandlerMetadata {

        internal readonly string destinationPath;
        internal readonly string caller;
        internal readonly Type handlerType;
        internal readonly Type objectsType;
        internal readonly GCHandle handle;
        internal readonly int objectsCount;


        internal HandlerMetadata (
            string destinationPath,
            string caller,
            Type handlerType,
            Type objectsType,
            GCHandle handle,
            int objectsCount
        ) {
            this.destinationPath = destinationPath;
            this.caller = caller;
            this.objectsType = objectsType;
            this.objectsCount = objectsCount;
            this.handlerType = handlerType;
            this.handle = handle;
        }

    }

}