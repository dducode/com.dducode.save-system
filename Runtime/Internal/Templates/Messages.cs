﻿namespace SaveSystem.Internal.Templates {

    public static class Messages {

        public const string RegistrationClosed =
            "Registration was closed. You cannot register objects after loading or saving. If you want to add object as dynamically, you can use DynamicObjectFactory instead";

        public const string CannotReadData = "Cannot read data when it hasn't loaded";

        public const string SettingsNotFound =
            "Save system settings not found. Use default instead.\nTo create settings, select \"Assets/Create/Save System/Save System Settings\"";

    }

}