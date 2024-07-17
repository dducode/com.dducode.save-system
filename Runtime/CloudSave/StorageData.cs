using System;
using System.Diagnostics.CodeAnalysis;

namespace SaveSystem.CloudSave {

    public class StorageData {

        public readonly byte[] rawData;
        public readonly string fileName;


        public StorageData ([NotNull] byte[] rawData, [NotNull] string fileName) {
            if (rawData == null)
                throw new ArgumentNullException(nameof(rawData));
            if (rawData.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(rawData));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            this.rawData = rawData;
            this.fileName = fileName;
        }

    }

}