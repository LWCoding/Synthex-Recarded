using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{

    private void Start()
    {
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(1.25f);
    }

}
