using System;
using UnityEngine.LowLevel;

namespace SaveSystem.Internal {

    internal static class PlayerLoopManager {

        internal static bool TryInsertSubSystem (
            ref PlayerLoopSystem currentPlayerLoop,
            PlayerLoopSystem insertedSubSystem,
            Type targetLoopType
        ) {
            if (currentPlayerLoop.type == targetLoopType) {
                InsertSubSystemAtLast(ref currentPlayerLoop, insertedSubSystem);
                return true;
            }

            if (currentPlayerLoop.subSystemList != null)
                for (var i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
                    if (TryInsertSubSystem(ref currentPlayerLoop.subSystemList[i], insertedSubSystem, targetLoopType))
                        return true;

            return false;
        }


        internal static bool TryRemoveSubSystem (
            ref PlayerLoopSystem currentPlayerLoop,
            Type removedSubSystemType,
            Type targetLoopType
        ) {
            if (currentPlayerLoop.type == targetLoopType) {
                RemoveSubSystem(ref currentPlayerLoop, removedSubSystemType);
                return true;
            }

            if (currentPlayerLoop.subSystemList != null)
                for (var i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
                    if (TryRemoveSubSystem(ref currentPlayerLoop.subSystemList[i], removedSubSystemType,
                        targetLoopType))
                        return true;

            return false;
        }


        private static void InsertSubSystemAtLast (ref PlayerLoopSystem currentPlayerLoop,
            PlayerLoopSystem insertedSubSystem
        ) {
            var newSubSystems = new PlayerLoopSystem[currentPlayerLoop.subSystemList.Length + 1];

            for (var i = 0; i < currentPlayerLoop.subSystemList.Length; i++)
                newSubSystems[i] = currentPlayerLoop.subSystemList[i];

            newSubSystems[^1] = insertedSubSystem;
            currentPlayerLoop.subSystemList = newSubSystems;
        }


        private static void RemoveSubSystem (ref PlayerLoopSystem currentPlayerLoop, Type removedSubSystemType) {
            var newSubSystems = new PlayerLoopSystem[currentPlayerLoop.subSystemList.Length - 1];

            var j = 0;

            foreach (PlayerLoopSystem subSystem in currentPlayerLoop.subSystemList) {
                if (subSystem.type == removedSubSystemType)
                    continue;

                newSubSystems[j] = subSystem;
                j++;
            }

            currentPlayerLoop.subSystemList = newSubSystems;
        }

    }

}