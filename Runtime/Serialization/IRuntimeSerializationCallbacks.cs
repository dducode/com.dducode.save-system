namespace SaveSystemPackage.Serialization {

    public interface IRuntimeSerializationCallbacks {

        public void OnBeforeRuntimeSerialization ();
        public void OnAfterRuntimeDeserialization ();

    }

}