using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class EnergyController : MonoBehaviour
{

    public static EnergyController Instance;
    [Header("Object Assignments")]
    [SerializeField] private Image _energyGlowImage;
    [SerializeField] private TextMeshProUGUI _energyText;
    [SerializeField] private Color _badEnergyTextColor;

    [HideInInspector] public UnityEvent<int> OnEnergyChanged = new UnityEvent<int>(); // Current amt of energy as param.

    private int _currentEnergy;
    private int _maxEnergy = 3;
    private Color _initialEnergyTextColor;
    private IEnumerator _energyGlowCoroutine = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = GetComponent<EnergyController>();
        _initialEnergyTextColor = _energyText.color;
    }

    public int GetCurrentEnergy()
    {
        return _currentEnergy;
    }

    public void RestoreEnergy()
    {
        // If the character has the POWERBANK relic, grant +1 energy if the energy is more than zero.
        if (GameManager.HasRelic(RelicType.POWERBANK) && GetCurrentEnergy() > 0)
        {
            _currentEnergy = _maxEnergy;
            ChangeEnergy(1);
            TopBarController.Instance.FlashRelicObject(RelicType.POWERBANK);
            return;
        }
        _currentEnergy = _maxEnergy;
    }

    public void ChangeEnergy(int change)
    {
        _currentEnergy += change;
        OnEnergyChanged.Invoke(_currentEnergy);
        // If we're gaining energy, play the gain energy sound effect.
        if (change > 0)
        {
            SoundManager.Instance.PlaySFX(SoundEffect.CHARGE_ENERGY);
        }
        UpdateEnergyText();
        // Make the energy cost glow to make it look cool.
        EnergyGlow();
    }

    public void UpdateMaxEnergy(int change, bool shouldRestoreAllEnergy = true)
    {
        _maxEnergy += change;
        UpdateEnergyText();
        if (shouldRestoreAllEnergy)
        {
            RestoreEnergy();
        }
    }

    public void UpdateEnergyText()
    {
        _energyText.text = _currentEnergy.ToString() + "/" + _maxEnergy.ToString();
    }

    public void EnergyGlow()
    {
        if (_energyGlowCoroutine != null)
        {
            StopCoroutine(_energyGlowCoroutine);
        }
        _energyGlowCoroutine = EnergyGlowCoroutine();
        StartCoroutine(_energyGlowCoroutine);
    }

    private IEnumerator EnergyGlowCoroutine()
    {
        float time = 0.2f;
        float increment = 1 / time * Time.deltaTime;
        Color initialColor = _energyGlowImage.color;
        _energyGlowImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1);
        while (Mathf.Abs(_energyGlowImage.color.a - targetColor.a) > 0.1f)
        {
            _energyGlowImage.color = Color.Lerp(_energyGlowImage.color, targetColor, increment);
            yield return null;
        }

        targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        while (Mathf.Abs(_energyGlowImage.color.a - targetColor.a) > 0.01f)
        {
            _energyGlowImage.color = Color.Lerp(_energyGlowImage.color, targetColor, increment);
            yield return null;
        }

        _energyGlowImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
    }

    public void EnergyCostTurnRed()
    {
        StartCoroutine(EnergyCostRedCoroutine());
    }

    // Makes the energy text become RED during battle.
    // Sets back to normal after mouse button is released.
    private IEnumerator EnergyCostRedCoroutine()
    {
        _energyText.color = _badEnergyTextColor;
        yield return new WaitUntil(() =>
        {
            return !Input.GetMouseButton(0);
        });
        _energyText.color = _initialEnergyTextColor;
    }

}
