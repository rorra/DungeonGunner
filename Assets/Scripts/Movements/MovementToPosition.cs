using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementToPositionEvent))]
[DisallowMultipleComponent]

public class MovementToPosition : MonoBehaviour
{
    private Rigidbody2D rigidBody2d;
    private MovementToPositionEvent movementToPositionEvent;
    
    private void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    private void OnEnable()
    {
        movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;
    }

    private void OnDisable()
    {
        movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;
    }

    // On movement event
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        MoveRigidBody(movementToPositionArgs.movePosition, movementToPositionArgs.currentPosition, movementToPositionArgs.moveSpeed);
    }

    /// <summary>
    /// Move the rigid body component
    /// </summary>
    /// <param name="movePosition"></param>
    /// <param name="currentPosition"></param>
    /// <param name="moveSpeed"></param>
    private void MoveRigidBody(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);
        rigidBody2d.MovePosition(rigidBody2d.position + (unitVector * moveSpeed * Time.fixedDeltaTime));
    }

}
