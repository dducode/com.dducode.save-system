namespace SaveSystemPackage.Providers {

    public interface IKeyProvider {

        public string Provide<TData> () where TData : ISaveData;
        public string Provide<TData> (string prefix) where TData : ISaveData;

    }

}