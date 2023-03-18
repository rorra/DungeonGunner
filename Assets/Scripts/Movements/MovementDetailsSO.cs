using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header

    #region Tooltip
    [Tooltip("The minimum move speed. The GetMoveSpeed method calculates a random value between the min and max move speed.")]
    #endregion Tooltip
    public float minMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("The maximum move speed. The GetMoveSpeed method calculates a random value between the min and max move speed.")]
    #endregion Tooltip
    public float maxMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("If there is a roll movement - this is the roll speed")]
    #endregion Tooltip
    public float rollSpeed;

    #region Tooltip
    [Tooltip("If there is a roll movement - this is the roll distance")]
    #endregion Tooltip
    public float rollDistance;

    #region Tooltip
    [Tooltip("If there is a roll movement - this is the cooldown time in seconds between roll actions")]
    #endregion Tooltip
    public float rollCooldownTime;

    /// <summary>
    /// Get a random movement speed between the min and max move speed
    /// </summary>
    /// <returns></returns>
    public float GetMoveSpeed()
    {
        if (minMoveSpeed == maxMoveSpeed) return minMoveSpeed;
        return Random.Range(minMoveSpeed, maxMoveSpeed);
    }

    #region Validator
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMoveSpeed), minMoveSpeed, nameof(maxMoveSpeed), maxMoveSpeed, false);
        
        if (rollDistance != 0f || rollSpeed != 0 || rollCooldownTime != 0)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollCooldownTime), rollCooldownTime, false);
        }
    }
#endif
    #endregion Validator
}
