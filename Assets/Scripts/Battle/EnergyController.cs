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
    public Image energyGlowImage;
    public TextMeshProUGUI energyText;
    public Color badEnergyTextColor;
    public UnityEvent<int> OnEnergyChanged = new UnityEvent<int>(); // Current amt of energy as param.

    [HideInInspector] private int _currentEnergy;
    [HideInInspector] private int _maxEnergy = 3;
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
        _initialEnergyTextColor = energyText.color;
    }

    public int GetCurrentEnergy()
    {
        return _currentEnergy;
    }

    public void RestoreEnergy()
    {
        // If the character has the POWERBANK relic, grant +1 energy if the energy is more than zero.
        if (GameController.HasRelic(RelicType.POWERBANK) && GetCurrentEnergy() > 0)
        {
            _currentEnergy = _maxEnergy;
            UpdateEnergy(1);
            TopBarController.Instance.FlashRelicObject(RelicType.POWERBANK);
            return;
        }
        _currentEnergy = _maxEnergy;
    }

    public void UpdateEnergy(int change)
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
        energyText.text = _currentEnergy.ToString() + "/" + _maxEnergy.ToString();
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
        Color initialColor = energyGlowImage.color;
        energyGlowImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1);
        while (Mathf.Abs(energyGlowImage.color.a - targetColor.a) > 0.1f)
        {
            energyGlowImage.color = Color.Lerp(energyGlowImage.color, targetColor, increment);
            yield return null;
        }

        targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        while (Mathf.Abs(energyGlowImage.color.a - targetColor.a) > 0.01f)
        {
            energyGlowImage.color = Color.Lerp(energyGlowImage.color, targetColor, increment);
            yield return null;
        }

        energyGlowImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
    }

    public void EnergyCostTurnRed()
    {
        StartCoroutine(EnergyCostRedCoroutine());
    }

    // Makes the energy text become RED during battle.
    // Sets back to normal after mouse button is released.
    private IEnumerator EnergyCostRedCoroutine()
    {
        energyText.color = badEnergyTextColor;
        yield return new WaitUntil(() =>
        {
            return !Input.GetMouseButton(0);
        });
        energyText.color = _initialEnergyTextColor;
    }

}
