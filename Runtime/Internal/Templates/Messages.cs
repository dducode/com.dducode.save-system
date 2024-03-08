namespace SaveSystem.Internal.Templates {

    public static class Messages {

        public const string RegistrationClosed =
            "Registration was closed. You cannot register objects after loading or saving. If you want to add object as dynamically, you can use DynamicObjectFactory instead";

        public const string TryingToReadNotLoadedData = "Trying to read a not loaded data";

        public const string SettingsNotFound =
            "Save system settings not found. Use default instead.\nTo create settings, select \"Assets/Create/Save System/Save System Settings\"";

        public const string DataIsCorrupted =
            "Data is corrupted. If you are sure of the authenticity of the data, make sure that you delete player prefs with data files. " +
            "Separately deleting may cause inconsistency";

    }

}