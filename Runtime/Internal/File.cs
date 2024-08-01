using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.Internal {

    internal class File {

        internal string Name { get; private set; }
        internal string Extension { get; }
        public string FullName { get; private set; }
        internal string Path { get; private set; }
        internal Directory Directory { get; }
        internal long DataSize => new FileInfo(Path).Length;
        internal bool Exists => System.IO.File.Exists(Path);


        internal File (string name, string extension, Directory directory) {
            Directory = directory;
            Name = Directory.GenerateUniqueName(name);
            Extension = extension;
            FullName = $"{Name}.{Extension}";
            Path = System.IO.Path.Combine(Directory.Path, FullName);
        }


        internal void Rename (string newName) {
            string oldPath = Path;
            string oldName = Name;
            Name = Directory.GenerateUniqueName(newName);
            Path = oldPath.Replace(oldName, Name);
            FullName = $"{Name}.{Extension}";

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Move(oldPath, Path);
        }


        internal FileStream Open () {
            return System.IO.File.Open(Path, FileMode.OpenOrCreate);
        }


        public byte[] ReadAllBytes () {
            return System.IO.File.ReadAllBytes(Path);
        }


        internal async UniTask<byte[]> ReadAllBytesAsync (CancellationToken token = default) {
            return await System.IO.File.ReadAllBytesAsync(Path, token);
        }


        public void WriteAllBytes (byte[] bytes) {
            System.IO.File.WriteAllBytes(Path, bytes);
        }


        internal async UniTask WriteAllBytesAsync (byte[] bytes, CancellationToken token = default) {
            await System.IO.File.WriteAllBytesAsync(Path, bytes, token);
        }


        internal void Delete () {
            System.IO.File.Delete(Path);
        }

    }

}