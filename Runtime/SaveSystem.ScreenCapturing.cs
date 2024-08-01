using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Internal;
using UnityEngine;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        /// <summary>
        /// Event is called after screen capturing
        /// </summary>
        /// <value>
        /// Listeners will be called when the system has finished screenshot saving.
        /// Listeners must accept <see cref="Texture2D"/> texture
        /// </value>
        public static event Action<Texture2D> OnScreenCaptured;


        public static void CaptureScreenshot ([NotNull] string filename = "screenshot", int superSize = 1) {
            SaveScreenshot(ScreenCapture.CaptureScreenshotAsTexture(superSize), filename);
        }


        public static void SaveScreenshot (Texture2D screenshot, [NotNull] string filename = "screenshot") {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));

            File screenshotFile = Storage.ScreenshotsDirectory.CreateFile(filename, "png");

            SynchronizationPoint.ScheduleTask(async token => {
                await screenshotFile.WriteAllBytesAsync(screenshot.EncodeToPNG(), token);
                OnScreenCaptured?.Invoke(screenshot);
                Logger.Log(nameof(SaveSystem), "Capture screenshot");
            });
        }


        public static async IAsyncEnumerable<Texture2D> LoadScreenshots () {
            foreach (File file in Storage.ScreenshotsDirectory.EnumerateFiles("png")) {
                byte[] data = await file.ReadAllBytesAsync();
                var screenshot = new Texture2D(2, 2);
                screenshot.LoadImage(data);
                screenshot.name = file.Name;
                yield return screenshot;
            }
        }


        public static void DeleteScreenshot ([NotNull] string screenshotName) {
            if (string.IsNullOrEmpty(screenshotName))
                throw new ArgumentNullException(nameof(screenshotName));

            Directory directory = Storage.ScreenshotsDirectory;
            directory.DeleteFile(screenshotName);
            if (directory.IsEmpty)
                directory.Delete();

            Logger.Log(nameof(SaveSystem), $"The screenshot \"{screenshotName}\" deleted");
        }


        public static void ClearScreenshotsFolder () {
            Storage.ScreenshotsDirectory.Delete();
            Logger.Log(nameof(SaveSystem), "All screenshots deleted");
        }

    }

}