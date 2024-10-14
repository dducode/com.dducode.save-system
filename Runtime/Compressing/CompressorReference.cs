using UnityEngine;

namespace SaveSystemPackage.Compressing {

    public abstract class CompressorReference : ScriptableObject {

        protected abstract ICompressor GetCompressor ();

    }

}