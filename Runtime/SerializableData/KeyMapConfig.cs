using System;
using System.Collections.Generic;
using System.Reflection;

namespace SaveSystemPackage.SerializableData {

    [Serializable]
    public class KeyMapConfig : List<KeyMapItem>, ISaveData {

        internal static readonly KeyMapConfig Template = new() {
            new KeyMapItem {
                assemblyName = "YourFirstAssemblyName",
                types = new Map<string, string> {
                    {"YourNamespace.YourFirstExampleType", "first-type-key"},
                    {"YourNamespace.YourSecondExampleType", "second-type-key"},
                }
            },
            new KeyMapItem {
                assemblyName = "YourSecondAssemblyName",
                types = new Map<string, string> {
                    {"YourNamespace.YourThirdExampleType", "third-type-key"},
                }
            }
        };

        public static KeyMapConfig Empty = new();


        public static implicit operator KeyMap (KeyMapConfig config) {
            var map = new KeyMap();

            foreach (KeyMapItem item in config) {
                Assembly assembly = Assembly.Load(item.assemblyName);
                foreach ((string typeName, string typeKey) in item.types)
                    map.Add(assembly.GetType(typeName), typeKey);
            }

            return map;
        }


        public static implicit operator KeyMapConfig (KeyMap map) {
            var config = new KeyMapConfig();
            var item = new KeyMapItem {
                assemblyName = Assembly.GetExecutingAssembly().FullName,
                types = new Map<string, string>()
            };
            foreach ((Type type, string typeKey) in map)
                item.types.Add(type.FullName ?? throw new InvalidOperationException(), typeKey);
            config.Add(item);
            return config;
        }

    }

}