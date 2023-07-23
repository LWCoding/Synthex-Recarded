using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _exitButton;

    private void Start()
    {
        // Initialize the buttons that should modify UI elements.
        _upgradeButton.onClick.AddListener(() => TopBarController.Instance.ToggleCardOverlay(GameController.GetHeroCards(), _upgradeButton));
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(1.25f);
    }

}
