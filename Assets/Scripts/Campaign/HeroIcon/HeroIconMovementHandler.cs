using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroIconMovementHandler : MonoBehaviour
{

    private void Update()
    {
        // If events have not completed, don't let the player move.
        if (!CampaignEventController.Instance.AreEventsComplete()) { return; }
        // Render player movement based on key inputs.
        // TODO: Actually write the code!
    }

}
