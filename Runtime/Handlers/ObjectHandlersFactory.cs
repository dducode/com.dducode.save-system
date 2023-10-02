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
        /// Creates an object handler that will saving a single object
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
        /// Creates an object handler that will saving some objects
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
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="objects"></param>
        /// <param name="factoryFunc"></param>
        /// <param name="caller"></param>
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
        /// Creates an object handler that will saving a single object async
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="obj"> Objects which will be saved </param>
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
        /// Creates an object handler that will saving some objects async
        /// </summary>
        /// <param name="filePath"> Path to save and load objects </param>
        /// <param name="objects"> Objects which will be saved </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncObjectHandler<TO> CreateAsyncHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            return CreateAsyncHandler(filePath, objects, null, caller);
        }


        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="objects"></param>
        /// <param name="factoryFunc"></param>
        /// <param name="caller"></param>
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
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="factoryFunc"></param>
        /// <param name="caller"></param>
        public static BufferableObjectHandler<TO> CreateBufferableHandler<TO> (
            [NotNull] string filePath,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IBufferableObject {
            return CreateBufferableHandler(filePath, Array.Empty<TO>(), factoryFunc, caller);
        }


        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        /// <param name="caller"></param>
        public static BufferableObjectHandler<TO> CreateBufferableHandler<TO> (
            [NotNull] string filePath,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IBufferableObject {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return CreateBufferableHandler(filePath, new[] {obj}, null, caller);
        }


        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="objects"></param>
        /// <param name="caller"></param>
        public static BufferableObjectHandler<TO> CreateBufferableHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IBufferableObject {
            return CreateBufferableHandler(filePath, objects, null, caller);
        }


        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="objects"></param>
        /// <param name="factoryFunc"></param>
        /// <param name="caller"></param>
        public static BufferableObjectHandler<TO> CreateBufferableHandler<TO> (
            [NotNull] string filePath,
            [NotNull] ICollection<TO> objects,
            Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IBufferableObject {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (objects == null) throw new ArgumentNullException(nameof(objects));

            var handler = new BufferableObjectHandler<TO>(filePath, objects.ToArray(), factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(BufferableObjectHandler<TO>), typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }

    }

}