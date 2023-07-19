using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalEnemyController : MonoBehaviour
{

    public static JournalEnemyController Instance;
    [Header("Prefab Assignments")]
    public GameObject enemySelectionPrefab;
    [Header("Object Assignments")]
    public Transform selectionContainerVertTransform;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemySubtextText;
    public TextMeshProUGUI enemyDescText;
    public RawImage enemyImage;
    public Transform enemyShadowTransform;

    private List<Transform> renderedEnemySelections = new List<Transform>();

    // If we don't have a JournalEnemyController instance already
    // set, set it to this one. Or else, delete it.
    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // Initializes the selection container's enemies based on a
    // list of enemies that is supplied. Max of 20 enemies.
    // Empty slots are rendered as empty slots.
    public void InitializeEnemySelections(List<Enemy> enemies)
    {
        // Destroy all of the enemy selection frames that may be rendered.
        ClearAllEnemySelections();
        // We can't render more than twenty enemies per page.
        if (enemies.Count > 20) { return; }
        for (int row = 0; row < 4; row++)
        {
            // Create a horizontal layout group for every row that we render.
            GameObject newRowObject = CreateNewEnemySelectionRow(12);
            newRowObject.transform.SetParent(selectionContainerVertTransform);
            renderedEnemySelections.Add(newRowObject.transform);
            // Now, loop five times, creating five columns in each row.
            for (int column = 0; column < 5; column++)
            {
                int currIdx = row * 5 + column;
                // Attempt to find the enemy at the current index.
                Enemy enemy = (currIdx >= enemies.Count) ? null : enemies[currIdx];
                // If the enemy exists, initialize the square with the enemy's information.
                // Or else, just leave it empty.
                GameObject newEnemyObject = Instantiate(enemySelectionPrefab, newRowObject.transform);
                renderedEnemySelections.Add(newEnemyObject.transform);
                if (enemy != null)
                {
                    GameObject previewObject = newEnemyObject.transform.Find("Preview").gameObject;
                    previewObject.GetComponent<Image>().sprite = enemy.idleSprite;
                    previewObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetEnemyInfo(enemy);
                    });
                }
            }
        }
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewEnemySelectionRow(int spacing)
    {
        GameObject newRow = new GameObject("CardRow", typeof(HorizontalLayoutGroup));
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
        while (renderedEnemySelections.Count > 0)
        {
            GameObject objToDelete = renderedEnemySelections[0].gameObject;
            renderedEnemySelections.RemoveAt(0);
            Destroy(objToDelete);
        }
    }

    // Sets an enemy to be currently active in the enemy preview.
    public void SetEnemyInfo(Enemy enemy)
    {
        enemyNameText.SetText(enemy.characterName);
        enemyDescText.SetText(enemy.characterDesc);
        enemySubtextText.SetText("Location found: <color=\"green\">" + enemy.locationFound + "</color>");
        enemyImage.texture = enemy.idleSprite.texture;
        enemyImage.transform.localScale = enemy.spriteScale;
        enemyShadowTransform.localScale = enemy.shadowScale;
        float aspect = (float)Screen.width / Screen.height;
        float worldHeight = Camera.main.orthographicSize * 2;
        float worldWidth = worldHeight * aspect;
        enemyImage.transform.localPosition = enemy.spriteOffset * new Vector2(Screen.width / worldWidth, Screen.height / worldHeight);
        // Set the native size of the sprite.
        enemyImage.SetNativeSize();
    }

}
