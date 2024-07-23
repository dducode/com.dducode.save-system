namespace SaveSystemPackage.Internal {

    public static class ByteExtensions {

        public static bool EqualsBytes (this byte[] source, byte[] target) {
            if (source.Length != target.Length)
                return false;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != target[i])
                    return false;

            return true;
        }

    }

}