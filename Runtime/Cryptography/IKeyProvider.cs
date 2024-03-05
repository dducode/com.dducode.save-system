namespace SaveSystem.Cryptography {

    public interface IKeyProvider<out TKey> {

        public TKey GetKey ();

    }

}