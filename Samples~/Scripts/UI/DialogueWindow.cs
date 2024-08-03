using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class DialogueWindow : Window {

    [SerializeField]
    private Text title;

    [SerializeField]
    private Text description;

    [SerializeField]
    private Text okayButton;

    [SerializeField]
    private Text cancelButton;

    private UniTaskCompletionSource<bool> m_tcs;


    public async UniTask<bool> Open (
        string title, string description, string okayButton = "Ok", string cancelButton = "Cancel"
    ) {
        base.Open();
        m_tcs = new UniTaskCompletionSource<bool>();
        this.title.text = title;
        this.description.text = description;
        this.okayButton.text = okayButton;
        this.cancelButton.text = cancelButton;
        return await m_tcs.Task;
    }


    public void Okay () {
        base.Close();
        m_tcs.TrySetResult(true);
    }


    public void Cancel () {
        base.Close();
        m_tcs.TrySetResult(false);
    }

}