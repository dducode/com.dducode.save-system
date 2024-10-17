using System;
using SaveSystemPackage.SerializableData;

namespace SaveSystemPackage {

    public class KeyMap : Map<Type, string> {

        internal static KeyMap PredefinedMap = new() {
            {typeof(ColorData), "color-data"},
            {typeof(ProfileData), "profile-data"},
            {typeof(ProfilesManagerData), "profiles-manager-data"},
            {typeof(QuaternionData), "quaternion-data"},
            {typeof(RigidbodyData), "rigidbody-data"},
            {typeof(TransformData), "transform-data"},
            {typeof(Vector2Data), "vector2-data"},
            {typeof(Vector3Data), "vector3-data"},
            {typeof(Vector4Data), "vector4-data"},
            {typeof(DataBatch<ColorData>), "color-data-batch"},
            {typeof(DataBatch<QuaternionData>), "quaternion-data-batch"},
            {typeof(DataBatch<RigidbodyData>), "rigidbody-data-batch"},
            {typeof(DataBatch<TransformData>), "transform-data-batch"},
            {typeof(DataBatch<Vector2Data>), "vector2-data-batch"},
            {typeof(DataBatch<Vector3Data>), "vector3-data-batch"},
            {typeof(DataBatch<Vector4Data>), "vector4-data-batch"}
        };


        public void Concat (KeyMap otherMap) {
            foreach ((Type key, string value) in otherMap)
                TryAdd(key, value);
        }


        public void AddKey<TType> (string key) {
            TryAdd(typeof(TType), key);
        }

    }

}