using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeSO", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;

    #region Header
    [Header("One Type should be a Corridor")]
    #endregion Header
    public bool isCorridor;

    #region Header
    [Header("One Type should be a CorridorNS")]
    #endregion Header
    public bool isCorridorNS;

    #region Header
    [Header("One Type should be a CorridorEW")]
    #endregion Header
    public bool isCorridorEW;

    #region Header
    [Header("One Type should be an Entrance")]
    #endregion Header
    public bool isEntrance;

    #region Header
    [Header("One Type should be a Boss Room")]
    #endregion Header
    public bool isBossRoom;

    #region Header
    [Header("One Type should be None (Unassigned)")]
    #endregion Header
    public bool isNone;

    #region Validator
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
