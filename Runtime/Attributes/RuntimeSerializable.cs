using System;

namespace SaveSystemPackage.Attributes {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RuntimeSerializableAttribute : Attribute { }

}