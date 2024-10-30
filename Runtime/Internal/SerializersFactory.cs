using System;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Settings;
using UnityEngine;

namespace SaveSystemPackage.Internal {

    internal static class SerializersFactory {

        public static ISerializer Create (SaveSystemSettings settings) {
            ISerializer serializer = SelectBaseSerializer(settings);
            if (serializer == null)
                return null;
            if (Debug.isDebugBuild)
                return serializer;

            if (settings.encrypt && settings.compress) {
                return new CompositeSerializer(
                    serializer,
                    new AesEncryptor(settings.encryptionSettings),
                    new DeflateCompressor(settings.compressionSettings)
                );
            }
            else {
                if (settings.compress)
                    return new CompressionSerializer(serializer, new DeflateCompressor(settings.compressionSettings));
                else if (settings.encrypt)
                    return new EncryptionSerializer(serializer, new AesEncryptor(settings.encryptionSettings));
                else
                    return serializer;
            }
        }


        private static ISerializer SelectBaseSerializer (SaveSystemSettings settings) {
            SerializerType serializerType = settings.serializerType;

            switch (serializerType) {
                case SerializerType.BinarySerializer:
                    return new BinarySerializer();
                case SerializerType.JSONSerializer:
                    return new JsonSerializer(settings.jsonSerializationSettings);
                case SerializerType.XMLSerializer:
                    return new XmlSerializer();
                case SerializerType.YAMLSerializer:
                    return new YamlSerializer();
                case SerializerType.Custom:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, null);
            }
        }

    }

}