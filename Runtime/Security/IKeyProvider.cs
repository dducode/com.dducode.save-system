using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Security {

    public interface IKeyProvider : ICloneable<IKeyProvider> {

        public byte[] GetKey ();

    }

}