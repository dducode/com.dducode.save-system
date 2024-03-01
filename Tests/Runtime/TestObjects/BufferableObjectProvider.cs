namespace SaveSystem.Tests.TestObjects {

    public class BufferableObjectProvider : ISerializationProvider<BufferableObjectAdapter, BufferableObject> {

        public BufferableObjectAdapter GetAdapter (BufferableObject target) {
            return new BufferableObjectAdapter(target);
        }

    }

}