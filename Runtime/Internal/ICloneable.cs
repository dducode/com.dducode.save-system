namespace SaveSystemPackage.Internal {

    public interface ICloneable<out TClone> {

        public TClone Clone ();

    }

}