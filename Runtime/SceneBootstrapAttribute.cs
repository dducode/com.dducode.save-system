using System;
using JetBrains.Annotations;

namespace SaveSystem {

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class SceneBootstrapAttribute : Attribute {

        public readonly bool loadHandlers;
        public readonly bool invokeCallbacks;


        public SceneBootstrapAttribute (bool loadHandlers = false, bool invokeCallbacks = false) {
            this.loadHandlers = loadHandlers;
            this.invokeCallbacks = invokeCallbacks;
        }

    }

}