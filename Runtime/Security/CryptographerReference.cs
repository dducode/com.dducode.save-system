using UnityEngine;

namespace SaveSystemPackage.Security {

    public abstract class CryptographerReference : ScriptableObject {

        public static implicit operator Cryptographer (CryptographerReference reference) {
            return reference.GetCryptographer();
        }


        protected abstract Cryptographer GetCryptographer ();

    }

}