using SaveSystem.CheckPoints;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.CheckPoints {

    internal static class CheckPointsCreatorEditor {

        internal static CheckPoint CreateCheckPoint (Vector3 position) {
            CheckPoint checkPoint = CheckPointsCreator.CreateCheckPoint(position);
            Undo.RegisterCreatedObjectUndo(checkPoint.gameObject, checkPoint.gameObject.name);

            return checkPoint;
        }


        internal static CheckPoint2D CreateCheckPoint2D (Vector2 position) {
            CheckPoint2D checkPoint = CheckPointsCreator.CreateCheckPoint2D(position);
            Undo.RegisterCreatedObjectUndo(checkPoint.gameObject, checkPoint.gameObject.name);

            return checkPoint;
        }

    }

}