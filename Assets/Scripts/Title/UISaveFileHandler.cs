using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(UIMouseHoverScaler))]
public class UISaveFileHandler : MonoBehaviour, IPointerClickHandler
{

    public string SaveFileName;
    [Header("Object Assignments (within this Object)")]
    [SerializeField] private TextMeshProUGUI fileNameText;
    [Header("External Object Assignments")]
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveContentText;

    private List<UISaveFileHandler> _allSaveOptions;
    private UIMouseHoverScaler _uiMouseHoverScaler;

    private void Awake()
    {
        _allSaveOptions = new List<UISaveFileHandler>(GameObject.FindObjectsOfType<UISaveFileHandler>());
        _uiMouseHoverScaler = GetComponent<UIMouseHoverScaler>();
        fileNameText.text = SaveFileName;
    }

    private void Start()
    {
        // Initialize the hover scaler.
        _uiMouseHoverScaler.Initialize(transform);
        _uiMouseHoverScaler.SetIsInteractable(true);
        // Add events to the hover scaler so that SFX plays and transparency updates.
        _uiMouseHoverScaler.OnHoverEnter.AddListener(() =>
        {
            if (TitleSaveController.CurrentlySelectedSave != this) SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
        });
        _uiMouseHoverScaler.OnHoverEnter.AddListener(() => UpdateTransparency(true));
        _uiMouseHoverScaler.OnHoverExit.AddListener(() => UpdateTransparency(TitleSaveController.CurrentlySelectedSave == this));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TitleSaveController.Instance.IsUIAnimating) { return; }
        // Play button sound effect.
        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
        // Select this file as the current save file.
        SelectAsSaveFile();
    }

    public void SelectAsSaveFile()
    {
        TitleSaveController.CurrentlySelectedSave = this;
        // Set the contents of the preview text.
        saveNameText.text = SaveFileName;
        string currSaveFile = TitleSaveController.Instance.GetCurrentlySelectedSaveFileName();
        if (SaveLoadManager.DoesSaveExist(currSaveFile))
        {
            SaveObject so = SaveLoadManager.GetSaveObject(currSaveFile);
            GameScene currScene = (so.campaignSave != null) ? so.campaignSave.currScene : so.mapObject.currScene;
            saveContentText.text = "Play as hero " + so.hero.heroData.characterName.ToUpper() + " in " + currScene.ToString().ToUpper() + " with " + so.hero.currentHealth + "/" + so.hero.maxHealth + " HP, $" + so.money + ", and " + so.xp + " XP.";
        }
        else
        {
            saveContentText.text = "No information here! Click Play button to begin a new save.";
        }
        // Update transparencies of all save files.
        foreach (UISaveFileHandler saveFile in _allSaveOptions)
        {
            saveFile.UpdateTransparency(saveFile == this);
        }
    }

    public void UpdateTransparency(bool isSelected)
    {
        if (isSelected)
        {
            fileNameText.color = new Color(1, 1, 1, 1);
        }
        else
        {
            fileNameText.color = new Color(1, 1, 1, 0.3f);
        }
    }

}
