using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture(superSize);
            screenshot.name = SaveScreenshot(screenshot, filename);
            OnScreenCaptured?.Invoke(screenshot);
            Logger.Log(nameof(SaveSystem), "Capture screenshot");
        }


        public static string SaveScreenshot (Texture2D screenshot, [NotNull] string filename = "screenshot") {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));

            File screenshotFile = Storage.ScreenshotsDirectory.CreateFile(filename, "png");

            s_synchronizationPoint.ScheduleTask(async token => {
                var rawData = new NativeArray<byte>(screenshot.GetRawTextureData(), Allocator.Persistent);
                GraphicsFormat graphicsFormat = screenshot.graphicsFormat;
                var width = (uint)screenshot.width;
                var height = (uint)screenshot.height;

                await Task.Run(async () => {
                    NativeArray<byte> data = ImageConversion.EncodeNativeArrayToPNG(
                        rawData, graphicsFormat, width, height
                    );
                    await screenshotFile.WriteAllBytesAsync(data.ToArray(), token);
                    rawData.Dispose();
                    data.Dispose();
                }, token);
                Logger.Log(nameof(SaveSystem), $"Screenshot \"{screenshotFile.Name}\" saved");
            });

            return screenshotFile.Name;
        }


        public static async IAsyncEnumerable<Texture2D> LoadScreenshots () {
            if (!Storage.ScreenshotsDirectoryExists())
                yield break;

            foreach (File file in Storage.ScreenshotsDirectory.EnumerateFiles("png")) {
                byte[] data;

                try {
                    data = await file.ReadAllBytesAsync(s_exitCancellation.Token);
                }
                catch (OperationCanceledException) {
                    Logger.LogWarning(nameof(SaveSystem), "Screenshots loading canceled");
                    yield break;
                }

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