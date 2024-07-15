namespace SaveSystem.CloudSave {

    public struct StorageData {

        public byte[] rawData;
        public string fileName;
        public Type type;



        public enum Type {

            Global,
            Profile

        }

    }

}