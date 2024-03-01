namespace SaveSystem {

    public interface ISerializationAdapter<out TTarget> : IRuntimeSerializable {

        public TTarget Target { get; }

    }



    public interface IAsyncSerializationAdapter<out TTarget> : IAsyncRuntimeSerializable {

        public TTarget Target { get; }

    }

}