namespace SaveSystemPackage.Internal.Extensions {

    internal static class ArrayExtensions {

        internal static (T[], T[]) Split<T> (this T[] array, int index) {
            return (array[..index], array[index..]);
        }

    }

}