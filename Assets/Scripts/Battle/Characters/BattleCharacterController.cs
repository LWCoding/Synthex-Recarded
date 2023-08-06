using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public enum CharacterState
{
    IDLE = 0, DAMAGED = 1, DEATH = 2
}

public enum Alignment
{
    HERO = 0, ENEMY = 1
}

[RequireComponent(typeof(CharacterHealthHandler))]
[RequireComponent(typeof(CharacterStatusHandler))]
public partial class BattleCharacterController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Alignment _characterAlignment;
    [SerializeField] protected Transform _characterSpriteContainer;
    [SerializeField] protected SpriteRenderer _characterSpriteRenderer;
    [SerializeField] protected SpriteRenderer _characterShadowSpriteRenderer;
    [SerializeField] protected SpriteRenderer _targetSpriteRenderer;
    [SerializeField] protected BoxCollider2D _spriteCollider;
    public BoxCollider2D GetSpriteCollider() => _spriteCollider;
    [SerializeField] protected Animator _targetAnimator;

    private Character _characterInfo;
    protected List<BattleCharacterController> targetBCCs = new List<BattleCharacterController>();
    [HideInInspector] public UnityEvent<Card> OnPlayCard = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<Card> OnPlayedCard = new UnityEvent<Card>();
    [HideInInspector] public UnityEvent<BattleCharacterController> OnDealDamage = new UnityEvent<BattleCharacterController>();
    [HideInInspector] public UnityEvent OnTakeDamage = new UnityEvent();
    [HideInInspector] public UnityEvent OnUpdateHealthText = new UnityEvent();
    [HideInInspector] public UnityEvent OnDeath = new UnityEvent();

    protected CharacterStatusHandler statusHandler;
    public StatusEffect GetStatusEffect(Effect e) => statusHandler.GetStatusEffect(e);
    public void AddStatusEffect(StatusEffect s) => statusHandler.AddStatusEffect(s);
    public void RemoveStatusEffect(Effect e) => statusHandler.RemoveStatusEffect(e);

    protected CharacterHealthHandler healthHandler;
    public void ChangeHealth(int change, bool ignBlock = false) => healthHandler.ChangeHealth(change, ignBlock);
    public void ChangeBlock(int change, bool playAnim = true) => healthHandler.ChangeBlock(change, playAnim);
    public void DisableCharacterUI() => healthHandler.DisableCharacterUI();
    public void InitializeHealthData(int startingHP, int maxHP) => healthHandler.Initialize(startingHP, maxHP);
    public bool IsAlive() => healthHandler.IsAlive();
    public int GetHealth() => healthHandler.GetHealth();
    public int GetMaxHealth() => healthHandler.GetMaxHealth();
    public int GetBlock() => healthHandler.GetBlock();

    private Card _storedCard;
    private Vector3 _initialSpriteLocalPosition;
    private Vector3 _initialSpritePosition;
    protected IEnumerator _flashColorCoroutine = null;
    protected IEnumerator _damageShakeCoroutine = null;
    private bool _canSpriteChange = true;

    public virtual void Awake()
    {
        statusHandler = GetComponent<CharacterStatusHandler>();
        healthHandler = GetComponent<CharacterHealthHandler>();
    }

    // Reset the sprite's box collider by destroying it and re-adding it.
    // Then, increase the box collider slightly to allow for better detection.
    private IEnumerator ResetBoxCollider()
    {
        GameObject colliderObject = _spriteCollider.gameObject;
        Destroy(_spriteCollider);
        yield return new WaitForEndOfFrame();
        _spriteCollider = colliderObject.AddComponent<BoxCollider2D>();
        _spriteCollider.size += new Vector2(1, 1);
    }

    public void Initialize(Character charInfo)
    {
        // Store this character's information.
        _characterInfo = charInfo;
        // Initialize the box collider.
        StartCoroutine(ResetBoxCollider());
        // Set the positions and scales.
        _characterSpriteRenderer.transform.localScale = charInfo.spriteScale;
        _characterSpriteRenderer.transform.localPosition = charInfo.spriteOffset;
        _initialSpritePosition = _characterSpriteContainer.transform.position;
        _initialSpriteLocalPosition = _characterSpriteRenderer.transform.localPosition;
        // Scale the shadow sprite and calculate how much we need to enlarge the
        // target sprite correspondingly.
        Vector3 initialShadowScale = _characterShadowSpriteRenderer.transform.localScale;
        _characterShadowSpriteRenderer.transform.localScale = charInfo.shadowScale;
        Vector3 shadowScaleUpwards = new Vector3(_characterShadowSpriteRenderer.transform.localScale.x / initialShadowScale.x,
                                                _characterShadowSpriteRenderer.transform.localScale.y / _characterShadowSpriteRenderer.transform.localScale.y, 1);
        _characterShadowSpriteRenderer.transform.SetParent(_characterSpriteRenderer.transform);
        _targetSpriteRenderer.transform.localScale = shadowScaleUpwards;
        SetCharacterSprite(CharacterState.IDLE);
        InitializeStatusEffectScripts();
        TurnUnselectedColor();
    }

    private void InitializeStatusEffectScripts()
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
            // RENDER TARGET'S REFLECT STATUS EFFECT -> TAKE DAMAGE ON ATTACK
            int reflectAmplifier = (targetBCC.statusHandler.GetStatusEffect(Effect.REFLECT) != null) ? targetBCC.statusHandler.GetStatusEffect(Effect.REFLECT).amplifier : 0;
            if (reflectAmplifier > 0)
            {
                targetBCC.statusHandler.GetStatusEffect(Effect.REFLECT).shouldActivate = true;
                ChangeHealth(-reflectAmplifier);
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
                switch (s.statusInfo.type)
                {
                    // Bleed effect should change health.
                    case Effect.BLEED:
                        ChangeHealth(-s.amplifier, true);
                        s.shouldActivate = true;
                        break;
                    // Focus effect should reward one energy.
                    case Effect.FOCUS:
                        s.shouldActivate = true;
                        EnergyController.Instance.ChangeEnergy(1);
                        break;
                    // Effects that should flash if it is the hero.
                    case Effect.LUCKY_DRAW:
                        if (_characterAlignment == Alignment.HERO) { s.shouldActivate = true; }
                        break;
                }
            }
            // If the player has the Persevere effect, don't remove block.
            if (statusHandler.GetStatusEffect(Effect.PERSEVERE) != null)
            {
                StatusEffect s = statusHandler.GetStatusEffect(Effect.PERSEVERE);
                s.shouldActivate = true;
            }
            else
            {
                // Or else, remove all block.
                ChangeBlock(-GetBlock(), false);
            }
        }
        // Decrement all status effects that should decrement every turn.
        for (int i = statusHandler.statusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect s = statusHandler.statusEffects[i];
            if (s.statusInfo.decrementEveryTurn)
            {
                statusHandler.DecrementStatusEffect(s.statusInfo.type, 1);
                s.shouldActivate = true;
            }
        }
    }

    // This function will be run at the end of the turn.
    public void TurnEndLogic()
    {
        // Inflict status effects that run at the start
        // of each turn.
        foreach (StatusEffect s in statusHandler.statusEffects)
        {
            switch (s.statusInfo.type)
            {
                case Effect.VOLATILE:
                    statusHandler.DecrementStatusEffect(Effect.VOLATILE, 1);
                    // If the amplifier is zero, then this character explodes.
                    if (s.amplifier == 0)
                    {
                        BattlePooler.Instance.StartOneShotAnimationFromPool(transform.position, "Explosion");
                        SoundManager.Instance.PlaySFX(SoundEffect.EXPLOSION);
                        ChangeHealth(-GetHealth(), true);
                        foreach (BattleCharacterController bcc in targetBCCs)
                        {
                            bcc.ChangeHealth(-30);
                        }
                    }
                    break;
            }
        }
        // Combo status effect can't stay forever.
        statusHandler.RemoveStatusEffect(Effect.COMBO);
    }

    public IEnumerator PlayCardCoroutine(Card c, List<BattleCharacterController> targetBCCs)
    {
        // Disallow playing cards unless the battle is currently ongoing.
        yield return new WaitUntil(() => BattleController.Instance.GetGameState() == GameState.PLAYER_TURN);
        this.targetBCCs = targetBCCs;
        OnPlayCard.Invoke(c);
        CardStats cardStats = c.GetCardStats();
        StatusEffect doubleTakeEffect = statusHandler.GetStatusEffect(Effect.DOUBLE_TAKE);
        if (doubleTakeEffect != null)
        {
            statusHandler.DecrementStatusEffect(Effect.DOUBLE_TAKE, 1);
            yield return RenderCardCoroutine(c, 2);
        }
        else
        {
            yield return RenderCardCoroutine(c);
        }
    }

    // This function includes an optional `timesPlayed` variable
    // to repeat the amount of times this card is played. This may
    // be impacted by status effects like Effect.DOUBLE_TAKE.
    private IEnumerator RenderCardCoroutine(Card c, int timesPlayed = 1)
    {
        // If we're playing a card that should be repeated, stop the cards in hand from
        // being chosen until its done.
        if (c.GetCardStats().attackRepeatCount > 1 || timesPlayed > 1)
        {
            // Make it so the player can't play cards while this is animating.
            BattleController.Instance.DisableInteractionsForCardsInHand();
        }
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
                // If all targets are dead, just ignore rendering.
                List<BattleCharacterController> aliveTargets = targetBCCs.FindAll((e) => e.IsAlive());
                if (aliveTargets.Count == 0) { break; }
            }
        }
        OnPlayedCard.Invoke(c);
        // Let the player play cards again.
        BattleController.Instance.EnableInteractionsForCardsInHand();
    }

    // Renders the printed attack and block actions of a card `c`. Does not carry out any
    // special effects.
    private void RenderAttackAndBlock(Card c, BattleCharacterController targetBCC)
    {
        CardStats cardStats = c.GetCardStats();
        // Perform actions specified on card.
        if (cardStats.damageValue > 0)
        {
            OnDealDamage.Invoke(targetBCC);
            // If we're targeting ourselves, don't try to render any modifiers.
            int damageInc = (cardStats.damageTarget == Target.SELF) ? 0 : CalculateDamageModifiers(c);
            // Deal damage based on what the card displays.
            if (cardStats.damageTarget == Target.ENEMY_ALL || cardStats.damageTarget == Target.OTHER || cardStats.damageTarget == Target.PLAYER_AND_ENEMY)
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
            if (cardStats.damageTarget == Target.ENEMY_ALL || cardStats.blockTarget == Target.OTHER || cardStats.blockTarget == Target.PLAYER_AND_ENEMY)
            {
                targetBCC.ChangeBlock(cardStats.blockValue + defenseBuff);
            }
            if (cardStats.blockTarget == Target.SELF || cardStats.blockTarget == Target.PLAYER_AND_ENEMY)
            {
                ChangeBlock(cardStats.blockValue + defenseBuff);
            }
        }
    }

    // Returns an integer that should be added to damage when calculated
    // in direct attacks. Negative = weaker hits, positive = harder hits.
    public int CalculateDamageModifiers(Card c)
    {
        int damageInc = 0;
        // Add damage based on the current combo.
        StatusEffect combo = statusHandler.GetStatusEffect(Effect.COMBO);
        if (combo != null && combo.specialValue == c.GetCardDisplayName())
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
        if (playHurtAnimation) { SetCharacterSprite(CharacterState.DAMAGED); }
        Color targetColor = new Color(1, 1, 1);
        _characterSpriteRenderer.color = initialColor;
        float currTime = 0;
        float timeToWait = 0.4f;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteRenderer.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        _characterSpriteRenderer.color = targetColor;
        SetCharacterSprite(CharacterState.IDLE);
    }

    private IEnumerator DamageShakeCoroutine(float waitTimeMultiplier, float moveDistanceMultiplier)
    {
        float currTime = 0;
        float timeToWait = 0.05f * waitTimeMultiplier;
        float distance = 0.7f * moveDistanceMultiplier;

        Vector3 targetPosition = _characterSpriteRenderer.transform.localPosition + Vector3.right * distance;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteRenderer.transform.localPosition = Vector3.Lerp(_initialSpriteLocalPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        currTime = 0;
        timeToWait = 0.07f * waitTimeMultiplier;
        Vector3 initialPosition = targetPosition;
        targetPosition = _characterSpriteRenderer.transform.localPosition + Vector3.left * distance * 2;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteRenderer.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        currTime = 0;
        timeToWait = 0.12f * waitTimeMultiplier;
        initialPosition = targetPosition;
        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteRenderer.transform.localPosition = Vector3.Lerp(initialPosition, _initialSpriteLocalPosition, currTime / timeToWait);
            yield return null;
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
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator ShortDashForwardAttackCoroutine(bool shouldRenderEffects = true)
    {
        float distance = 0.9f * ((_characterAlignment == Alignment.HERO) ? 1 : -1);
        float currTime = 0;
        float timeToWait = 0.12f;

        Vector3 targetPosition = _initialSpritePosition + Vector3.right * distance;

        // Render particles at the start!
        RenderStartParticleAnimations();

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteContainer.transform.position = Vector3.Lerp(_initialSpritePosition, targetPosition, currTime / timeToWait);
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
            _characterSpriteContainer.transform.position = Vector3.Lerp(targetPosition, _initialSpritePosition, currTime / timeToWait);
            yield return null;
        }

        // Render particles at the end!
        RenderEndParticleAnimations();

        _characterSpriteContainer.transform.position = _initialSpritePosition;
    }

    private IEnumerator ShootProjectileCoroutine(bool shouldRenderEffects = true)
    {
        float timeToReachTarget = 0.4f;

        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            Vector3 projectileTargetPosition = targetBCC.transform.position;
            Vector3 adjustedSpawnPosition = _characterSpriteContainer.transform.position + new Vector3((_characterAlignment == Alignment.HERO ? 1 : -1) * _storedCard.cardData.projectileOffset.x, _storedCard.cardData.projectileOffset.y, 0);
            BattlePooler.Instance.StartProjectileAnimationFromPool(adjustedSpawnPosition, projectileTargetPosition, _storedCard.cardData.projectileSprite, timeToReachTarget);
        }

        // Render particles at the start!
        RenderStartParticleAnimations();

        float currTime = 0;
        float timeToWait = 0.1f;

        Vector3 targetPosition = _initialSpritePosition + Vector3.right * 0.7f;

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteContainer.transform.position = Vector3.Lerp(_initialSpritePosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        currTime = 0;
        timeToWait = 0.06f;
        Vector3 initialPosition = _characterSpriteContainer.transform.position;
        targetPosition = _initialSpritePosition;

        while (currTime < timeToWait)
        {
            currTime += Time.deltaTime;
            _characterSpriteContainer.transform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
            yield return null;
        }

        _characterSpriteContainer.transform.position = _initialSpritePosition;

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
        Vector3 adjustedSpawnPosition = _characterSpriteContainer.transform.position + new Vector3((_characterAlignment == Alignment.HERO ? 1 : -1) * _storedCard.cardData.projectileOffset.x, _storedCard.cardData.projectileOffset.y, 0);
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
    public void SetCharacterSprite(CharacterState newState, bool lockSprite = false)
    {
        if (!_canSpriteChange) { return; }
        switch (newState)
        {
            case CharacterState.IDLE:
                _characterSpriteRenderer.sprite = _characterInfo.idleSprite;
                break;
            case CharacterState.DAMAGED:
                _characterSpriteRenderer.sprite = _characterInfo.damagedSprite;
                break;
            case CharacterState.DEATH:
                _characterSpriteRenderer.sprite = (_characterInfo.deathSprite == null) ? _characterInfo.damagedSprite : _characterInfo.deathSprite;
                break;
        }
        if (lockSprite) { _canSpriteChange = false; }
    }

    public void TurnSelectedColor()
    {
        if (!IsAlive()) { return; }
        _characterSpriteRenderer.color = new Color(0.8f, 0.8f, 0.8f);
        _targetSpriteRenderer.enabled = true;
        _targetAnimator.enabled = true;
        _targetAnimator.Play("Pulse");
    }

    public void TurnUnselectedColor()
    {
        if (!IsAlive()) { return; }
        _characterSpriteRenderer.color = new Color(1, 1, 1);
        _targetSpriteRenderer.enabled = false;
        _targetAnimator.enabled = false;
    }

    public void MakeUninteractable()
    {
        Destroy(_characterSpriteRenderer.GetComponent<BoxCollider2D>());
    }

}

