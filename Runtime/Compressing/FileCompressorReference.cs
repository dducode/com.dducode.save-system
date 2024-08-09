using UnityEngine;

namespace SaveSystemPackage.Compressing {

    public abstract class FileCompressorReference : ScriptableObject {

        public static implicit operator FileCompressor (FileCompressorReference reference) {
            return reference.GetCompressor();
        }


        protected abstract FileCompressor GetCompressor ();

    }

}