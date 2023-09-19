using SaveSystem.UnityHandlers;

namespace SaveSystem {

    /// <summary>
    /// You can implement it to mark your object as persistent
    /// </summary>
    public interface IPersistentObject {

        /// <summary>
        /// It will be called when system will saving data
        /// </summary>
        /// <param name="writer"> <see cref="UnityWriter"/> </param>
        public void Save (UnityWriter writer);


        /// <summary>
        /// It will be called when system will loading data
        /// </summary>
        /// <param name="reader"> <see cref="UnityReader"/> </param>
        public void Load (UnityReader reader);

    }

}