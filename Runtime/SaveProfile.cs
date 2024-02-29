using System.IO;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystem {

    public class SaveProfile {

        public string Name { get; set; }

        public string DataPath {
            get => m_dataPath;
            set => m_dataPath = Storage.PrepareBeforeUsing(value, true);
        }

        private string m_dataPath;


        public virtual void Serialize (BinaryWriter writer) {
            writer.Write(Name);
            writer.Write(DataPath);
        }


        public virtual void Deserialize (BinaryReader reader) {
            Name = reader.ReadString();
            DataPath = reader.ReadString();
        }


        public override string ToString () {
            return $"name: {Name}, path: {Path.GetRelativePath(Storage.PersistentDataPath, m_dataPath)}";
        }

    }

}