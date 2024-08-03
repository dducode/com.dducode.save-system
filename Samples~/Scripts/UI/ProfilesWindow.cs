using SaveSystemPackage;
using SaveSystemPackage.Exceptions;
using UnityEngine;
using UnityEngine.UI;


public class ProfilesWindow : Window {

    [SerializeField]
    private ProfileEntry entryPrefab;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private InputField input;

    [SerializeField]
    private PopupWindow popupWindow;

    [SerializeField]
    private DialogueWindow dialogueWindow;

    private Game m_game;


    public void Awake () {
        m_game = FindAnyObjectByType<Game>();
        input.gameObject.SetActive(false);
        LoadProfiles();
    }


    public override void Refresh () {
        ClearProfileEntries();
        LoadProfiles();
    }


    public void CreateProfile () {
        input.gameObject.SetActive(true);
    }


    public async void CreateProfile (string name) {
        input.gameObject.SetActive(false);
        input.text = string.Empty;

        try {
            var profile = SaveSystem.CreateProfile<SaveProfileSample>(name);
            profile.iconKey = "user";
            profile.ApplyChanges();
            CreateProfileEntry(profile);
        }
        catch (ProfileExistsException ex) {
            await popupWindow.Open("Create profile", ex.Message);
        }
    }


    public void LoadProfile (SaveProfile profile) {
        m_game.LoadProfile(profile);
    }


    private void ClearProfileEntries () {
        for (var i = 0; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);
    }


    private void LoadProfiles () {
        foreach (SaveProfileSample profile in SaveSystem.LoadProfiles<SaveProfileSample>())
            CreateProfileEntry(profile);
    }


    private void CreateProfileEntry (SaveProfileSample profileSample) {
        ProfileEntry profileEntry = Instantiate(entryPrefab, content, true);
        profileEntry.SetProfile(profileSample);
        profileEntry.SetInterface(this);
        profileEntry.OnDelete += OnProfileDelete;
        profileEntry.OnFailRename += OnFailProfileRename;
    }


    private async void OnProfileDelete (ProfileEntry entry) {
        var description = $"Are you sure you want delete \"{entry.Profile.Name}\" profile? It can't be undone";

        if (await dialogueWindow.Open("Delete profile", description)) {
            SaveSystem.DeleteProfile(entry.Profile);
            Destroy(entry.gameObject);
        }
    }


    private async void OnFailProfileRename (string message) {
        await popupWindow.Open("Rename profile", message);
    }

}