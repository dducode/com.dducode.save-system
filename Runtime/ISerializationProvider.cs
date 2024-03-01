namespace SaveSystem {

    public interface ISerializationProvider<out TAdapter, in TTarget> where TAdapter : ISerializationAdapter<TTarget> {

        public TAdapter GetAdapter (TTarget target);

    }



    public interface IAsyncSerializationProvider<out TAdapter, in TTarget>
        where TAdapter : IAsyncSerializationAdapter<TTarget> {

        public TAdapter GetAdapter (TTarget target);

    }

}