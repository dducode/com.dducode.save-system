namespace SaveSystemPackage.Tests.TestObjects {

    public interface IObjectFactory<out TObject> {

        public TObject CreateObject ();

    }

}