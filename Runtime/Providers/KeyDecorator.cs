namespace SaveSystemPackage.Providers {

    public class KeyDecorator : IKeyProvider {

        private readonly IKeyProvider m_baseProvider;
        private readonly string m_persistentPrefix;


        public KeyDecorator (IKeyProvider baseProvider, string persistentPrefix) {
            m_baseProvider = baseProvider;
            m_persistentPrefix = persistentPrefix;
        }


        public string Provide<TData> () where TData : ISaveData {
            return $"{m_persistentPrefix}_{m_baseProvider.Provide<TData>()}";
        }


        public string Provide<TData> (string prefix) where TData : ISaveData {
            return $"{prefix}_{m_persistentPrefix}_{m_baseProvider.Provide<TData>()}";
        }

    }

}