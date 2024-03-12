namespace SaveSystem {

    public interface ISerializationAdapter<out TTarget> : IRuntimeSerializable {

        public TTarget Target { get; }

    }

}