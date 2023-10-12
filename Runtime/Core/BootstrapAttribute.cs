using System;
using JetBrains.Annotations;

namespace SaveSystem.Core {

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class BootstrapAttribute : Attribute { }

}