using System;

namespace SaveSystemPackage.Internal.Diagnostic {

    public class ObjectData {

        public readonly WeakReference<object> reference;
        public readonly int totalDataSize;
        public readonly int clearDataSize;


        public ObjectData (WeakReference<object> reference, int totalDataSize, int clearDataSize) {
            this.reference = reference;
            this.totalDataSize = totalDataSize;
            this.clearDataSize = clearDataSize;
        }

    }

}