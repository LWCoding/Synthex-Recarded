using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LevelSelectController : MonoBehaviour
{

    public static LevelSelectController Instance;
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject _mapOptionPrefab;
    [Header("Object Assignments")]
    [SerializeField] private Transform _playerIconObject;
    [SerializeField] private TextMeshPro _introBannerText;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip _footstepsSFX;

    private List<MapOptionController> _mapOptions = new List<MapOptionController>();

    // Make singleton instance of this class.
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    private void Start()
    {
        LoadAllLevels();
    }

    // Find all levels and load them into the _mapOptions list.
    // This is performance-heavy, so it should be done sparingly.
    private void LoadAllLevels()
    {
        _mapOptions = new List<MapOptionController>(GetComponents<MapOptionController>());
    }

}
