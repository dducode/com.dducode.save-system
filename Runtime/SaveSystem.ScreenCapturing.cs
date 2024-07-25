using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public static partial class SaveSystem {

    #if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Binds any key to screen capture
        /// </summary>
        public static KeyCode ScreenCaptureKey {
            get => m_screenCaptureKey;
            set {
                m_screenCaptureKey = value;
                PlayerPrefs.SetInt(SaveSystemConstants.ScreenCaptureKeyCode, (int)m_screenCaptureKey);
            }
        }
    #endif

    #if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Binds any input action to screen capture
        /// </summary>
        public static InputAction ScreenCaptureAction { get; set; }
    #endif

        /// <summary>
        /// Event is called after screen capturing
        /// </summary>
        /// <value>
        /// Listeners will be called when the system has finished screenshot saving.
        /// Listeners must accept <see cref="Texture2D"/> texture
        /// </value>
        public static event Action<Texture2D> OnScreenCaptured;

        private static string ScreenshotsFolder {
            get {
                if (string.IsNullOrEmpty(m_screenshotsFolder))
                    m_screenshotsFolder = Storage.PrepareBeforeUsing("screenshots");

                if (!Directory.Exists(m_screenshotsFolder))
                    Directory.CreateDirectory(m_screenshotsFolder);

                return m_screenshotsFolder;
            }
        }

        private static string m_screenshotsFolder;

    #if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode m_screenCaptureKey;
    #endif


        public static void CaptureScreenshot ([NotNull] string filename = "screenshot", int superSize = 1) {
            SaveScreenshot(ScreenCapture.CaptureScreenshotAsTexture(superSize), filename);
        }


        public static void SaveScreenshot (Texture2D screenshot, [NotNull] string filename = "screenshot") {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));

            string path = Path.Combine(ScreenshotsFolder, $"{filename}.png");

            if (File.Exists(path)) {
                var index = 1;
                while (File.Exists(path = Path.Combine(ScreenshotsFolder, $"{filename}_{index}.png")))
                    ++index;
            }

            SynchronizationPoint.ScheduleTask(async token => {
                await File.WriteAllBytesAsync(path, screenshot.EncodeToPNG(), token);
                OnScreenCaptured?.Invoke(screenshot);
                Logger.Log(nameof(SaveSystem), "Capture screenshot");
            });
        }


        public static async IAsyncEnumerable<Texture2D> LoadScreenshots () {
            string[] paths = Directory.GetFileSystemEntries(ScreenshotsFolder, "*.png");

            foreach (string path in paths) {
                byte[] data = await File.ReadAllBytesAsync(path);
                var screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(data);
                screenshot.name = Path.GetFileNameWithoutExtension(path);
                yield return screenshot;
            }
        }


        public static void DeleteScreenshot ([NotNull] string screenshotName) {
            if (string.IsNullOrEmpty(screenshotName))
                throw new ArgumentNullException(nameof(screenshotName));

            string path = Path.Combine(ScreenshotsFolder, $"{screenshotName}.png");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            Logger.Log(nameof(SaveSystem), $"The screenshot <b>{screenshotName}</b> deleted");
        }


        public static void ClearScreenshotsFolder () {
            Directory.Delete(ScreenshotsFolder, true);
            Logger.Log(nameof(SaveSystem), "All screenshots deleted");
        }

    }

}