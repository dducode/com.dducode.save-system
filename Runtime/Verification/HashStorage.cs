using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.Verification {

    public abstract class HashStorage {

        public abstract byte[] this [string key] { get; set; }
        protected readonly Dictionary<string, byte[]> map = new();


        public abstract UniTask Open ();
        public abstract UniTask Close ();

    }

}