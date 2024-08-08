using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SaveSystemPackage.Security {

    public class Key {

        public readonly byte[] value;
        private GCHandle m_handle;


        public Key (byte[] value, bool createCopy = false) {
            if (createCopy) {
                this.value = new byte[value.Length];
                value.CopyTo(this.value, 0);
            }
            else {
                this.value = value;
            }
        }


        public Key Pin () {
            m_handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            return this;
        }


        public void Free () {
            CryptographicOperations.ZeroMemory(value);
            m_handle.Free();
        }

    }

}