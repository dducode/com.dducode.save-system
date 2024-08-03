using UnityEngine;
using UnityEngine.UI;


public class FullRectWindow : Window {

    [SerializeField]
    private Image mainView;

    [SerializeField]
    private Image frame;


    public void Open (Sprite screenshot) {
        base.Open();
        mainView.sprite = screenshot;
        // mainView.SetNativeSize();
        // Vector2 size = mainView.rectTransform.sizeDelta;
        // size = new Vector2(size.x / 1.5f, size.y / 1.5f);
        // mainView.rectTransform.sizeDelta = size;
        // frame.rectTransform.sizeDelta = new Vector2(size.x + 25, size.y + 25);
    }

}