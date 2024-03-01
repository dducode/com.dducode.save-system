namespace SaveSystem {

    public interface IObjectFactory<out TObject> {

        public TObject CreateObject ();

    }

}