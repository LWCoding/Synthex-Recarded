using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeroMovementHandler : MonoBehaviour
{

    private Rigidbody2D _rb;
    private readonly float _moveSpeed = 3;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // If events have not completed, don't let the player move.
        if (!CampaignEventController.Instance.AreEventsComplete()) { return; }
        // Render player movement based on key inputs.
        RenderPlayerMovement();
    }

    private void RenderPlayerMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(horizontalInput, verticalInput) * _moveSpeed;
        _rb.velocity = movement;
    }

}
