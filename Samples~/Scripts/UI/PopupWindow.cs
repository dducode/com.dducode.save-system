using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class PopupWindow : Window {

    [SerializeField]
    private Text title;

    [SerializeField]
    private Text description;

    [SerializeField]
    private Text okayButton;

    private UniTaskCompletionSource m_tcs;


    public async UniTask Open (string title, string description, string okayButton = "Ok") {
        base.Open();
        m_tcs = new UniTaskCompletionSource();
        this.title.text = title;
        this.description.text = description;
        this.okayButton.text = okayButton;
        await m_tcs.Task;
    }


    public override void Close () {
        base.Close();
        m_tcs.TrySetResult();
    }

}