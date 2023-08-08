using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalEnemyTabController : MonoBehaviour
{

    [Header("Prefab Assignments")]
    public GameObject enemySelectionPrefab;
    [Header("Object Assignments")]
    public Transform selectionContainerVertTransform;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemySubtextText;
    public TextMeshProUGUI enemyDescText;
    public RawImage enemyImage;
    public Transform enemyShadowTransform;

    private Enemy _currEnemyShowing;
    private List<Enemy> _currEnemyList;
    private List<Transform> _renderedEnemySelections = new List<Transform>();

    // Initializes the selection container's enemies based on a
    // list of enemies that is supplied. Max of 20 enemies.
    // Empty slots are rendered as empty slots.
    public void InitializeEnemySelections(List<Enemy> enemies)
    {
        _currEnemyList = enemies;
        // Destroy all of the enemy selection frames that may be rendered.
        ClearAllEnemySelections();
        // We can't render more than twenty enemies per page.
        if (enemies.Count > 20) { return; }
        for (int row = 0; row < 4; row++)
        {
            // Create a horizontal layout group for every row that we render.
            GameObject newRowObject = CreateNewEnemySelectionRow(12);
            newRowObject.transform.SetParent(selectionContainerVertTransform, false);
            _renderedEnemySelections.Add(newRowObject.transform);
            // Now, loop five times, creating five columns in each row.
            for (int column = 0; column < 5; column++)
            {
                int currIdx = row * 5 + column;
                // Attempt to find the enemy at the current index.
                Enemy enemy = (currIdx >= enemies.Count) ? null : enemies[currIdx];
                // If the enemy exists, initialize the square with the enemy's information.
                // Or else, just leave it empty.
                if (enemy != null)
                {
                    GameObject newEnemyObject = Instantiate(enemySelectionPrefab, newRowObject.transform);
                    _renderedEnemySelections.Add(newEnemyObject.transform);
                    RectTransform maskTransform = (RectTransform)newEnemyObject.transform.Find("Mask");
                    GameObject previewObject = maskTransform.Find("Preview").gameObject;
                    previewObject.GetComponent<RawImage>().texture = enemy.enemyIcon.texture;
                    // Check if the enemy has been discovered yet. If not, change the enemy's
                    // icon and change the information when the button is clicked.
                    bool hasBeenDiscovered = PlayerPrefs.GetInt(enemy.characterName) > 0;
                    if (!hasBeenDiscovered)
                    {
                        previewObject.GetComponent<RawImage>().color = new Color(0, 0, 0);
                    }
                    newEnemyObject.transform.Find("ExclamationIcon").gameObject.SetActive(PlayerPrefs.GetInt(enemy.characterName) == 1);
                    // Add a listener so that, when clicked, it'll set the enemy information.
                    maskTransform.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
                        SetEnemyInfo(enemy);
                        InitializeEnemySelections(enemies);
                    });
                }
            }
        }
    }

    public void UnlockNewEnemy(Enemy enemy)
    {
        // If this enemy has never been unlocked, set it to be unlocked
        // and play the alert animation!
        if (PlayerPrefs.GetInt(enemy.characterName) == 0)
        {
            PlayerPrefs.SetInt(enemy.characterName, 1);
            JournalManager.Instance.PlayAlertAnimation();
        }
        // If the journal is already showing, update that!
        if (JournalManager.Instance.IsJournalShowing())
        {
            // Additionally, if we're currently on that enemy,
            // just make it auto-unlock.
            if (_currEnemyShowing == enemy)
            {
                PlayerPrefs.SetInt(enemy.characterName, 2);
                SetEnemyInfo(enemy);
            }
            InitializeEnemySelections(_currEnemyList);
        }
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewEnemySelectionRow(int spacing)
    {
        GameObject newRow = new GameObject("EnemyRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = false;
        newRowHLG.childControlHeight = false;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = spacing;
        return newRow;
    }

    // Clears all enemies currently in the journal.
    private void ClearAllEnemySelections()
    {
        while (_renderedEnemySelections.Count > 0)
        {
            GameObject objToDelete = _renderedEnemySelections[0].gameObject;
            _renderedEnemySelections.RemoveAt(0);
            Destroy(objToDelete);
        }
    }

    // Sets an enemy to be currently active in the enemy preview.
    public void SetEnemyInfo(Enemy enemy)
    {
        _currEnemyShowing = enemy;
        bool hasBeenDiscovered = PlayerPrefs.GetInt(enemy.characterName) > 0;
        // If we've discovered the enemy, set the appropriate data. 
        // Or else, make the enemy's information unknown.
        if (hasBeenDiscovered)
        {
            PlayerPrefs.SetInt(enemy.characterName, 2);
            JournalManager.Instance.UpdateAnimationStatus();
            enemyNameText.SetText(enemy.characterName);
            enemyDescText.SetText(enemy.characterDesc);
            enemySubtextText.SetText("Location found: <color=\"green\">" + enemy.locationFound + "</color>");
            enemyImage.color = new Color(1, 1, 1);
        }
        else
        {
            enemyNameText.SetText("???");
            enemyDescText.SetText("This enemy hasn't been discovered yet. Play the game and encounter it to learn more.");
            enemySubtextText.SetText("Location found: <color=#A9A9A9>Undiscovered</color>");
            enemyImage.color = new Color(0, 0, 0);
        }
        enemyImage.texture = enemy.idleSprite.texture;
        enemyImage.transform.localScale = enemy.spriteScale;
        enemyShadowTransform.localScale = enemy.shadowScale;
        float aspect = (float)Screen.width / Screen.height;
        float worldHeight = Camera.main.orthographicSize * 2;
        float worldWidth = worldHeight * aspect;
        enemyImage.transform.localPosition = enemy.spriteOffset * new Vector2(1280 / worldWidth, 720 / worldHeight);
        // Set the native size of the sprite.
        enemyImage.SetNativeSize();
    }

}
