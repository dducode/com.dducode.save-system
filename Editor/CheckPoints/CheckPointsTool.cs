using System;
using SaveSystem.CheckPoints;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaveSystem.Editor.CheckPoints {

    [EditorTool("Check Points Tool")]
    public class CheckPointsTool : EditorTool {

        private const string hideInHierarchyKey = "hide_in_hierarchy";
        private const string collidersColorKey = "colliders_color";

        private const string createPointShortcutId =
            nameof(SaveSystem) + "." + nameof(CheckPointsTool) + "." + nameof(CreateCheckPointBase);

        private const string deletePointShortcutId =
            nameof(SaveSystem) + "." + nameof(CheckPointsTool) + "." + nameof(DeleteAllCheckPoints);

        private const string setActiveShortcutId =
            nameof(SaveSystem) + "." + nameof(CheckPointsTool) + "." + nameof(SetActive);

        public override GUIContent toolbarIcon => m_icon;

        private GUIContent m_icon;
        private bool m_hideInHierarchy;
        private Color m_collidersColor;


        [Shortcut(setActiveShortcutId, KeyCode.C)]
        private static void SetActive () {
            ToolManager.SetActiveTool<CheckPointsTool>();
        }


        [Shortcut(createPointShortcutId, KeyCode.S, ShortcutModifiers.Control | ShortcutModifiers.Alt)]
        private static void CreateCheckPoint () {
            CreateCheckPointBase();
        }


        [Shortcut(deletePointShortcutId, KeyCode.Delete, ShortcutModifiers.Control)]
        private static void DeleteAllCheckPoints () {
            bool destroyCheckPoints = EditorUtility.DisplayDialog(
                "Clear All Check Points",
                "Are you sure to destroy all check points?",
                "Yes",
                "No");
            if (!destroyCheckPoints)
                return;

            CheckPointBase[] checkPoints = FindObjectsByType<CheckPointBase>(FindObjectsSortMode.None);

            foreach (CheckPointBase checkPoint in checkPoints)
                Undo.DestroyObjectImmediate(checkPoint.gameObject);
        }


        private void OnEnable () {
            m_hideInHierarchy = EditorPrefs.GetBool(hideInHierarchyKey, false);
            m_collidersColor = SaveSystemPrefs.GetColor(collidersColorKey, Color.green);
            HandleAllPointsInHierarchy(m_hideInHierarchy);

            m_icon = new GUIContent {
                image = EditorIconsTool.GetCheckPointsManagerIcon(),
                text = "Check Points Manager",
                tooltip = "Check Points Creator"
            };
        }


        public override void OnToolGUI (EditorWindow window) {
            if (window is not SceneView)
                return;

            CheckPointBase[] checkPoints = FindObjectsByType<CheckPointBase>(FindObjectsSortMode.None);

            foreach (CheckPointBase checkPointBase in checkPoints) {
                Vector3 position = AdjustPosition(checkPointBase);
                float radius = AdjustColliderRadius(checkPointBase);
                Handles.Label(position + Vector3.up,
                    $"\t{checkPointBase.name}" +
                    $"\n\t{position}" +
                    $"\n\tradius: {Math.Round(radius, 3)}"
                );
            }

            OnGUI();
        }


        private Vector3 AdjustPosition (Component checkPoint) {
            Transform transform = checkPoint.transform;
            Undo.RecordObject(transform, transform.name);

            Vector3 position = transform.position;
            position = Handles.PositionHandle(position, Quaternion.identity);
            transform.position = position;
            return position;
        }


        private float AdjustColliderRadius (CheckPointBase checkPointBase) {
            Color tempColor = Handles.color;
            Handles.color = m_collidersColor;

            switch (checkPointBase) {
                case CheckPoint checkPoint: {
                    var sphereCollider = checkPoint.GetComponent<SphereCollider>();
                    Undo.RecordObject(sphereCollider, sphereCollider.name);
                    sphereCollider.radius = Handles.RadiusHandle(
                        Quaternion.identity, checkPointBase.transform.position, sphereCollider.radius
                    );
                    Handles.color = tempColor;

                    return sphereCollider.radius;
                }
                case CheckPoint2D checkPoint2D: {
                    var circleCollider = checkPoint2D.GetComponent<CircleCollider2D>();
                    Undo.RecordObject(circleCollider, circleCollider.name);
                    circleCollider.radius = Handles.RadiusHandle(
                        Quaternion.identity, checkPointBase.transform.position, circleCollider.radius
                    );
                    Handles.color = tempColor;

                    return circleCollider.radius;
                }
                default:
                    return 0;
            }
        }


        private void OnGUI () {
            Handles.BeginGUI();

            var rect = new Rect(50, 2, 150, 25);

            DrawCreateCheckPointButton(ref rect);

            var hasAnyPoint = FindAnyObjectByType<CheckPointBase>();
            if (hasAnyPoint)
                DrawClearAllButton(ref rect);

            if (hasAnyPoint)
                DrawCollidersColorField(ref rect);

            if (hasAnyPoint)
                DrawHideInHierarchyToggle(ref rect);

            Handles.EndGUI();
        }


        private void DrawCreateCheckPointButton (ref Rect rect) {
            if (GUI.Button(rect, "Create Check Point")) {
                CheckPointBase checkPoint = CreateCheckPointBase();
                HandleCheckPointInHierarchy(m_hideInHierarchy, checkPoint);
            }

            rect.x += 155;
        }


        private void DrawClearAllButton (ref Rect rect) {
            if (GUI.Button(rect, "Clear All"))
                DeleteAllCheckPoints();

            rect.x += 155;
        }


        private void DrawCollidersColorField (ref Rect rect) {
            rect.x = 50;
            rect.width = 200;
            rect.height = 20;
            rect.y += 30;

            EditorGUI.BeginChangeCheck();
            m_collidersColor = EditorGUI.ColorField(rect, "Colliders Color", m_collidersColor);

            if (EditorGUI.EndChangeCheck())
                SaveSystemPrefs.SetColor(collidersColorKey, m_collidersColor);
        }


        private void DrawHideInHierarchyToggle (ref Rect rect) {
            rect.x = 50;
            rect.width = 150;
            rect.y += 25;

            EditorGUI.BeginChangeCheck();
            m_hideInHierarchy = EditorGUI.ToggleLeft(rect, "Hide In Hierarchy", m_hideInHierarchy);

            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetBool(hideInHierarchyKey, m_hideInHierarchy);
                HandleAllPointsInHierarchy(m_hideInHierarchy);
            }
        }


        private void HandleAllPointsInHierarchy (bool hideInHierarchy) {
            foreach (CheckPointBase checkPoint in FindObjectsByType<CheckPointBase>(FindObjectsSortMode.None))
                HandleCheckPointInHierarchy(hideInHierarchy, checkPoint);
        }


        private void HandleCheckPointInHierarchy (bool hideInHierarchy, Component checkPoint) {
            checkPoint.gameObject.hideFlags = hideInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
        }


        private static CheckPointBase CreateCheckPointBase () {
            Camera camera = SceneView.currentDrawingSceneView.camera;
            Vector3 randomPosition = Random.insideUnitSphere * 2.5f;

            switch (EditorSettings.defaultBehaviorMode) {
                case EditorBehaviorMode.Mode3D:
                    randomPosition.y = 0;
                    randomPosition = PositionRelativeToCamera(randomPosition, camera);
                    return CheckPointsCreatorEditor.CreateCheckPoint(randomPosition);
                case EditorBehaviorMode.Mode2D:
                    randomPosition += camera.transform.position;
                    randomPosition.z = 0;
                    return CheckPointsCreatorEditor.CreateCheckPoint2D(randomPosition);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static Vector3 PositionRelativeToCamera (Vector3 position, Component camera) {
            /*
             * Imagine a triangle which consists of three sides -
             * the down vector, the forward vector of the camera and the third side which connects two others sides.
             * The forward vector is a hypotenuse of the triangle and the down vector is a adjacent cathetus.
             * The third side lies on the XZ-plane.
             * We need to find hypotenuse
             */

            position += camera.transform.position;

            float corner = Vector3.Dot(Vector3.down, camera.transform.forward);

            // Set relative position only when the corner is valid
            if (corner > 0.01f) {
                float y = camera.transform.position.y;
                Vector3 hypotenuse = camera.transform.forward * y / corner;
                position += hypotenuse;
            }
            else {
                position.y = 0;
            }

            return position;
        }

    }

}