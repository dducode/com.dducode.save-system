namespace SaveSystemPackage {

    public interface ISaveData {

        public int Version => 0;
        public ISaveData Default => default;

    }

}