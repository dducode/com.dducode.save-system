namespace SaveSystem {

    public static partial class DataManager {

        private static readonly string MessageHeaderLog = $"<b>{nameof(DataManager)}:</b>";
        private static readonly string MessageHeaderWarning = $"<color=yellow><b>{nameof(DataManager)}:</b></color>";
        private static readonly string MessageHeaderError = $"<color=red><b>{nameof(DataManager)}:</b></color>";
        private static readonly string CancelLoadingMessage = $"{MessageHeaderLog} Object loading canceled";

        private static readonly string CancelSavingMessage =
            $"{MessageHeaderWarning} Objects saving canceled. Data file deleted";

        private const string SAVING_OPERATION = "saving";
        private const string LOADING_OPERATION = "loading";


        private static string ObjectIsNullMessage (string operation) {
            return $"{MessageHeaderError} Object for {operation} can't be null";
        }


        private static string ObjectsArrayIsNullMessage (string operation) {
            return $"{MessageHeaderError} Objects array for {operation} can't be null";
        }


        private static string ObjectsArrayIsEmptyMessage (string operation) {
            return $"{MessageHeaderWarning} Objects for {operation} haven't been transferred";
        }


        private static string AsyncModeIsNotImplementMessage (AsyncMode asyncMode, string methodName) {
            return $"{MessageHeaderError} AsyncMode {asyncMode} isn't implement in method {methodName}";
        }

    }

}