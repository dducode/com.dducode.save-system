namespace SaveSystem {

    public static partial class DataManager {

        private static readonly string MessageHeaderWarning = $"<color=yellow><b>{nameof(DataManager)}:</b></color>";
        private static readonly string MessageHeaderError = $"<color=red><b>{nameof(DataManager)}:</b></color>";


        private static string ObjectIsNullMessage (string methodName) {
            return $"{MessageHeaderError} Object can't be null in {methodName} method";
        }


        private static string ObjectsArrayIsNullMessage (string methodName) {
            return $"{MessageHeaderError} Objects array can't be null in {methodName} method";
        }


        private static string ObjectsArrayIsEmptyMessage (string methodName) {
            return $"{MessageHeaderWarning} Objects haven't been transferred in {methodName} method";
        }


        internal static string AsyncModeIsNotImplementMessage (AsyncMode asyncMode, string methodName) {
            return $"{MessageHeaderError} AsyncMode {asyncMode} isn't implement in {methodName} method";
        }

    }

}