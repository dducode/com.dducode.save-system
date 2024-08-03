using System;
using Cysharp.Threading.Tasks;
using SaveSystemPackage;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour {

    private const string LastOpenedScene = "lastOpenedScene";
    private const string PlayTimeKey = "play-time";

    private SaveProfile m_profile;
    private float m_playTime;


    private async void Awake () {
        DontDestroyOnLoad(gameObject);
        SaveSystem.CloudStorage = new LocalStorage();
        await SaveSystem.Game.Load();
        m_playTime = SaveSystem.Game.Data.Read<float>(PlayTimeKey);
        SceneManager.LoadScene(1);
    }


    public async void LoadProfile (SaveProfile profile) {
        m_profile = profile;
        await m_profile.Load();
        SaveSystem.Game.SaveProfile = m_profile;
        int lastOpenedScene = m_profile.Data.Read(LastOpenedScene, 2);
        await LoadLastOpenedScene(lastOpenedScene);
    }


    private async void Update () {
        m_playTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log($"Play Time: {new TimeSpan(0, 0, (int)m_playTime)}");

        if (Input.GetKeyDown(KeyCode.Alpha1))
            await LoadFirstGameSceneAsync();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            await LoadSecondGameSceneAsync();

        if (Input.GetKeyDown(KeyCode.Alpha9))
            await SaveSystem.UploadToCloud();

        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
    }


    private async void GoBack () {
        switch (SceneManager.GetActiveScene().buildIndex) {
            case 1:
                SaveSystem.Game.Data.Write(PlayTimeKey, m_playTime);
                await SaveSystem.ExitGame();
                break;
            default:
                await SaveSystem.LoadSceneAsync(async () => await SceneManager.LoadSceneAsync(1));
                break;
        }
    }


    private async UniTask LoadFirstGameSceneAsync () {
        m_profile.Data.Write(LastOpenedScene, 2);
        await SaveSystem.LoadSceneAsync(
            async () => await SceneManager.LoadSceneAsync(2),
            new SceneData("Start first scene")
        );
    }


    private async UniTask LoadSecondGameSceneAsync () {
        m_profile.Data.Write(LastOpenedScene, 3);
        await SaveSystem.LoadSceneAsync(
            async () => await SceneManager.LoadSceneAsync(3),
            new SceneData("Start second scene")
        );
    }


    private async UniTask LoadLastOpenedScene (int lastOpenedScene) {
        await SaveSystem.LoadSceneAsync(
            async () => await SceneManager.LoadSceneAsync(lastOpenedScene),
            new SceneData("Start last opened scene")
        );
    }

}