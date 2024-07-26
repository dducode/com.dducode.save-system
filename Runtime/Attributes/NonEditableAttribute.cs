using System;
using UnityEngine;

namespace SaveSystemPackage.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public class NonEditableAttribute : PropertyAttribute { }

}