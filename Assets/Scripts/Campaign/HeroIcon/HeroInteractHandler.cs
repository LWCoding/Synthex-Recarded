using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class HeroInteractHandler : MonoBehaviour
{

    [Header("Interaction Range")]
    [Tooltip("The radius of which this sprite can interact with other objects")]
    [SerializeField] private float _interactRange;

    private CircleCollider2D _collider;
    private List<CampaignOptionController> _allCollidingControllers = new();

    private void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.radius = _interactRange;
    }

    public void SelectLevel(CampaignOptionController optController)
    {
        CampaignController.Instance.CurrentLevel = optController;
        optController.SelectLevel();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we get near a CampaignOptionController, select it as the current level.
        if (collision.gameObject.TryGetComponent(out CampaignOptionController optController))
        {
            _allCollidingControllers.Add(optController);
            SelectLevel(optController);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // If we get near a CampaignOptionController, trigger.
        if (collision.gameObject.TryGetComponent(out CampaignOptionController optController))
        {
            optController.DeselectLevel();
            _allCollidingControllers.Remove(optController);
            if (_allCollidingControllers.Count > 0)
            {
                SelectLevel(_allCollidingControllers[^1]);
            }
            else
            {
                CampaignController.Instance.CurrentLevel = null;
            }
        }
    }

#if UNITY_EDITOR
    // Draws the circle indicating the _interactRange.
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, _interactRange);
    }
#endif

}
