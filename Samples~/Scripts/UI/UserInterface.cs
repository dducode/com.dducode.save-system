using SaveSystemPackage;
using UnityEngine;


public class UserInterface : MonoBehaviour {

    [SerializeField]
    private ProfilesWindow profilesWindow;

    [SerializeField]
    private ScreenshotsWindow screenshotsWindow;


    private void Awake () {
        profilesWindow.Open();
        screenshotsWindow.Close();
    }


    public void OpenProfilesWindow () {
        profilesWindow.Open();
        screenshotsWindow.Close();
    }


    public void OpenScreenshotsWindow () {
        profilesWindow.Close();
        screenshotsWindow.Open();
    }


    public async void RestoreData () {
        await SaveSystem.DownloadFromCloud();
        await SaveSystem.Game.Load();
        profilesWindow.Refresh();
        screenshotsWindow.Refresh();
    }

}