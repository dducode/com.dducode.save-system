using System;

namespace SaveSystemPackage.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NonRuntimeSerializedAttribute : Attribute { }

}