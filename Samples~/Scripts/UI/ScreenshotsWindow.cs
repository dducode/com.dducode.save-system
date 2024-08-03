using SaveSystemPackage;
using UnityEngine;


public class ScreenshotsWindow : Window {

    [SerializeField]
    private ScreenshotEntry entryPrefab;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private FullRectWindow fullRectWindow;


    private void Awake () {
        LoadScreenshots();
        SaveSystem.OnScreenCaptured += CreateScreenshotEntry;
    }


    private void OnDestroy () {
        SaveSystem.OnScreenCaptured -= CreateScreenshotEntry;
    }


    public override void Refresh () {
        ClearScreenshotEntries();
        LoadScreenshots();
    }


    public void ClearScreenshotsFolder () {
        SaveSystem.ClearScreenshotsFolder();
        ClearScreenshotEntries();
    }


    private void ClearScreenshotEntries () {
        for (var i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);
    }


    private async void LoadScreenshots () {
        await foreach (Texture2D screenshot in SaveSystem.LoadScreenshots())
            CreateScreenshotEntry(screenshot);
    }


    private void CreateScreenshotEntry (Texture2D screenshot) {
        ScreenshotEntry screenshotEntry = Instantiate(entryPrefab, content, true);
        screenshotEntry.SetScreenshot(screenshot);
        screenshotEntry.SetFullRectWindow(fullRectWindow);
    }

}