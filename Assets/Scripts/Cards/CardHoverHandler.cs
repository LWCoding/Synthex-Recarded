using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardHandler))]
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public bool allowDragging = true;
    public bool scaleOnHover = true;
    public bool transformOnHover = true;
    public bool sortTopOnHover = true;

    private CardHandler _parentCardHandler;
    private bool isMouseDragging = false;
    private BattleController battleController = BattleController.Instance;
    private List<BattleCharacterController> collidingBCCs = null;
    private Camera _camera;
    private Transform _canvasTransform;
    private Vector3 _boxSize = new Vector3(1.5f, 1.5f, 1.5f); // How big the mouse collider is for cards
    private IEnumerator _scaleCoroutine = null;
    private IEnumerator _showTooltipAfterDelayCoroutine = null;
    private float _tooltipDelay;
    private const float Y_VALUE_TO_INTERACT_FOR_SINGLE_ENEMY = -1.2f;

    public void SetTooltipDelay(float delay) => _tooltipDelay = delay;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _canvasTransform = GameObject.Find("Canvas").transform;
    }

    private void ResetAllBCCColors()
    {
        // Reset the colors of the player and all enemies.
        battleController.playerBCC.TurnUnselectedColor();
        foreach (BattleEnemyController bec in battleController.enemyBCCs)
        {
            bec.TurnUnselectedColor();
        }
    }

    public BattleCharacterController GetOverlappingEnemyBCC()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), _boxSize, 0);
        foreach (Collider2D collider in colliders)
        {
            foreach (BattleEnemyController bec in BattleController.Instance.enemyBCCs)
            {
                if (!bec.IsAlive()) { continue; }
                BoxCollider2D enemyCollider = bec.GetSpriteCollider();
                // If the enemy isn't active, don't consider it.
                if (!enemyCollider.gameObject.activeInHierarchy)
                {
                    return null;
                }
                // If we've matched the collider with the enemy, return the BCC!
                if (collider == enemyCollider)
                {
                    BattleCharacterController bcc = enemyCollider.transform.parent.parent.GetComponent<BattleCharacterController>();
                    return bcc;
                }
            }
        }
        return null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        // Play the card hover sound.
        SoundManager.Instance.PlaySFX(SoundEffect.CARD_HOVER);
        float cardScale = _parentCardHandler.initialScale + ((!scaleOnHover) ? 0 : 0.1f);
        Vector3 verticalPos = (!transformOnHover) ? Vector3.zero : _parentCardHandler.initialPosition + new Vector3(0, 125 * _canvasTransform.localScale.y, 0);
        _scaleCoroutine = _parentCardHandler.LerpTransformAndChangeOrder(cardScale, verticalPos, Quaternion.identity, (sortTopOnHover) ? 10 : _parentCardHandler.initialSortingOrder);
        StartCoroutine(_scaleCoroutine);
        PromptShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isMouseDragging) { return; }
        // Hide the tooltip.
        HideTooltip();
        SendCardBackToInitialPosition();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SettingsManager.Instance.IsGamePaused() || !allowDragging) { return; }
        // Hide the tooltip.
        HideTooltip();
        // Stop the scaling/rotating coroutine and set the rotation to straight.
        StopCoroutine(_scaleCoroutine);
        _parentCardHandler.transform.rotation = Quaternion.identity;
        isMouseDragging = true;
    }

    public void OnDrag(PointerEventData data)
    {
        if (SettingsManager.Instance.IsGamePaused() || !allowDragging) { return; }
        // Stop any scale coroutine 
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        // Lerp the card's position to the mouse's position.
        Vector3 globalMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), data.position, data.pressEventCamera, out globalMousePos);
        _parentCardHandler.transform.position = Vector3.Lerp(transform.position, globalMousePos, 5f);
        // If the card isn't playable, simply make the energy cost red and return.
        if (_parentCardHandler.card.GetCardStats().cardCost > EnergyController.Instance.GetCurrentEnergy())
        {
            EnergyController.Instance.EnergyCostTurnRed();
            return;
        }
        // Initially reset all colors. This may be set again in the code below.
        ResetAllBCCColors();
        // Run logic based on the target of the played card.
        Target cardTarget = _parentCardHandler.card.GetTarget();
        switch (cardTarget)
        {
            case Target.NONE:
            case Target.SELF:
                if (_camera.ScreenToWorldPoint(Input.mousePosition).y > Y_VALUE_TO_INTERACT_FOR_SINGLE_ENEMY)
                {
                    collidingBCCs = new List<BattleCharacterController>() { battleController.playerBCC };
                    battleController.playerBCC.TurnSelectedColor();
                }
                else
                {
                    collidingBCCs = null;
                }
                break;
            case Target.OTHER:
            case Target.PLAYER_AND_ENEMY:
                BattleCharacterController bcc = GetOverlappingEnemyBCC();
                if (bcc == null && battleController.enemyBCCs.Count == 1 && Camera.main.ScreenToWorldPoint(Input.mousePosition).y > Y_VALUE_TO_INTERACT_FOR_SINGLE_ENEMY)
                {
                    bcc = battleController.enemyBCCs[0];
                }
                if (bcc != null)
                {
                    // If we've found an enemy, store its BCC info and make it
                    // colored to show our selection.
                    collidingBCCs = new List<BattleCharacterController>() { bcc };
                    bcc.TurnSelectedColor();
                    // If we're also targeting the player, make the player selected.
                    if (cardTarget == Target.PLAYER_AND_ENEMY)
                    {
                        battleController.playerBCC.TurnSelectedColor();
                    }
                }
                else
                {
                    collidingBCCs = null;
                }
                break;
            case Target.ENEMY_ALL:
                collidingBCCs = new List<BattleCharacterController>();
                foreach (BattleEnemyController bec in BattleController.Instance.enemyBCCs)
                {
                    if (bec.IsAlive())
                    {
                        bec.TurnSelectedColor();
                        collidingBCCs.Add(bec);
                    }
                }
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(EndDragCoroutine(Input.GetMouseButtonUp(0)));
    }

    // We put this into a separate coroutine to prevent weird functionality
    // when the game is paused and the player lets go of the card.
    private IEnumerator EndDragCoroutine(bool wasMouseButtonRaised)
    {
        yield return new WaitUntil(() => !SettingsManager.Instance.IsGamePaused());
        SendCardBackToInitialPosition();
        if (allowDragging && isMouseDragging)
        {
            isMouseDragging = false;
            // If our mouse button wasn't raised this frame (it was probably interrupted),
            // then don't render the card play.
            if (!wasMouseButtonRaised) { yield break; }
            // If we are currently interacting with a valid enemy object,
            // run the card info.
            if (collidingBCCs != null || (battleController.enemyBCCs.Count == 1 && Camera.main.ScreenToWorldPoint(Input.mousePosition).y > Y_VALUE_TO_INTERACT_FOR_SINGLE_ENEMY))
            {
                if (battleController.enemyBCCs.Count == 1) { collidingBCCs = new List<BattleCharacterController>() { battleController.enemyBCCs[0] }; }
                // Reset the colors of the player and all enemies.
                ResetAllBCCColors();
                // If the player can't play it, stop here. Don't let the actions run.
                if (_parentCardHandler.card.GetCardStats().cardCost > EnergyController.Instance.GetCurrentEnergy())
                {
                    yield break;
                }
                // If the code gets here, the player plays the card!
                // If the card has effects, render those effects.
                if (_parentCardHandler.HasCardEffect(CardEffectType.POISON))
                {
                    battleController.playerBCC.ChangeHealth(-4, true);
                }
                battleController.PlayCardInHand(_parentCardHandler.card, collidingBCCs);
            }
        }
    }

    // Send the card back to its original position, from whatever position it is in right now.
    private void SendCardBackToInitialPosition()
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        Vector3 verticalPos = (!transformOnHover) ? Vector3.zero : _parentCardHandler.initialPosition;
        _scaleCoroutine = _parentCardHandler.LerpTransformAndChangeOrder(_parentCardHandler.initialScale, verticalPos, _parentCardHandler.initialRotation, _parentCardHandler.initialSortingOrder);
        StartCoroutine(_scaleCoroutine);
    }

    // Attempt to show the card tooltip after a certain delay.
    // This can be intercepted by calling HideTooltip();
    private void PromptShowTooltip()
    {
        if (_showTooltipAfterDelayCoroutine != null)
        {
            StopCoroutine(_showTooltipAfterDelayCoroutine);
        }
        _showTooltipAfterDelayCoroutine = ShowTooltipAfterDelayCoroutine();
        StartCoroutine(_showTooltipAfterDelayCoroutine);
    }

    private IEnumerator ShowTooltipAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(_tooltipDelay);
        _parentCardHandler.ShowTooltip();
    }

    // Hide the tooltip (or stop the delay) if it is shown.
    private void HideTooltip()
    {
        if (_showTooltipAfterDelayCoroutine != null)
        {
            StopCoroutine(_showTooltipAfterDelayCoroutine);
        }
        _parentCardHandler.HideTooltip();
    }

}
