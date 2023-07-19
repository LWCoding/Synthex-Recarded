using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalEnemyController : MonoBehaviour
{

    public static JournalEnemyController Instance;
    [Header("Prefab Assignments")]
    public GameObject enemySelectionPrefab;
    [Header("Object Assignments")]
    public Transform selectionContainerVertTransform;

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
                    newEnemyObject.transform.Find("Preview").GetComponent<Image>().sprite = enemy.idleSprite;
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

}
