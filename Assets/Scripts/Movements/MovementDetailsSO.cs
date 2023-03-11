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
    }
#endif
    #endregion Validator
}
