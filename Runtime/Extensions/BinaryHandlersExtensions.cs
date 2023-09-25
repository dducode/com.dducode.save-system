using System.IO;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Extensions {

    public static class BinaryHandlersExtensions {

        public static UnityWriter ExpandToUnityWriter (this BinaryWriter binaryWriter) {
            return new UnityWriter(binaryWriter);
        }


        public static UnityReader ExpandToUnityReader (this BinaryReader binaryReader) {
            return new UnityReader(binaryReader);
        }

    }

}