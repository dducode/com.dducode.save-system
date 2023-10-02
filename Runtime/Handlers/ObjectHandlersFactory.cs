using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using SaveSystem.Core;
using SaveSystem.Internal.Diagnostic;
using UnityEngine;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Use it to create <see cref="ObjectHandler{TO}">Object Handlers</see>,
    /// and <see cref="AsyncObjectHandler{TO}">Async Object Handlers</see>
    /// </summary>
    public static class ObjectHandlersFactory {

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
        /// Creates an empty handler
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static ObjectHandler<TO> CreateHandler<TO> (
            [NotNull] string filePath,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            return CreateHandler(filePath, Array.Empty<TO>(), factoryFunc, caller);
        }


        /// <summary>
        /// Creates an object handler that will saving and loading a single object
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="obj"> Object which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static ObjectHandler<TO> CreateHandler<TO> (
            [NotNull] string filePath,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return CreateHandler(filePath, new[] {obj}, null, caller);
        }


        /// <summary>
        /// Creates an object handler that will saving and loading some objects
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static ObjectHandler<TO> CreateHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            return CreateHandler(filePath, objects, null, caller);
        }


        /// <summary>
        /// Creates an object handler that will saving and loading some objects
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static ObjectHandler<TO> CreateHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            var objectHandler = new ObjectHandler<TO>(filePath, objects.ToArray(), factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(ObjectHandler<TO>), typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterObjectHandler(objectHandler);
            return objectHandler;
        }


        /// <summary>
        /// Creates an empty async handler
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncObjectHandler<TO> CreateAsyncHandler<TO> (
            [NotNull] string filePath,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            return CreateAsyncHandler(filePath, Array.Empty<TO>(), factoryFunc, caller);
        }


        /// <summary>
        /// Creates an async object handler that will saving and loading a single object async
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="obj"> Objects which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncObjectHandler<TO> CreateAsyncHandler<TO> (
            [NotNull] string filePath,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return CreateAsyncHandler(filePath, new[] {obj}, null, caller);
        }


        /// <summary>
        /// Creates an object handler that will saving and loading some objects async
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncObjectHandler<TO> CreateAsyncHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            return CreateAsyncHandler(filePath, objects, null, caller);
        }


        /// <summary>
        /// Creates an object handler that will saving and loading some objects async
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncObjectHandler<TO> CreateAsyncHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            var handler = new AsyncObjectHandler<TO>(filePath, objects.ToArray(), factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(AsyncObjectHandler<TO>), typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an empty smart handler
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static SmartHandler<TO> CreateSmartHandler<TO> (
            [NotNull] string filePath,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IStorable {
            return CreateSmartHandler(filePath, Array.Empty<TO>(), factoryFunc, caller);
        }


        /// <summary>
        /// Creates a smart handler that will saving and loading a single storable object
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="obj"> Objects which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static SmartHandler<TO> CreateSmartHandler<TO> (
            [NotNull] string filePath,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IStorable {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return CreateSmartHandler(filePath, new[] {obj}, null, caller);
        }


        /// <summary>
        /// Creates a smart handler that will saving and loading some storable objects
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static SmartHandler<TO> CreateSmartHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IStorable {
            return CreateSmartHandler(filePath, objects, null, caller);
        }


        /// <summary>
        /// Creates a smart handler that will saving and loading some storable objects
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved and loaded </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static SmartHandler<TO> CreateSmartHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IStorable {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));

            var handler = new SmartHandler<TO>(filePath, objects.ToArray(), factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(SmartHandler<TO>), typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }

    }

}