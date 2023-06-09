using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details")]
    #endregion Tooltip
    [SerializeField] private MovementDetailsSO movementDetails;

    #region Tooltip
    [Tooltip("The player WeaponShotPosition gameobject in the hierarchy")]
    #endregion Tooltip
    [SerializeField] private Transform weaponShotPosition;

    private Player player;
    private float moveSpeed;
    private Coroutine playerRollCourutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // create waiteForFixedUpdate to use in the coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Set player speed animation
        SetPlayerAnimationSpeed();
    }

    /// <summary>
    /// Set player animation speed to match movement speed
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        if (isPlayerRolling) return;
        MovementInput(); // Process player movement input
        WeaponInput(); // Process player weapon input
        PlayerRollCooldownTimer(); // Player roll cooldown timer
    }

    private void MovementInput()
    {
        // Get movement input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        // Create a direction vector based on the input
        Vector2 direction = new Vector2(horizontalInput, verticalInput);

        // Adjust distance for diagonal movement (pythagoras aproximation)
        if (horizontalInput != 0 && verticalInput != 0)
        {
            direction *= 0.7f;
        }

        // If there is movement either move or roll
        if (direction != Vector2.zero)
        {
            if (!rightMouseButtonDown)
                // Trigger the movement event
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            else if (playerRollCooldownTimer <= 0f)
                // Trigger the roll event
                PlayerRoll((Vector3)direction);
        }
        else
        {
            // No movement
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Player roll
    /// </summary>
    /// <param name="direction"></param>
    private void PlayerRoll(Vector3 direction)
    {
        playerRollCourutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    /// <summary>
    /// Player roll coroutine
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        float minDistance = 0.2f;
        isPlayerRolling = true;
        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;
        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.transform.position, movementDetails.rollSpeed, direction, isPlayerRolling);
            yield return waitForFixedUpdate; // yield and wait for fixed update
        }
        isPlayerRolling = false;
        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0f) playerRollCooldownTimer -= Time.deltaTime;
    }

    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        // Get mouse world position
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // Calculate direction of the vector of mouse cursor from weapon shoot position
        weaponDirection = (mouseWorldPosition - weaponShotPosition.position);

        // Calculate direction vector of mouse cursor from the player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // Get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Set the player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        // Trigger the player aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If collided with something, stop player coroutine
        StopPlayerRollRoutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // If collided with something, stop player coroutine
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if (playerRollCourutine != null)
        {
            StopCoroutine(playerRollCourutine);
            isPlayerRolling = false;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
#endregion Validation
}
