namespace SaveSystemPackage {

    public interface IObjectFactory<out TObject> {

        public TObject CreateObject ();

    }

}