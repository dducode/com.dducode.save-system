using System;
using SaveSystemPackage;
using SaveSystemPackage.Exceptions;
using UnityEngine;
using UnityEngine.UI;


public class ProfileEntry : MonoBehaviour {

    [SerializeField]
    private Image icon;

    [SerializeField]
    private InputField profileName;

    public event Action<ProfileEntry> OnDelete;
    public event Action<string> OnFailRename;
    public SaveProfile Profile { get; private set; }

    private ProfilesWindow m_userInterface;


    private void Start () {
        transform.localScale = Vector3.one;
    }


    public void SetInterface (ProfilesWindow userInterface) {
        m_userInterface = userInterface;
    }


    public void SetProfile (SaveProfileSample profileSample) {
        Profile = profileSample;
        icon.sprite = Resources.Load<Sprite>($"UserInterface/{profileSample.iconKey}");
        profileName.onSubmit.AddListener(RenameProfile);
        profileName.text = profileSample.Name;
    }


    public void SelectProfile () {
        m_userInterface.LoadProfile(Profile);
    }


    public void DeleteProfile () {
        OnDelete?.Invoke(this);
    }


    private void RenameProfile (string newName) {
        try {
            Profile.Name = newName;
        }
        catch (ProfileExistsException ex) {
            profileName.text = Profile.Name;
            OnFailRename?.Invoke(ex.Message);
        }
    }

}