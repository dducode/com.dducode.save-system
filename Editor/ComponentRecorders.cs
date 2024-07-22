using SaveSystemPackage.ComponentsRecording;
using UnityEditor;
using UnityEngine;

namespace SaveSystemPackage.Editor {

    public static class ComponentRecorders {

        [MenuItem("CONTEXT/Transform/Record at runtime")]
        private static void AddTransformSerialization (MenuCommand command) {
            var transform = (Transform)command.context;
            transform.gameObject.AddComponent<TransformRecorder>();
        }


        [MenuItem("CONTEXT/Rigidbody/Record at runtime")]
        private static void AddRigidbodySerialization (MenuCommand command) {
            var rigidbody = (Rigidbody)command.context;
            rigidbody.gameObject.AddComponent<RigidbodyRecorder>();
        }


        [MenuItem("CONTEXT/MeshFilter/Record at runtime")]
        private static void AddMeshFilterSerialization (MenuCommand command) {
            var meshFilter = (MeshFilter)command.context;
            meshFilter.gameObject.AddComponent<MeshFilterRecorder>();
        }


        [MenuItem("CONTEXT/Transform/Record at runtime", true)]
        private static bool AddTransformSerializationValidate (MenuCommand command) {
            return !((Transform)command.context).gameObject.TryGetComponent(out TransformRecorder _);
        }


        [MenuItem("CONTEXT/Rigidbody/Record at runtime", true)]
        private static bool AddRigidbodySerializationValidate (MenuCommand command) {
            return !((Rigidbody)command.context).gameObject.TryGetComponent(out RigidbodyRecorder _);
        }


        [MenuItem("CONTEXT/MeshFilter/Record at runtime", true)]
        private static bool AddMeshFilterSerializationValidate (MenuCommand command) {
            return !((MeshFilter)command.context).gameObject.TryGetComponent(out MeshFilterRecorder _);
        }

    }

}