using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Verification {

    public abstract class HashStorage {

        public abstract byte[] this [string key] { get; set; }
        protected readonly Dictionary<string, byte[]> map = new();


        public abstract UniTask Open ();
        public abstract UniTask Close ();

        public abstract void Add (File file, byte[] bytes);
        public abstract byte[] Get (File file);
        public abstract void RenameLink (File file);

    }

}