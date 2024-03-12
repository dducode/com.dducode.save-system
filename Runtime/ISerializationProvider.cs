namespace SaveSystem {

    public interface ISerializationProvider<out TAdapter, in TTarget> where TAdapter : ISerializationAdapter<TTarget> {

        public TAdapter GetAdapter (TTarget target);

    }

}