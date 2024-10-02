using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Security {

    public interface ISecurityKeyProvider : ICloneable<ISecurityKeyProvider> {

        public Key GetKey ();

    }

}