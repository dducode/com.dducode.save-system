using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.Internal {

    public class File {

        internal string Name { get; private set; }
        internal string Extension { get; }
        internal string FullName { get; private set; }
        internal string Path => System.IO.Path.Combine(Directory.Path, FullName);
        internal Directory Directory { get; }

        internal long DataSize => System.IO.File.Exists(Path) ? new FileInfo(Path).Length : 0;
        internal bool IsEmpty => DataSize == 0;
        internal bool Exists => System.IO.File.Exists(Path);

        internal string OldFullName { get; private set; }
        internal string OldName { get; private set; }


        internal File (string name, string extension, Directory directory) {
            Directory = directory;
            Name = Directory.GenerateUniqueName(name);
            Extension = extension;
            FullName = $"{Name}.{Extension}";
        }


        internal void Rename (string newName) {
            string oldPath = Path;
            OldName = string.Copy(Name);
            Name = Directory.GenerateUniqueName(newName);
            OldFullName = string.Copy(FullName);
            FullName = $"{Name}.{Extension}";
            Directory.UpdateFile(this, OldName);

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Move(oldPath, Path);
        }


        internal FileStream Open () {
            return System.IO.File.Open(Path, FileMode.OpenOrCreate);
        }


        public byte[] ReadAllBytes () {
            return System.IO.File.ReadAllBytes(Path);
        }


        public string ReadAllText () {
            return System.IO.File.ReadAllText(Path);
        }


        internal async UniTask<byte[]> ReadAllBytesAsync (CancellationToken token = default) {
            return await System.IO.File.ReadAllBytesAsync(Path, token);
        }


        public void WriteAllBytes (byte[] bytes) {
            System.IO.File.WriteAllBytes(Path, bytes);
        }


        public void WriteAllText (string text) {
            System.IO.File.WriteAllText(Path, text);
        }


        internal async UniTask WriteAllBytesAsync (byte[] bytes, CancellationToken token = default) {
            await System.IO.File.WriteAllBytesAsync(Path, bytes, token);
        }


        internal void Delete () {
            Directory.DeleteFile(Name);
        }

    }

}