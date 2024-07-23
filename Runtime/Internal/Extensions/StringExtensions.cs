namespace SaveSystemPackage.Internal.Extensions {

    public static class StringExtensions {

        public static string ToPathFormat (this string str) {
            return str.ToLower().Replace(' ', '-');
        }

    }

}