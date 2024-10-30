using System;
using SaveSystemPackage.Internal.Templates;
using UnityEngine;

namespace SaveSystemPackage.Settings {

    [Serializable]
    public class FileSystemCacheSettings {

        public enum SizeUnit {

            Byte,
            Kbyte,
            Mbyte

        }


        [Tooltip(Tooltips.SizeUnit)]
        public SizeUnit sizeUnit = SizeUnit.Kbyte;

        [Min(0), Tooltip(Tooltips.CacheSize)]
        public int cacheSize = 64;


        public int GetSize () {
            switch (sizeUnit) {
                case SizeUnit.Byte:
                    return cacheSize;
                case SizeUnit.Kbyte:
                    return cacheSize * 1024;
                case SizeUnit.Mbyte:
                    return cacheSize * 1_048_576;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}