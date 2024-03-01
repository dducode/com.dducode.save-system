namespace SaveSystem.Internal {

    public static class MessageTemplates {

        public const string RegistrationClosedMessage =
            "Registration was closed. You cannot register objects after loading or saving. If you want to add object as dynamically, you can use DynamicObjectFactory instead";

        public const string SavePeriodTooltip = "It's used into autosave loop to determine saving frequency" +
                                                "\nIf it equals 0, saving will be executed at every frame";

        public const string IsParallelTooltip = "Configure it to set parallel saving handlers" +
                                                "\nYou must ensure that your objects are thread safe";

        public const string DataPathTooltip = "Path to save global data (registered in the Core)";

    }

}