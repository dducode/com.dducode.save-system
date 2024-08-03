using System;
using SaveSystemPackage.ComponentsRecording;
using SaveSystemPackage.Internal.Cryptography;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystemPackage.Editor {

    [InitializeOnLoad]
    public static class ObjectChangesRecorder {

        static ObjectChangesRecorder () {
            ObjectChangeEvents.changesPublished += OnObjectsChanged;
        }


        private static void OnObjectsChanged (ref ObjectChangeEventStream stream) {
            for (var i = 0; i < stream.length; i++) {
                ObjectChangeKind type = stream.GetEventType(i);

                switch (type) {
                    case ObjectChangeKind.None:
                        break;
                    case ObjectChangeKind.ChangeScene:
                        break;
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        OnCreateGameObject(stream, i);
                        break;
                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                        break;
                    case ObjectChangeKind.ChangeGameObjectStructure:
                        break;
                    case ObjectChangeKind.ChangeGameObjectParent:
                        break;
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                        break;
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        break;
                    case ObjectChangeKind.CreateAssetObject:
                        break;
                    case ObjectChangeKind.DestroyAssetObject:
                        break;
                    case ObjectChangeKind.ChangeAssetObjectProperties:
                        break;
                    case ObjectChangeKind.UpdatePrefabInstances:
                        break;
                    case ObjectChangeKind.ChangeChildrenOrder:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private static void OnCreateGameObject (ObjectChangeEventStream stream, int i) {
            stream.GetCreateGameObjectHierarchyEvent(i, out CreateGameObjectHierarchyEventArgs eventArgs);
            var gameObject = EditorUtility.InstanceIDToObject(eventArgs.instanceId) as GameObject;

            if (gameObject != null) {
                ComponentRecorder[] recorders = gameObject.GetComponents<ComponentRecorder>();
                foreach (ComponentRecorder recorder in recorders)
                    GenerateId(recorder);
            }
        }


        private static void GenerateId (ComponentRecorder recorder) {
            var serializedObject = new SerializedObject(recorder);
            SerializedProperty id = serializedObject.FindProperty("id");

            do {
                id.stringValue = CryptoUtilities.GenerateKey(8);
                serializedObject.ApplyModifiedProperties();
            } while (!IsUniqueId(recorder));
        }


        private static bool IsUniqueId (ComponentRecorder recorder) {
            ComponentRecorder[] recorders = Object.FindObjectsByType<ComponentRecorder>(FindObjectsSortMode.None);

            foreach (ComponentRecorder other in recorders)
                if (other != recorder && string.Equals(other.Id, recorder.Id))
                    return false;

            return true;
        }

    }

}