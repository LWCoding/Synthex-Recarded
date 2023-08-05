using Discord;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    public long applicationID;
    [Space]
    public string largeImage;
    public string largeText;

    private Rigidbody rb;
    private long time;

    private static bool instanceExists;
    public Discord.Discord discord;

    void Start()
    {
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);

        time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        UpdateStatus();
    }

    void Update()
    {
        // Destroy the GameObject if Discord isn't running
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(this);
        }
    }

    void LateUpdate()
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
        // Update Status every frame
        try
        {
            string details = "Lost in the title screen...";
            string currState = "Can somebody send help?";
            if (GameController.GetMapScene() != MapScene.NONE)
            {
                if (MapController.Instance != null)
                {
                    details = "Taking a gander at the map";
                }
                if (BattleController.Instance != null)
                {
                    details = "In battle fighting ";
                    for (int i = 0; i < GameController.nextBattleEnemies.Count; i++)
                    {
                        details += GameController.nextBattleEnemies[i].characterName;
                        if (i != GameController.nextBattleEnemies.Count - 1)
                        {
                            details += " and ";
                        }
                    }
                }
                if (ShopController.Instance != null)
                {
                    details = "Purchasing things at the shop";
                }
                if (UpgradeController.Instance != null)
                {
                    details = "Tinkering with the upgrade machine";
                }
                currState = GameController.GetHeroData().characterName + " (" + GameController.GetHeroHealth() + "/" + GameController.GetHeroMaxHealth() + " HP) | " + GameController.GetMapScene().ToString() + " Lvl. " + (GameController.GetMapObject().currLocation.floorNumber + 1);
            }
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = details,
                State = currState,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeText
                },
                Timestamps =
                {
                    Start = time
                }
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed connecting to Discord!");
            });
        }
        catch
        {
            // If updating the status fails, Destroy the GameObject
            Destroy(this);
        }
    }
}