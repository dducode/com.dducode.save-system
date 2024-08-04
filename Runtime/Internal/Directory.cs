using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SaveSystemPackage.Internal {

    internal class Directory {

        internal string Name { get; private set; }
        internal string Path => System.IO.Path.Combine(Parent != null ? Parent.Path : RootPath, Name);
        internal Directory Parent { get; }
        internal Directory Root { get; }
        private string RootPath { get; }

        internal long DataSize {
            get {
                long size = 0;

                foreach (Directory directory in m_directories.Values)
                    size += directory.DataSize;
                foreach (File file in m_files.Values)
                    size += file.DataSize;

                return size;
            }
        }

        internal bool IsEmpty => m_directories.Count == 0 && m_files.All(p => p.Value.IsEmpty);
        internal bool Exists => System.IO.Directory.Exists(Path);

        private readonly Dictionary<string, File> m_files = new();
        private readonly Dictionary<string, Directory> m_directories = new();


        internal static Directory CreateRoot (string name, string path, FileAttributes attributes = 0) {
            return new Directory(name, path, attributes);
        }


        private Directory (string name, string path, FileAttributes attributes = 0) {
            Name = name;
            RootPath = path;
            Root = this;
            DirectoryInfo info = System.IO.Directory.CreateDirectory(Path);
            info.Attributes |= attributes;
            Init();
        }


        private Directory (string name, Directory parent, FileAttributes attributes = 0) {
            Parent = parent;
            Name = parent.GenerateUniqueName(name);
            Root = parent.Root;
            DirectoryInfo info = System.IO.Directory.CreateDirectory(Path);
            info.Attributes |= attributes;
            Init();
        }


        internal Directory GetOrCreateDirectory (string name, FileAttributes attributes = 0) {
            if (!m_directories.ContainsKey(name))
                m_directories.Add(name, new Directory(name, this, attributes));
            return m_directories[name];
        }


        internal File CreateFile (string name, string extension) {
            var file = new File(name, extension, this);
            m_files.Add(file.Name, file);
            return file;
        }


        internal IEnumerable<File> EnumerateFiles () {
            foreach (File file in m_files.Values)
                yield return file;
        }


        internal IEnumerable<File> EnumerateFiles (string extension) {
            foreach (File file in m_files.Values)
                if (string.Equals(file.Extension, extension))
                    yield return file;
        }


        internal IEnumerable<Directory> EnumerateDirectories () {
            foreach (Directory directory in m_directories.Values)
                yield return directory;
        }


        internal File GetFile (string name) {
            if (!m_files.ContainsKey(name))
                throw new InvalidOperationException($"Directory doesn't contain file with name \"{name}\"");
            return m_files[name];
        }


        internal bool TryGetFile (string name, out File file) {
            return m_files.TryGetValue(name, out file);
        }


        internal File GetOrCreateFile (string name, string extension) {
            return m_files.TryGetValue(name, out File file) ? file : CreateFile(name, extension);
        }


        internal bool ContainsFile (string name) {
            return m_files.ContainsKey(name);
        }


        internal bool ContainsDirectory (string name) {
            return m_directories.ContainsKey(name);
        }


        internal bool ContainsEntry (string name) {
            return ContainsDirectory(name) || ContainsFile(name);
        }


        internal void DeleteFile (string fileName) {
            if (!m_files.ContainsKey(fileName))
                return;

            string path = m_files[fileName].Path;
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            m_files.Remove(fileName);
        }


        internal void UpdateFile (File file, string oldName) {
            if (!m_files.ContainsKey(oldName))
                throw new InvalidOperationException($"Directory doesn't contain file with name \"{oldName}\"");
            m_files.Remove(oldName);
            m_files.Add(file.Name, file);
        }


        internal void Rename (string newName) {
            string oldName = Name;
            string oldPath = Path;
            Name = Parent.GenerateUniqueName(newName);
            Parent.UpdateDirectory(this, oldName);

            if (System.IO.Directory.Exists(oldPath))
                System.IO.Directory.Move(oldPath, Path);
        }


        internal void Clear () {
            foreach (Directory directory in m_directories.Values.Where(d => d.Exists))
                System.IO.Directory.Delete(directory.Path, true);

            foreach (File file in m_files.Values.Where(f => f.Exists))
                System.IO.File.Delete(file.Path);

            m_directories.Clear();
            m_files.Clear();
        }


        internal void Delete () {
            Parent?.DeleteDirectory(Name);
        }


        internal string GenerateUniqueName (string name) {
            if (!ContainsEntry(name))
                return name;

            var index = 1;
            string uniqueName;

            do {
                uniqueName = $"{name} {index}";
                ++index;
            } while (ContainsEntry(uniqueName));

            return uniqueName;
        }


        private void UpdateDirectory (Directory directory, string oldName) {
            if (!m_directories.ContainsKey(oldName))
                throw new InvalidOperationException($"Directory doesn't contain directory with name \"{oldName}\"");
            m_directories.Remove(oldName);
            m_directories.Add(directory.Name, directory);
        }


        private void DeleteDirectory (string directoryName) {
            if (!m_directories.ContainsKey(directoryName))
                return;

            string path = m_directories[directoryName].Path;
            if (System.IO.Directory.Exists(path))
                System.IO.Directory.Delete(path, true);
            m_directories.Remove(directoryName);
        }


        private void Init () {
            foreach (string path in System.IO.Directory.EnumerateDirectories(Path)) {
                string name = new DirectoryInfo(path).Name;
                var directory = new Directory(name, this);
                m_directories.Add(directory.Name, directory);
            }

            foreach (string path in System.IO.Directory.EnumerateFiles(Path)) {
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                string extension = System.IO.Path.GetExtension(path).Remove(0, 1);
                var file = new File(name, extension, this);
                m_files.Add(file.Name, file);
            }
        }

    }

}