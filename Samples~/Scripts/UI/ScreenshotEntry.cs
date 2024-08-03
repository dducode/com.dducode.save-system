using SaveSystemPackage;
using UnityEngine;
using UnityEngine.UI;


public class ScreenshotEntry : MonoBehaviour {

    [SerializeField]
    private Image mainView;

    [SerializeField]
    private Text screenshotName;

    private FullRectWindow m_fullRectWindow;


    private void Start () {
        transform.localScale = Vector3.one;
    }


    public void SetScreenshot (Texture2D screenshot) {
        var rect = new Rect(0, 0, screenshot.width, screenshot.height);
        var pivot = new Vector2(rect.width / 2, rect.height / 2);
        mainView.sprite = Sprite.Create(screenshot, rect, pivot, 100, 0, SpriteMeshType.FullRect);
        screenshotName.text = screenshot.name;
    }


    public void SetFullRectWindow (FullRectWindow fullRectWindow) {
        m_fullRectWindow = fullRectWindow;
    }


    public void OpenFullRectWindow () {
        m_fullRectWindow.Open(mainView.sprite);
    }


    public void DeleteScreenshot () {
        SaveSystem.DeleteScreenshot(mainView.mainTexture.name);
        Destroy(gameObject);
    }

}