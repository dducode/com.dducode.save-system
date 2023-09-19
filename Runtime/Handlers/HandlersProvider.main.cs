using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using SaveSystem.InternalServices.Diagnostic;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Use it to create <see cref="ObjectHandler"/> and <see cref="AdvancedObjectHandler"/>
    /// </summary>
    public static partial class HandlersProvider {

        /// <summary>
        /// Creates an object handler that will saving a single object
        /// </summary>
        /// <param name="obj"> Object which will be saved </param>
        /// <param name="filePath"> Path to save object </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static ObjectHandler CreateObjectHandler (
            [NotNull] IPersistentObject obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AddMetadata(new HandlerMetadata(filePath, caller, obj.GetType(), 1));
            return new ObjectHandler(filePath, new[] {obj});
        }


        /// <summary>
        /// Creates an object handler that will saving some objects
        /// </summary>
        /// <param name="objects"> Objects which will be saved </param>
        /// <param name="filePath"> Path to save objects </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static ObjectHandler CreateObjectHandler (
            [NotNull] IEnumerable<IPersistentObject> objects,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            IPersistentObject[] objectsArray = objects.ToArray();
            if (objectsArray.Length == 0)
                throw new ArgumentException("Objects array cannot be an empty", nameof(objects));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AddMetadata(new HandlerMetadata(filePath, caller, objectsArray[0].GetType(), objectsArray.Length));
            return new ObjectHandler(filePath, objectsArray);
        }


        /// <summary>
        /// Creates an object handler that will saving a single object async
        /// </summary>
        /// <param name="obj"> Object which will be saved </param>
        /// <param name="filePath"> Path to save objects </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static AdvancedObjectHandler CreateObjectHandler (
            [NotNull] IPersistentObjectAsync obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AddMetadata(new HandlerMetadata(filePath, caller, obj.GetType(), 1));
            return new AdvancedObjectHandler(filePath, new[] {obj});
        }


        /// <summary>
        /// Creates an object handler that will saving some objects async
        /// </summary>
        /// <param name="objects"> Objects which will be saved </param>
        /// <param name="filePath"> Path to save objects </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static AdvancedObjectHandler CreateObjectHandler (
            [NotNull] IEnumerable<IPersistentObjectAsync> objects,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            IPersistentObjectAsync[] objectsArray = objects.ToArray();
            if (objectsArray.Length == 0)
                throw new ArgumentException("Objects array cannot be an empty", nameof(objects));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AddMetadata(new HandlerMetadata(filePath, caller, objectsArray[0].GetType(), objectsArray.Length));
            return new AdvancedObjectHandler(filePath, objectsArray);
        }


        static partial void AddMetadata (HandlerMetadata metadata);

    }

}