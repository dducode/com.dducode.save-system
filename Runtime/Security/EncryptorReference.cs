using UnityEngine;

namespace SaveSystemPackage.Security {

    public abstract class EncryptorReference : ScriptableObject {

        protected abstract IEncryptor GetEncryptor ();

    }

}