using SaveSystemPackage.CheckPoints;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.Editor.CheckPoints {

    internal static class CheckPointsFactoryEditor {

        internal static CheckPoint CreateCheckPoint (Vector3 position) {
            CheckPoint checkPoint = CheckPointsFactory.CreateCheckPoint(position);
            Undo.RegisterCreatedObjectUndo(checkPoint.gameObject, checkPoint.gameObject.name);

            return checkPoint;
        }


        internal static CheckPoint2D CreateCheckPoint2D (Vector2 position) {
            CheckPoint2D checkPoint = CheckPointsFactory.CreateCheckPoint2D(position);
            Undo.RegisterCreatedObjectUndo(checkPoint.gameObject, checkPoint.gameObject.name);

            return checkPoint;
        }

    }

}