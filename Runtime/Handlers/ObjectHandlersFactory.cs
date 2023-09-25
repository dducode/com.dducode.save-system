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
    /// Use it to create <see cref="ObjectHandler{TO}">Object Handlers</see>,
    /// <see cref="AsyncObjectHandler{TO}">Async Object Handlers</see>
    /// and <see cref="RemoteHandler{TO}">Remote Handlers</see>
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
            [NotNull] Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            var objectHandler = new ObjectHandler<TO>(filePath, factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(filePath, caller, typeof(TO), 0));
            if (RegisterImmediately)
                SaveSystemCore.RegisterObjectHandler(objectHandler);
            return objectHandler;
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
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var objectHandler = new ObjectHandler<TO>(filePath, new[] {obj}) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(filePath, caller, typeof(TO), 1));
            if (RegisterImmediately)
                SaveSystemCore.RegisterObjectHandler(objectHandler);
            return objectHandler;
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
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

            var objectHandler = new ObjectHandler<TO>(filePath, objects.ToArray()) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(TO), objects.Count)
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
            [NotNull] Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            var handler = new AsyncObjectHandler<TO>(filePath, factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(filePath, caller, typeof(TO), 0));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
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
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var handler = new AsyncObjectHandler<TO>(filePath, new[] {obj}) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(filePath, caller, typeof(TO), 1));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
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
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

            var handler = new AsyncObjectHandler<TO>(filePath, objects.ToArray()) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(filePath, caller, typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an empty remote handler
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static RemoteHandler<TO> CreateRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            var handler = new RemoteHandler<TO>(url, factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(url, caller, typeof(TO), 0));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an object handler that will saving single object at remote storage
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="obj"> Objects that will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static RemoteHandler<TO> CreateRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var handler = new RemoteHandler<TO>(url, new[] {obj}) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(url, caller, typeof(TO), 1));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an object handler that will saving some objects at remote storage
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="objects"> Objects that will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static RemoteHandler<TO> CreateRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObject {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

            var handler = new RemoteHandler<TO>(url, objects.ToArray()) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(url, caller, typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an empty async remote handler
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="factoryFunc"> Function for objects spawn. This is necessary to load dynamic objects </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncRemoteHandler<TO> CreateAsyncRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] Func<TO> factoryFunc,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (factoryFunc == null)
                throw new ArgumentNullException(nameof(factoryFunc));

            var handler = new AsyncRemoteHandler<TO>(url, factoryFunc) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(url, caller, typeof(TO), 0));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an async object handler that will saving single object at remote storage
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="obj"> Objects that will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncRemoteHandler<TO> CreateAsyncRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] TO obj,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var handler = new AsyncRemoteHandler<TO>(url, new[] {obj}) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(new HandlerMetadata(url, caller, typeof(TO), 1));
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }


        /// <summary>
        /// Creates an async object handler that will saving some objects at remote storage
        /// </summary>
        /// <param name="url"> Link to a remote storage </param>
        /// <param name="objects"> Objects that will be saved and loaded </param>
        /// <param name="caller"> For internal use (no need to pass it manually) </param>
        public static AsyncRemoteHandler<TO> CreateAsyncRemoteHandler<TO> (
            [NotNull] string url,
            [NotNull] ICollection<TO> objects,
            [CallerMemberName] string caller = ""
        ) where TO : IPersistentObjectAsync {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (objects.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));

            var handler = new AsyncRemoteHandler<TO>(url, objects.ToArray()) {
                diagnosticIndex = DiagnosticService.HandlersData.Count
            };
            DiagnosticService.AddMetadata(
                new HandlerMetadata(url, caller, typeof(TO), objects.Count)
            );
            if (RegisterImmediately)
                SaveSystemCore.RegisterAsyncObjectHandler(handler);
            return handler;
        }

    }

}