using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector]
    public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("Room Prefab")]

    #endregion HeaderROOM PREFAB

    #region Tooltip

    [Tooltip("The game object prefab for the room (this will contain all the tilemaps for the room and environment game objects")]

    #endregion Tooltip
    public GameObject prefab;

    [HideInInspector]
    public GameObject previousPrefab; // this is used to regenerate the guid if the so is copied and the prefab is changed

    #region Header Room Configuration

    #region Tooltip
    [Tooltip("The room node type SO. The room node types correspond to the room nodes used in the room node graph. The exceptions being with corridors." +
        "In the room node graph there is just one corridor type 'Corridor'. For the room template there are two corridor node types - CorridorNS and CorridorEW.")]
    #endregion Tooltip
    public RoomNodeTypeSO roomNodeType;

    #region Tooltip
    [Tooltip("If you imagine a rectangle around the room tilemap that just completly enclose it, the room lower bounds represent the bottom left corner of that rectangle. " +
        "This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that bottom left corner (Note: " +
        "this is the local tilemap position and NOT world position)).")]
    #endregion Tooltip
    public Vector2Int lowerBounds;

    #region Tooltip
    [Tooltip("If you imagine a rectangle around the room tilemap that just completly enclose it, the room lower bounds represent the top right corner of that rectangle. " +
        "This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that top right corner (Note: " +
        "this is the local tilemap position and NOT world position)).")]
    #endregion Tooltip
    public Vector2Int upperBounds;

    #region Tooltip
    [Tooltip("There should be a maximum of four doorways for a room. One for each compass direction. These should have a consistent 3 tile opening " +
        "size, with the middle tile position being the doorway coordinate 'position'.")]
    #endregion Tooltip
    [SerializeField]
    public List<Doorway> doorwayList;

    #region Tooltip
    [Tooltip("Each possible spawn position (used for enemies and chest) for the room in the tilemap coordinates should be added to this array")]
    #endregion Tooltip
    public Vector2Int[] spawnPositionArray;

    #endregion Header Room Configuration

    /// <summary>
    /// Returns the list of Entrances for the room template
    /// </summary>
    /// <returns></returns>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR
    /// <summary>
    /// Validate SO fields
    /// </summary>
    private void OnValidate()
    {
        // Set uniquie GUID if it is empty for the prefab changes
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        // Check the spawn positions populated
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
#endif
    }
    
    #endregion Validation
}
