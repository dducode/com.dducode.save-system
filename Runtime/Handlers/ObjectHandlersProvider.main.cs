using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using SaveSystem.Core;
using SaveSystem.InternalServices.Diagnostic;
using UnityEngine;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Use it to create <see cref="ObjectHandler"/> and <see cref="AdvancedObjectHandler"/>
    /// </summary>
    public static partial class ObjectHandlersFactory {

        /// <summary>
        /// Configure it to automatically register handlers in the Save System Core
        /// </summary>
        /// <value>
        /// If true, the Handlers Provider will register all created handlers in the Core,
        /// otherwise you will have to do it manually (it's false by default)
        /// </value>
        public static bool RegisterImmediately { get; set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize () {
            var settings = Resources.Load<SaveSystemSettings>(nameof(SaveSystemSettings));

            if (settings == null) {
                throw new ArgumentNullException(
                    "", "Save system settings not found. Did you delete, rename or transfer them?"
                );
            }

            RegisterImmediately = settings.registerImmediately;
        }


        /// <summary>
        /// Creates an object handler that will saving a single object
        /// </summary>
        /// <param name="obj"> Object which will be saved </param>
        /// <param name="filePath"> Path to save object </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static ObjectHandler Create (
            [NotNull] IPersistentObject obj,
            [NotNull] string filePath,
            [CallerMemberName] string caller = ""
        ) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            AddMetadata(new HandlerMetadata(filePath, caller, obj.GetType(), 1));
            var objectHandler = new ObjectHandler(filePath, new[] {obj});
            if (RegisterImmediately)
                SaveSystemCore.RegisterObjectHandler(objectHandler);
            return objectHandler;
        }


        /// <summary>
        /// Creates an object handler that will saving some objects
        /// </summary>
        /// <param name="objects"> Objects which will be saved </param>
        /// <param name="filePath"> Path to save objects </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static ObjectHandler Create (
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
            var objectHandler = new ObjectHandler(filePath, objectsArray);
            if (RegisterImmediately)
                SaveSystemCore.RegisterObjectHandler(objectHandler);
            return objectHandler;
        }


        /// <summary>
        /// Creates an object handler that will saving a single object async
        /// </summary>
        /// <param name="obj"> Object which will be saved </param>
        /// <param name="filePath"> Path to save objects </param>
        /// <param name="caller"> A method where the object handler was created </param>
        public static AdvancedObjectHandler Create (
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
        public static AdvancedObjectHandler Create (
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