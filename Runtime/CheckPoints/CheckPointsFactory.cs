using UnityEngine;

namespace SaveSystem.CheckPoints {

    /// <summary>
    /// You can create checkpoints at runtime by using this class
    /// </summary>
    public static class CheckPointsFactory {

        /// <summary>
        /// Creates a 3D checkpoint with sphere collider
        /// </summary>
        /// <param name="position"> 3D Position at which the checkpoint will be created </param>
        /// <param name="radius"> Radius of trigger </param>
        public static CheckPoint CreateCheckPoint (Vector3 position, float radius = 0.5f) {
            var checkPoint = new GameObject("Check Point") {
                transform = {
                    localPosition = position
                }
            };
            var script = checkPoint.AddComponent<CheckPoint>();
            var trigger = checkPoint.GetComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;

            return script;
        }


        /// <summary>
        /// Creates a 2D checkpoint with circle collider 2D
        /// </summary>
        /// <param name="position"> 2D Position at which the checkpoint will be created </param>
        /// <param name="radius"> Radius of trigger </param>
        public static CheckPoint2D CreateCheckPoint2D (Vector2 position, float radius = 0.5f) {
            var checkPoint2D = new GameObject("Check Point 2D") {
                transform = {
                    localPosition = position
                }
            };
            var script = checkPoint2D.AddComponent<CheckPoint2D>();
            var trigger = checkPoint2D.GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = radius;

            return script;
        }

    }

}