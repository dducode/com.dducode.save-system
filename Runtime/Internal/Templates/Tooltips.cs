namespace SaveSystem.Internal.Templates {

    public static class Tooltips {

        public const string SavePeriod = "It's used into autosave loop to determine saving frequency" +
                                                "\nIf it equals 0, saving will be executed at every frame";

        public const string IsParallel = "Configure it to set parallel saving handlers" +
                                                "\nYou must ensure that your objects are thread safe";

        public const string DataPath = "Path to save global data (registered in the Core)";

    }

}