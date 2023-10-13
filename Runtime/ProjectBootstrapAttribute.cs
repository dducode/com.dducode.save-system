using System;
using JetBrains.Annotations;

namespace SaveSystem {

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class ProjectBootstrapAttribute : Attribute { }

}