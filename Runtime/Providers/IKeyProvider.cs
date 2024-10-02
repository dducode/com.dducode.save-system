using System;

namespace SaveSystemPackage.Providers {

    public interface IKeyProvider {

        public string GetKey<TData> () where TData : ISaveData;


        public void AddKey<TData> (string key) where TData : ISaveData {
            throw new NotImplementedException();
        }

    }

}