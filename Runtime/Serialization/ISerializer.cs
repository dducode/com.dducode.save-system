namespace SaveSystemPackage.Serialization {

    public interface ISerializer {

        public byte[] Serialize<TData> (TData data) where TData : ISaveData;
        public TData Deserialize<TData> (byte[] data) where TData : ISaveData;
        public string GetFormatCode ();

    }

}