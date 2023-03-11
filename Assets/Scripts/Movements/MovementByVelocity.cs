using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementByVelocityEvent))]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D rigidBody2d;
    private MovementByVelocityEvent movementByVelocityEvent;

    private void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
    }

    private void OnEnable()
    {
        movementByVelocityEvent.OnMovementByVelocityEvent += MovementByVelocityEvent_OnMovementByVelocityEvent;
    }

    private void OnDisable()
    {
        movementByVelocityEvent.OnMovementByVelocityEvent -= MovementByVelocityEvent_OnMovementByVelocityEvent;
    }

    /// <summary>
    /// On movement event
    /// </summary>
    /// <param name="movementByVelocityEvent"></param>
    /// <param name="movementByVelocityEventArgs"></param>
    private void MovementByVelocityEvent_OnMovementByVelocityEvent(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityEventArgs movementByVelocityEventArgs)
    {
        MoveRigidBody(movementByVelocityEventArgs.moveDirection, movementByVelocityEventArgs.moveSpeed);
    }

    /// <summary>
    /// Move the rigid body component
    /// </summary>
    /// <param name="moveDirection"></param>
    /// <param name="moveSpeed"></param>
    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {
        // ensure the rb collision detection is set to continuous
        rigidBody2d.velocity = moveDirection * moveSpeed;
    }
}
