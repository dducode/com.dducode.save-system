using System;
using System.Collections.Generic;
using System.Reflection;

namespace SaveSystemPackage.Internal.Extensions {

    internal static class TypeExtensions {

        internal static bool IsUnmanaged (this Type type, HashSet<Type> checkedTypes = null) {
            checkedTypes ??= new HashSet<Type>();
            if (!checkedTypes.Add(type))
                return true;

            if (type.IsPrimitive || type.IsPointer || type.IsEnum)
                return true;

            if (type.IsValueType) {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                foreach (FieldInfo field in fields)
                    if (!field.FieldType.IsUnmanaged(checkedTypes))
                        return false;

                return true;
            }

            return false;
        }

    }

}