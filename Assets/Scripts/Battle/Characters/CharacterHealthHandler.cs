using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public struct BattleTooltip
{
    public string text;
    public int fontSize;
    public Vector3 position;
    public Color color;
    public float speed;
}

[RequireComponent(typeof(CharacterStatusHandler))]
public partial class CharacterHealthHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private SpriteRenderer _characterSprite;
    [SerializeField] private SpriteRenderer _barIconSprite;
    [SerializeField] private SpriteRenderer _shieldOverlaySprite;
    [SerializeField] private TextMeshPro _healthText;
    [SerializeField] private TextMeshPro _blockText;
    [SerializeField] private GameObject _healthFillObject;
    [SerializeField] private Transform _healthBoxTransform;
    [Header("Sprite Assignments")]
    [SerializeField] private Sprite _healthIcon;
    [SerializeField] private Sprite _blockIcon;

    private int _health;
    public int GetHealth() => _health;
    public bool IsAlive() => _health > 0;
    private int _maxHealth;
    public int GetMaxHealth() => _maxHealth;
    private int _block;
    public int GetBlock() => _block;

    private CharacterStatusHandler statusHandler;
    private BattleCharacterController bcc;

    private Vector2 _healthFillMaxValue;
    protected IEnumerator blockOverlayCoroutine = null;
    private IEnumerator renderTooltipCoroutine = null;
    private List<BattleTooltip> tooltipsToRender = new List<BattleTooltip>();
    private Color _blockColor = new Color(0.2f, 0.6f, 1);
    private Color _damagedColor = new Color(1, 0.3f, 0.3f);

    private void Awake()
    {
        bcc = GetComponent<BattleCharacterController>();
        statusHandler = GetComponent<CharacterStatusHandler>();
        _healthFillMaxValue = _healthFillObject.transform.localScale;
    }

    public void Initialize(int startingHP, int maxHP)
    {
        _maxHealth = maxHP;
        _health = startingHP;
        UpdateHealthAndBlockText();
    }

    /// <summary>
    /// Adds or removes _health from the character.
    /// + values -> heals
    /// - values -> damages
    /// Does not work if the character is already dead.
    ///</summary>
    public void ChangeHealth(int val, bool ignoresBlock)
    {
        if (!IsAlive()) { return; }
        if (val <= 0)
        {
            RenderDamage(val, ignoresBlock);
        }
        else
        {
            RenderHeal(val);
        }
        UpdateHealthAndBlockText();
    }

    // Takes a NEGATIVE integer `val` and subtracts that from the current _health.
    private void RenderDamage(int val, bool ignoresBlock)
    {
        // If this character has the disease effect and is taking
        // damage, increase that damage by the disease amplifier.
        StatusEffect disease = statusHandler.GetStatusEffect(Effect.DISEASE);
        if (disease != null) { val -= disease.amplifier; }

        // If the move doesn't ignore _block, subtract the damage taken
        // by the amount of _block.
        if (!ignoresBlock && _block > 0 && val != 0)
        {
            ChangeBlock(val, true);
            // If we damage the shield, play the shield damage SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHIELD_DAMAGE);
            if (_block <= 0)
            {
                val = _block;
                _block = 0;
            }
            else
            {
                val = 0;
            }
        }

        // Spawn pop-up text for damage amount, if the damage isn't equal to 0.
        if (val != 0)
        {
            // This sprite took damage, so invoke this method.
            bcc.OnTakeDamage.Invoke();
            // Play the take-damage SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_DAMAGE_TAKEN, Mathf.Lerp(0.65f, 1.2f, Mathf.Min(Mathf.Abs(val / 20f), 1)));
            if (!ignoresBlock)
            {
                RenderTooltip(val.ToString(), 8, _characterSprite.transform.position, new Color(1, 0.1f, 0.1f), 1);
            }
            else
            {
                RenderTooltip(val.ToString(), 8, _characterSprite.transform.position - new Vector3(0, 0.8f), new Color(1, 0.1f, 0.6f), 1);
            }
        }

        // Adjust _health value accordingly.
        _health += val;
        _health = Mathf.Max(_health, 0);

        // Tint/shake the damaged character.
        bcc.FlashColor(_damagedColor, true);
        bcc.DamageShake(1, 1);

        // If this character dies, invoke the proper function.
        if (!IsAlive()) { bcc.OnDeath.Invoke(); }
    }

    // Takes a POSITIVE integer `val` and adds that to the current _health.
    // _health cannot go over the maximum limit.
    private void RenderHeal(int val)
    {
        // Render the healing effect, but cap it at max _health.
        _health += val;
        if (_health > _maxHealth)
        {
            _health = _maxHealth;
        }
        // Play the heal sound effect.
        SoundManager.Instance.PlaySFX(SoundEffect.HEAL_HEALTH);
        // Spawn a popup text.
        RenderTooltip("+" + val.ToString(), 8, _characterSprite.transform.position - new Vector3(0, 0.8f), new Color(0.1f, 1, 0.1f), 0.5f);
    }

    // Adds or removes _block from the character.
    // + values -> gains _block
    // - values -> removes _block
    public void ChangeBlock(int val, bool shouldPlayBlockAnim)
    {
        if (!IsAlive()) { return; }
        _block += val;
        if (val > 0)
        {
            // If the function call increases _block, show how much _block
            // was added.
            RenderTooltip("+" + val.ToString(), 4, _barIconSprite.transform.position + new Vector3(0, 0.5f), new Color(0.9f, 0.95f, 1), 0.5f);
            // Play the increase _block sound effect.
            SoundManager.Instance.PlaySFX(SoundEffect.SHIELD_APPLY);
            // Flash the _block overlay and tint the character.
            bcc.FlashColor(_blockColor);
        }
        else if (val < 0)
        {
            // If the function call reduces _block, show how much _block was removed.
            RenderTooltip((_block < 0) ? (val - _block).ToString() : val.ToString(), 4, _barIconSprite.transform.position + new Vector3(0, 0.5f), new Color(0.9f, 0.95f, 1), 0.5f);
        }
        if (shouldPlayBlockAnim)
        {
            FlashBlockOverlay();
        }
        UpdateHealthAndBlockText();
    }

    public void RenderTooltip(string text, int fontSize, Vector3 position, Color color, float speed)
    {
        BattleTooltip bt = new BattleTooltip();
        bt.text = text;
        bt.fontSize = fontSize;
        bt.position = position;
        bt.color = color;
        bt.speed = speed;
        tooltipsToRender.Add(bt);
        if (renderTooltipCoroutine == null)
        {
            renderTooltipCoroutine = RenderNumberTooltipsCoroutine();
            StartCoroutine(renderTooltipCoroutine);
        }
    }

    private IEnumerator RenderNumberTooltipsCoroutine()
    {
        while (tooltipsToRender.Count > 0)
        {
            BattleTooltip nextTooltip = tooltipsToRender[0];
            tooltipsToRender.RemoveAt(0);
            ObjectPooler.Instance.SpawnPopup(nextTooltip.text, nextTooltip.fontSize, nextTooltip.position, nextTooltip.color, nextTooltip.speed);
            yield return new WaitForSeconds(0.18f);
        }
        yield return new WaitForEndOfFrame();
        renderTooltipCoroutine = null;
    }

    // Update the _health and _block bar and text.
    public void UpdateHealthAndBlockText()
    {
        // Update _health values.
        _healthText.text = _health.ToString() + "/" + _maxHealth.ToString();
        float newHealthXScale = _healthFillMaxValue.x * ((float)_health / _maxHealth);
        _healthFillObject.transform.localScale = new Vector3(newHealthXScale, _healthFillMaxValue.y, 1);
        bcc.OnUpdateHealthText.Invoke();
        // Update _block values.
        _blockText.text = _block.ToString();
        _blockText.gameObject.SetActive(IsAlive() && _block > 0);
        _barIconSprite.sprite = (_block > 0) ? _blockIcon : _healthIcon;
    }

    public void FlashBlockOverlay()
    {
        if (blockOverlayCoroutine != null)
        {
            StopCoroutine(blockOverlayCoroutine);
        }
        blockOverlayCoroutine = FlashBlockOverlayCoroutine();
        StartCoroutine(blockOverlayCoroutine);
    }

    public IEnumerator FlashBlockOverlayCoroutine()
    {
        _shieldOverlaySprite.color = new Color(1, 1, 1, 0.8f);
        _shieldOverlaySprite.transform.localPosition = new Vector3(0, 0.4f, 0);
        WaitForSeconds wfs = new WaitForSeconds(0.025f);
        for (int i = 0; i < 20; i++)
        {
            _shieldOverlaySprite.color -= new Color(0, 0, 0, 0.04f);
            _shieldOverlaySprite.transform.localPosition = Vector3.Lerp(_shieldOverlaySprite.transform.localPosition, new Vector3(0, -0.4f, 0), 0.05f);
            yield return wfs;
        }
    }

    public void DisableCharacterUI()
    {
        _barIconSprite.gameObject.SetActive(false);
        _healthBoxTransform.gameObject.SetActive(false);
        _healthText.gameObject.SetActive(false);
        _blockText.gameObject.SetActive(false);
        statusHandler.statusParentTransform.gameObject.SetActive(false);
    }

}

