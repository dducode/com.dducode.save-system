using System;

namespace SaveSystemPackage.Internal {

    internal class TempFile : File, IDisposable {

        internal TempFile (string name, string extension, Directory directory) : base(name, extension, directory) { }


        public void Dispose () {
            Delete();
        }

    }

}