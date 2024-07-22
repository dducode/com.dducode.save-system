namespace SaveSystemPackage {

    public interface ISerializationAdapter<out TTarget> : IRuntimeSerializable {

        public TTarget Target { get; }

    }

}