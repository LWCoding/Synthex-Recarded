using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


public enum Alignment
{
    HERO, ENEMY
}

[RequireComponent(typeof(CharacterStatusHandler))]
public partial class BattleCharacterController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Alignment _characterAlignment;
    [SerializeField] private SpriteRenderer _characterSprite;
    [SerializeField] private SpriteRenderer _characterShadowSprite;
    [SerializeField] protected SpriteRenderer _barIconSprite;
    [SerializeField] private SpriteRenderer _shieldOverlaySprite;
    [SerializeField] private SpriteRenderer _targetSprite;
    [SerializeField] private Animator _targetAnimator;
    [SerializeField] protected TextMeshPro _healthText;
    [SerializeField] protected TextMeshPro _blockText;
    [SerializeField] protected GameObject _healthFillObject;
    [SerializeField] protected Transform _healthBoxTransform;
    [Header("Sprite Assignments")]
    [SerializeField] private Sprite _healthIcon;
    [SerializeField] private Sprite _blockIcon;

    [HideInInspector] public Sprite damagedSprite;
    [HideInInspector] public Sprite idleSprite;
    [HideInInspector] public int health;
    [HideInInspector] public int block;
    [HideInInspector] public int maxHealth;
    [HideInInspector] public List<BattleCharacterController> targetBCCs = new List<BattleCharacterController>();
    [HideInInspector] public UnityEvent<Card> OnPlayCard = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Card> OnPlayedCard = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<BattleCharacterController> OnDealDamage = new UnityEvent<BattleCharacterController>();
    [HideInInspector] public UnityEvent OnTakeDamage = new UnityEvent();
    [HideInInspector] public UnityEvent OnUpdateHealthText = new UnityEvent();
    [HideInInspector] public UnityEvent OnDeath = new UnityEvent();
    [HideInInspector] public CharacterStatusHandler statusHandler;

    private Card _storedCard;
    private Vector3 _initialPosition;
    private Vector2 _healthFillMaxValue;
    protected IEnumerator blockOverlayCoroutine = null;
    protected IEnumerator _flashColorCoroutine = null;
    protected IEnumerator _damageShakeCoroutine = null;
    private Color _blockColor = new Color(0.2f, 0.6f, 1);
    private Color _damagedColor = new Color(1, 0.3f, 0.3f);
    private bool _canSpriteChange = true;

    public virtual void Awake()
    {
        statusHandler = GetComponent<CharacterStatusHandler>();
        _healthFillMaxValue = _healthFillObject.transform.localScale;
    }

    public void Initialize(Character characterInfo)
    {
        idleSprite = characterInfo.idleSprite;
        damagedSprite = characterInfo.damagedSprite;
        _characterSprite.transform.localScale = characterInfo.spriteScale;
        _characterSprite.transform.localPosition = characterInfo.spriteOffset;
        _initialPosition = _characterSprite.transform.localPosition;
        // Scale the shadow sprite and calculate how much we need to enlarge the
        // target sprite correspondingly.
        Vector3 initialShadowScale = _characterShadowSprite.transform.localScale;
        _characterShadowSprite.transform.localScale = characterInfo.shadowScale;
        Vector3 shadowScaleUpwards = new Vector3(_characterShadowSprite.transform.localScale.x / initialShadowScale.x,
                                                _characterShadowSprite.transform.localScale.y / _characterShadowSprite.transform.localScale.y, 1);
        _characterShadowSprite.transform.SetParent(_characterSprite.transform);
        _targetSprite.transform.localScale = shadowScaleUpwards;
        SetCharacterSprite(idleSprite);
        UpdateHealthAndBlockText();
        InitializeStatusEffectScripts();
        TurnUnselectedColor();
    }

    public void InitializeStatusEffectScripts()
    {
        OnDealDamage.AddListener((targetBCC) =>
        {
            // RENDER SHARPEN STATUS EFFECT -> +BLEED PER DEAL DAMAGE
            int sharpenAmplifier = (statusHandler.GetStatusEffect(Effect.SHARPEN) != null) ? statusHandler.GetStatusEffect(Effect.SHARPEN).amplifier : 0;
            if (sharpenAmplifier > 0)
            {
                statusHandler.GetStatusEffect(Effect.SHARPEN).shouldActivate = true;
                targetBCC.statusHandler.AddStatusEffect(Globals.GetStatus(Effect.BLEED, sharpenAmplifier));
            }
        });
        OnTakeDamage.AddListener(() =>
        {
            // RENDER GROWTH STATUS EFFECT -> +STRENGTH PER TAKE DAMAGE
            StatusEffect growth = statusHandler.GetStatusEffect(Effect.GROWTH);
            if (growth != null)
            {
                statusHandler.AddStatusEffect(Globals.GetStatus(Effect.STRENGTH, 1));
            }
            // RENDER BARRIER STATUS EFFECT -> +BLOCK PER TAKE DAMAGE
            StatusEffect barrier = statusHandler.GetStatusEffect(Effect.BARRIER);
            if (barrier != null)
            {
                ChangeBlock(barrier.amplifier);
            }
        });
    }

    // This function will be run at the start of the turn.
    public void TurnStartLogic(int _turnNumber)
    {
        if (_turnNumber != 1)
        {
            // Inflict status effects that run at the start
            // of each turn.
            foreach (StatusEffect s in statusHandler.statusEffects)
            {
                // BLEED EFFECT
                if (s.statusInfo.type == Effect.BLEED)
                {
                    ChangeHealth(-s.amplifier, true);
                    s.ChangeCount(-1);
                    s.shouldActivate = true;
                }
                // FOCUS EFFECT
                if (s.statusInfo.type == Effect.FOCUS)
                {
                    s.shouldActivate = true;
                    s.ChangeCount(-1);
                    EnergyController.Instance.ChangeEnergy(1);
                }
                // LUCKY DRAW EFFECT
                if (_characterAlignment == Alignment.HERO && s.statusInfo.type == Effect.LUCKY_DRAW)
                {
                    s.shouldActivate = true;
                }
            }
            // If the player has the Persevere effect, don't remove block.
            if (statusHandler.GetStatusEffect(Effect.PERSEVERE) != null)
            {
                StatusEffect s = statusHandler.GetStatusEffect(Effect.PERSEVERE);
                s.ChangeCount(-1);
                s.shouldActivate = true;
                statusHandler.UpdateStatusIcons();
                return;
            }
            else
            {
                // Or else, remove all block.
                ChangeBlock(-block, false);
            }
        }
        // Update all status effect icons.
        statusHandler.UpdateStatusIcons();
    }

    // This function will be run at the end of the turn.
    public void TurnEndLogic()
    {
        // Inflict status effects that run at the start
        // of each turn.
        foreach (StatusEffect s in statusHandler.statusEffects)
        {
            // VOLATILE EFFECT
            if (s.statusInfo.type == Effect.VOLATILE)
            {
                s.ChangeCount(-1);
                s.shouldActivate = true;
                // If the amplifier is zero, then this character explodes.
                if (s.amplifier == 0)
                {
                    BattlePooler.Instance.StartOneShotAnimationFromPool(transform.position, "Explosion");
                    SoundManager.Instance.PlaySFX(SoundEffect.EXPLOSION);
                    ChangeHealth(-health, true);
                    foreach (BattleCharacterController bcc in targetBCCs)
                    {
                        bcc.ChangeHealth(-30);
                    }
                }
            }
        }
        // Combo status effect can't stay forever.
        statusHandler.RemoveStatusEffect(Globals.GetStatus(Effect.COMBO));
    }

    public IEnumerator PlayCard(Card c, List<BattleCharacterController> targetBCCs)
    {
        // Disallow playing cards unless the battle is currently ongoing.
        yield return new WaitUntil(() => BattleController.Instance.GetGameState() == GameState.BATTLE);
        this.targetBCCs = targetBCCs;
        OnPlayCard.Invoke(c);
        CardStats cardStats = c.GetCardStats();
        StatusEffect doubleTakeEffect = statusHandler.GetStatusEffect(Effect.DOUBLE_TAKE);
        if (doubleTakeEffect != null)
        {
            doubleTakeEffect.ChangeCount(-1);
            statusHandler.UpdateStatusIcons();
            yield return PlayCardCoroutine(c, 2);
        }
        else
        {
            yield return PlayCardCoroutine(c);
        }
    }

    // This function includes an optional `timesPlayed` variable
    // to repeat the amount of times this card is played. This may
    // be impacted by status effects like Effect.DOUBLE_TAKE.
    private IEnumerator PlayCardCoroutine(Card c, int timesPlayed = 1)
    {
        _storedCard = c;
        for (int i = 0; i < timesPlayed; i++)
        {
            for (int j = 0; j < c.GetCardStats().attackRepeatCount; j++)
            {
                // Play the card's OnPlay sound effect.
                if (c.cardData.onPlaySFX != null)
                {
                    SoundManager.Instance.PlayOneShot(c.cardData.onPlaySFX, c.cardData.onPlaySFXVolume);
                }
                else
                {
                    SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_CARD_PLAYED);
                }
                // Move and render the card effects.
                switch (c.cardData.characterMovement)
                {
                    case Movement.NO_MOVEMENT:
                        yield return NoMovementAttackCoroutine();
                        break;
                    case Movement.SHORT_DASH_FORWARD:
                        yield return ShortDashForwardAttackCoroutine(j == 0);
                        break;
                    case Movement.SHOOT_PROJECTILE:
                        yield return ShootProjectileCoroutine();
                        break;
                }
            }
        }
        OnPlayedCard.Invoke(c);
    }

    // Returns an integer that should be added to damage when calculated
    // in direct attacks. Negative = weaker hits, positive = harder hits.
    public int CalculateDamageModifiers(Card c)
    {
        int damageInc = 0;
        // Add damage based on the current combo.
        StatusEffect combo = statusHandler.GetStatusEffect(Effect.COMBO);
        if (combo != null && combo.specialValue == c.cardData.cardName)
        {
            damageInc += combo.amplifier;
        }
        // Find if there is a strength buff for this character. (Strength status effect!)
        int strengthBuff = (statusHandler.GetStatusEffect(Effect.STRENGTH) != null) ? statusHandler.GetStatusEffect(Effect.STRENGTH).amplifier : 0;
        if (c.GetCardStats().damageValue > 0 && strengthBuff > 0)
        {
            statusHandler.GetStatusEffect(Effect.STRENGTH).shouldActivate = true;
            damageInc += strengthBuff;
        }
        // Find if there is a weakness debuff on this character. (Crippled status effect!)
        int crippledDebuff = (statusHandler.GetStatusEffect(Effect.CRIPPLED) != null) ? statusHandler.GetStatusEffect(Effect.CRIPPLED).amplifier : 0;
        if (c.GetCardStats().damageValue > 0 && crippledDebuff > 0)
        {
            statusHandler.GetStatusEffect(Effect.CRIPPLED).shouldActivate = true;
            damageInc -= crippledDebuff;
        }
        return damageInc;
    }

    // Returns an integer that should be added to damage when calculated
    // in block amounts. Negative = less block, positive = more block.
    public int CalculateDefenseModifiers()
    {
        int defenseInc = 0;
        // Add defense based on the defense buff.
        StatusEffect defense = statusHandler.GetStatusEffect(Effect.DEFENSE);
        if (defense != null)
        {
            defenseInc += defense.amplifier;
        }
        return defenseInc;
    }

    // Returns an integer that represents how much extra damage this character
    // takes from attacks. Positive = takes more damage, negative = takes less damage.
    public int CalculateVulnerabilityModifiers()
    {
        int vulnerabilityInc = 0;
        // Add damage based on the current disease.
        StatusEffect disease = statusHandler.GetStatusEffect(Effect.DISEASE);
        if (disease != null)
        {
            vulnerabilityInc += disease.amplifier;
        }
        return vulnerabilityInc;
    }

    // Renders the printed attack and block actions of a card `c`. Does not carry out any
    // special effects.
    public void RenderAttackAndBlock(Card c, BattleCharacterController targetBCC)
    {
        CardStats cardStats = c.GetCardStats();
        // Perform actions specified on card.
        if (cardStats.damageValue > 0)
        {
            OnDealDamage.Invoke(targetBCC);
            // If we're targeting ourselves, don't try to render any modifiers.
            int damageInc = (cardStats.damageTarget == Target.SELF) ? 0 : CalculateDamageModifiers(c);
            // Deal damage based on what the card displays.
            if (cardStats.damageTarget == Target.OTHER_ALL || cardStats.damageTarget == Target.OTHER || cardStats.damageTarget == Target.PLAYER_AND_ENEMY)
            {
                targetBCC.ChangeHealth(Mathf.Min(0, -(cardStats.damageValue + damageInc)), c.HasTrait(Trait.DAMAGE_IGNORES_BLOCK));
            }
            if (cardStats.damageTarget == Target.SELF || cardStats.damageTarget == Target.PLAYER_AND_ENEMY)
            {
                ChangeHealth(Mathf.Min(0, -(cardStats.damageValue + damageInc)), c.HasTrait(Trait.DAMAGE_IGNORES_BLOCK));
            }
        }
        if (cardStats.blockValue > 0)
        {
            // Find if there is a strength buff for this character. (Strength status effect!)
            int defenseBuff = CalculateDefenseModifiers();
            if (defenseBuff > 0)
            {
                statusHandler.GetStatusEffect(Effect.DEFENSE).shouldActivate = true;
            }
            if (cardStats.damageTarget == Target.OTHER_ALL || cardStats.blockTarget == Target.OTHER || cardStats.blockTarget == Target.PLAYER_AND_ENEMY)
            {
                targetBCC.ChangeBlock(cardStats.blockValue + defenseBuff);
            }
            if (cardStats.blockTarget == Target.SELF || cardStats.blockTarget == Target.PLAYER_AND_ENEMY)
            {
                ChangeBlock(cardStats.blockValue + defenseBuff);
            }
        }
    }

    /// <summary>
    /// Adds or removes health from the character.
    /// + values -> heals
    /// - values -> damages
    /// Does not work if the character is already dead.
    ///</summary>
    public void ChangeHealth(int val, bool ignoresBlock = false)
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

    // Takes a NEGATIVE integer `val` and subtracts that from the current health.
    private void RenderDamage(int val, bool ignoresBlock)
    {
        // If this character has the disease effect and is taking
        // damage, increase that damage by the disease amplifier.
        StatusEffect disease = statusHandler.GetStatusEffect(Effect.DISEASE);
        if (disease != null)
        {
            val -= disease.amplifier;
        }

        // If the move doesn't ignore block, subtract the damage taken
        // by the amount of block.
        if (!ignoresBlock && block > 0 && val != 0)
        {
            ChangeBlock(val);
            // If we damage the shield, play the shield damage SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHIELD_DAMAGE);
            if (block <= 0)
            {
                val = block;
                block = 0;
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
            OnTakeDamage.Invoke();
            // Play the take-damage SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_DAMAGE_TAKEN, Mathf.Lerp(0.65f, 1.2f, Mathf.Min(Mathf.Abs(val / 20f), 1)));
            if (!ignoresBlock)
            {
                ObjectPooler.Instance.SpawnPopup(val.ToString(), 8, _characterSprite.transform.position, new Color(1, 0.1f, 0.1f));
            }
            else
            {
                ObjectPooler.Instance.SpawnPopup(val.ToString(), 8, _characterSprite.transform.position - new Vector3(0, 0.8f), new Color(1, 0.1f, 0.6f));
            }
        }

        // Adjust health value accordingly.
        health += val;
        if (health < 0)
        {
            health = 0;
        }

        // Tint/shake the damaged character.
        FlashColor(_damagedColor, true);
        DamageShake(1, 1);

        // If this character has zero health...
        if (health == 0)
        {
            OnDeath.Invoke();
        }
    }

    // Takes a POSITIVE integer `val` and adds that to the current health.
    // Health cannot go over the maximum limit.
    private void RenderHeal(int val)
    {
        // Render the healing effect, but cap it at max health.
        health += val;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        // Play the heal sound effect.
        SoundManager.Instance.PlaySFX(SoundEffect.HEAL_HEALTH);
        // Spawn a popup text.
        ObjectPooler.Instance.SpawnPopup("+" + val.ToString(), 8, _characterSprite.transform.position - new Vector3(0, 0.8f), new Color(0.1f, 1, 0.1f));
    }

    // Adds or removes block from the character.
    // + values -> gains block
    // - values -> removes block
    public void ChangeBlock(int val, bool shouldPlayBlockAnim = true)
    {
        if (!IsAlive()) { return; }
        block += val;
        if (val > 0)
        {
            // If the function call increases block, show how much block
            // was added.
            ObjectPooler.Instance.SpawnPopup("+" + val.ToString(), 4, _barIconSprite.transform.position + new Vector3(0, 0.5f), new Color(0.9f, 0.95f, 1), 0.5f);
            // Play the increase block sound effect.
            SoundManager.Instance.PlaySFX(SoundEffect.SHIELD_APPLY);
            // Flash the block overlay and tint the character.
            FlashColor(_blockColor);
        }
        else if (val < 0)
        {
            // If the function call reduces block, show how much block
            // was removed.
            ObjectPooler.Instance.SpawnPopup((block < 0) ? (val - block).ToString() : val.ToString(), 4, _barIconSprite.transform.position + new Vector3(0, 0.5f), new Color(0.9f, 0.95f, 1), 0.5f);
        }
        if (shouldPlayBlockAnim)
        {
            FlashBlockOverlay();
        }
        UpdateHealthAndBlockText();
    }

    // Update the health and block bar and text.
    public void UpdateHealthAndBlockText()
    {
        // Update health values.
        _healthText.text = health.ToString() + "/" + maxHealth.ToString();
        float newHealthXScale = _healthFillMaxValue.x * ((float)health / maxHealth);
        _healthFillObject.transform.localScale = new Vector3(newHealthXScale, _healthFillMaxValue.y, 1);
        OnUpdateHealthText.Invoke();
        // Update block values.
        _blockText.text = block.ToString();
        _blockText.gameObject.SetActive(IsAlive() && block > 0);
        _barIconSprite.sprite = (block > 0) ? _blockIcon : _healthIcon;
    }

    public void FlashColor(Color initialColor, bool playHurtAnimation = false)
    {
        if (_flashColorCoroutine != null)
        {
            StopCoroutine(_flashColorCoroutine);
        }
        _flashColorCoroutine = FlashColorCoroutine(initialColor, playHurtAnimation);
        StartCoroutine(_flashColorCoroutine);
    }

    public void DamageShake(float waitTimeMultiplier = 1, float moveDistanceMultiplier = 1)
    {
        if (_damageShakeCoroutine != null)
        {
            StopCoroutine(_damageShakeCoroutine);
        }
        _damageShakeCoroutine = DamageShakeCoroutine(waitTimeMultiplier, moveDistanceMultiplier);
        StartCoroutine(_damageShakeCoroutine);
    }

    private IEnumerator FlashColorCoroutine(Color initialColor, bool playHurtAnimation = false)
    {
        if (playHurtAnimation) { SetCharacterSprite(damagedSprite); }
        Color targetColor = new Color(1, 1, 1);
        _characterSprite.color = initialColor;
        float currTime = 0;
        float targetTime = 0.4f;
        while (currTime < targetTime)
        {
            currTime += Time.deltaTime;
            _characterSprite.color = Color.Lerp(initialColor, targetColor, currTime / targetTime);
            yield return null;
        }
        _characterSprite.color = targetColor;
        SetCharacterSprite(idleSprite);
    }

    private IEnumerator DamageShakeCoroutine(float waitTimeMultiplier, float moveDistanceMultiplier)
    {
        float frames = 0;
        float maxFrames = 60 * 0.05f * waitTimeMultiplier;
        float distance = 0.7f * moveDistanceMultiplier;
        _characterSprite.transform.localPosition = _initialPosition;

        Vector3 targetPosition = _characterSprite.transform.localPosition + Vector3.right * distance;
        while (frames < maxFrames)
        {
            frames++;
            _characterSprite.transform.localPosition = Vector3.Lerp(_initialPosition, targetPosition, (float)frames / maxFrames);
            yield return null;
        }

        frames = 0;
        maxFrames = 60 * 0.07f * waitTimeMultiplier;
        Vector3 initialPosition = targetPosition;
        targetPosition = _characterSprite.transform.localPosition + Vector3.left * distance * 2;
        while (frames < maxFrames)
        {
            frames++;
            _characterSprite.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, (float)frames / maxFrames);
            yield return null;
        }

        frames = 0;
        maxFrames = 60 * 0.12f * waitTimeMultiplier;
        initialPosition = targetPosition;
        while (frames < maxFrames)
        {
            frames++;
            _characterSprite.transform.localPosition = Vector3.Lerp(initialPosition, _initialPosition, (float)frames / maxFrames);
            yield return null;
        }

        _characterSprite.transform.localPosition = _initialPosition;
    }

    protected IEnumerator DisappearCoroutine()
    {
        Color initialColor = new Color(0.6f, 0.6f, 0.6f, 1);
        Color targetColor = new Color(0.6f, 0.6f, 0.6f, 0);
        Color initialShadowColor = _characterShadowSprite.color;
        Color targetShadowColor = _characterShadowSprite.color - new Color(0, 0, 0, 1);
        SetCharacterSprite(damagedSprite);
        float currTime = 0;
        float timeToWait = 1.2f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSprite.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            _characterShadowSprite.color = Color.Lerp(initialShadowColor, targetShadowColor, currTime / timeToWait);
            yield return null;
        }
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

    private IEnumerator NoMovementAttackCoroutine(bool shouldRenderEffects = true)
    {
        // Render particles at the start!
        RenderStartParticleAnimations();
        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            if (shouldRenderEffects)
            {
                RenderCardEffects(targetBCC);
            }
            else
            {
                RenderAttackAndBlock(_storedCard, targetBCC);
            }
        }
        // Render particles at the end!
        RenderEndParticleAnimations();
        // Wait for a bit in case this is repeated twice.
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator ShortDashForwardAttackCoroutine(bool shouldRenderEffects = true)
    {
        float distance = 0.9f * ((_characterAlignment == Alignment.HERO) ? 1 : -1);
        float currTime = 0;
        float timeToWait = 0.12f;

        Vector3 targetPosition = _initialPosition + Vector3.right * distance;

        // Render particles at the start!
        RenderStartParticleAnimations();

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSprite.transform.localPosition = Vector3.Lerp(_initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            if (shouldRenderEffects)
            {
                RenderCardEffects(targetBCC);
            }
            else
            {
                RenderAttackAndBlock(_storedCard, targetBCC);
            }
        }

        currTime = 0;
        timeToWait = 0.08f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSprite.transform.localPosition = Vector3.Lerp(targetPosition, _initialPosition, currTime / timeToWait);
            yield return null;
        }

        // Render particles at the end!
        RenderEndParticleAnimations();

        _characterSprite.transform.localPosition = _initialPosition;
    }

    private IEnumerator ShootProjectileCoroutine(bool shouldRenderEffects = true)
    {
        float timeToReachTarget = 0.4f;

        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            Vector3 projectileTargetPosition = targetBCC.transform.position;
            Vector3 adjustedSpawnPosition = transform.position + new Vector3((_characterAlignment == Alignment.HERO ? 1 : -1) * _storedCard.cardData.projectileOffset.x, _storedCard.cardData.projectileOffset.y, 0);
            BattlePooler.Instance.StartProjectileAnimationFromPool(adjustedSpawnPosition, projectileTargetPosition, _storedCard.cardData.projectileSprite, timeToReachTarget);
        }

        // Render particles at the start!
        RenderStartParticleAnimations();

        float currTime = 0;
        float timeToWait = 0.1f;

        Vector3 targetPosition = _initialPosition + Vector3.right * 0.7f;

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSprite.transform.localPosition = Vector3.Lerp(_initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        currTime = 0;
        timeToWait = 0.06f;
        Vector3 initialPosition = _characterSprite.transform.localPosition;
        targetPosition = _initialPosition;

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSprite.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        _characterSprite.transform.localPosition = _initialPosition;

        yield return new WaitForSeconds(timeToReachTarget * 0.7f);

        // Render particles at the end!
        RenderEndParticleAnimations();

        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            if (shouldRenderEffects)
            {
                RenderCardEffects(targetBCC);
            }
            else
            {
                RenderAttackAndBlock(_storedCard, targetBCC);
            }
        }

    }

    private void RenderStartParticleAnimations()
    {
        // Calculate the spawn position of the particles.
        Vector3 adjustedSpawnPosition = transform.position + new Vector3((_characterAlignment == Alignment.HERO ? 1 : -1) * _storedCard.cardData.projectileOffset.x, _storedCard.cardData.projectileOffset.y, 0);
        // Calculate the burst direction if this is a hero or enemy. Hero = right, enemy = left.
        int burstDirection = (_characterAlignment == Alignment.HERO) ? 1 : -1;
        // Spawn the particles at the start!
        ParticleInfo particleInfo = _storedCard.cardData.sourceParticleInfo;
        if (particleInfo.particleType != ParticleType.NONE)
        {
            BattlePooler.Instance.StartParticleAnimationFromPool(particleInfo, adjustedSpawnPosition, burstDirection);
        }
    }

    private void RenderEndParticleAnimations()
    {
        // Calculate the burst direction if this is a hero or enemy. Hero = right, enemy = left.
        int burstDirection = (_characterAlignment == Alignment.HERO) ? 1 : -1;

        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            // Spawn the particles at the end.
            ParticleInfo particleInfo = _storedCard.cardData.destinationParticleInfo;
            if (particleInfo.particleType != ParticleType.NONE)
            {
                BattlePooler.Instance.StartParticleAnimationFromPool(particleInfo, targetBCC.transform.position, burstDirection);
            }
        }
    }

    // Set the character's sprite. (e.g. idle, damaged, dead)
    // If the second parameter is true, the sprite can no longer change.
    public void SetCharacterSprite(Sprite newSprite, bool lockSprite = false)
    {
        if (!_canSpriteChange) { return; }
        _characterSprite.sprite = newSprite;
        if (lockSprite) { _canSpriteChange = false; }
    }

    public void TurnSelectedColor()
    {
        if (!IsAlive()) { return; }
        _characterSprite.color = new Color(0.8f, 0.8f, 0.8f);
        _targetSprite.enabled = true;
        _targetAnimator.enabled = true;
        _targetAnimator.Play("Pulse");
    }

    public void TurnUnselectedColor()
    {
        if (!IsAlive()) { return; }
        _characterSprite.color = new Color(1, 1, 1);
        _targetSprite.enabled = false;
        _targetAnimator.enabled = false;
    }

    public void MakeUninteractable()
    {
        Destroy(_characterSprite.GetComponent<BoxCollider2D>());
    }

    public bool IsAlive()
    {
        return health != 0;
    }

}

