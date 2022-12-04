using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        // Load the room node type list
        LoadRoomNodeTypeList();

        // Set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    /// <summary>
    /// Load the Room Node Type List
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Generates a Random Dungeon, returns true if the Dungeon is built, false otherwise
    /// </summary>
    /// <param name="currentDungeonLevel"></param>
    /// <returns></returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // Load the scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;

        int dungeonBuildAttemps = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttemps < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttemps++;

            // Select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // Loop until the dungeon is built successfully or max attempts is reached
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttempsForRoomGraph)
            {
                dungeonRebuildAttemptsForNodeGraph++;

                // Clear the dungeon
                ClearDungeon();

                // Build the dungeon
                dungeonBuildSuccessful = AttemptsToBuildRoomDungeon(roomNodeGraph);
            }

            // If the dungeon build was successful, then set the dungeon level
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameObjects();
            }
        }

        return dungeonBuildSuccessful;
    }

    /// <summary>
    /// Load the scriptable objects room templates into the dictionary
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key in " + roomTemplateList);
            }
        }
    }

    /// <summary>
    /// Atempts to build the Dungeon
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <returns></returns>
    private bool AttemptsToBuildRoomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // Create Open Room Node Queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // Add an Entrance Node to the Room Node Queue from the Room Node Graph
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
            openRoomNodeQueue.Enqueue(entranceNode);
        else
        {
            Debug.Log("No Entrance Node in " + roomNodeGraph);
            return false;
        }

        // Start with no room overlaps
        bool noRoomOverlaps = true;

        // Proces the Open Room Node Queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // If all rooms has been processed and there is no overlap, return true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Process the Rooms in the Open Room Node Queue
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <param name="openRoomNodeQueue"></param>
    /// <param name="noRoomOverlaps"></param>
    /// <returns></returns>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // While there are rooms in the open room node queue and there is no room overlap detected
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // Get the next Room Node from the Queue
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // Add the child nodes to the queue from the room node graph (with links to this parent room)
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // If the Room has the Entrance, mark it as positioned and add it to the Room Dictionary
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // Add room to the room dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                // Get the parent room for the node
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // Check if the room can be placed without overlapings
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // Assume room is overlap
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            // Select random unconnected available doorway for Parent
            List<Doorway> unconnectedAvailableParentDoorways = GetConnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                Debug.Log("No unconnected available doorways in " + parentRoom);
                return false;
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            // Get a Random Room Template for the Room Node that is consistent with the Parent Door Orientation
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // Create a Room
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // Place the Room. Returns true if the room doesn't overlap
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // Add room to the room dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
                roomOverlaps = false;
                room.isPositioned = true;
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true; // No room overlaps
    }

    /// <summary>
    /// Gets a Random ROom Template for the Room Node that is consistent with the Parent Door Orientation
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        // If room node is a corridor, then select a random correct corridor room template
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // Get the Doorway for the Room
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        if (doorway == null)
        {
            Debug.Log("No Opposite Doorway in " + room);
            doorwayParent.isUnavailable = true;
            return false;
        }

        // Calculate the 'world' grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        // Calculate the adjustment position offset based on the room dorway position that we are trying to connect (e.g. if this doorway is west then we need to add (1,0) to the east parent doorway position)
        Vector2Int adjustment = Vector2Int.zero;
        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none: break;
            default: break;
        }

        // Calculate the room Lower Bounds and Upper Bounds based on the position to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);
        
        if (overlappingRoom == null)
        {
            // Mark doorways as connected and unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    /// <summary>
    /// Get the doorway from the doorway list that is opposite to the parent doorway
    /// </summary>
    /// <param name="parentDoorway"></param>
    /// <param name="doorwayList"></param>
    /// <returns></returns>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (doorwayToCheck.orientation == GetOppositeOrientation(parentDoorway.orientation))
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the OppositeOrientation of orientation
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns></returns>
    private Orientation GetOppositeOrientation(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.north:
                return Orientation.south;
            case Orientation.south:
                return Orientation.north;
            case Orientation.east:
                return Orientation.west;
            case Orientation.west:
                return Orientation.east;
            case Orientation.none:
                return Orientation.none;
            default:
                return Orientation.none;
        }
    }

    /// <summary>
    /// Check for rooms that overlap the upper and lower bounds parameters, and return the Room if overlaps
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns></returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        // Iterate through all of the rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Skip the same room and any room that is not yet positioned
            if (room.id == roomToTest.id || !room.isPositioned) continue;

            if (IsOverlappingRoom(roomToTest, room))
                return room;
        }
        return null;
    }

    /// <summary>
    /// Checks if two rooms overlaps
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverlappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = IsOverlappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.x, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if interval1 overlaps interval2
    /// </summary>
    /// <param name="imin1"></param>
    /// <param name="imax1"></param>
    /// <param name="imin2"></param>
    /// <param name="imax2"></param>
    /// <returns></returns>
    private bool IsOverlappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
            return true;
        else
            return false;
    }


    /// <summary>
    /// Gets a Rnadom Room Template for the roomNodeType provided
    /// </summary>
    /// <param name="roomNodeType"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // Loop through the Room Template List
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // Add all the matching room templates
            if (roomTemplate.roomNodeType == roomNodeType)
                matchingRoomTemplateList.Add(roomTemplate);
        }

        // Return null if the list is empty
        if (matchingRoomTemplateList.Count == 0) return null;

        // Select a Random Room Template from the list
        return matchingRoomTemplateList[Random.Range(0, matchingRoomTemplateList.Count)];
    }

    /// <summary>
    /// Get unconnected doorways for a list of room dorways
    /// </summary>
    /// <param name="roomDorwayList"></param>
    /// <returns></returns>
    private IEnumerable<Doorway> GetConnectedAvailableDoorways(List<Doorway> roomDorwayList)
    {
        foreach (Doorway doorway in roomDorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }


    /// <summary>
    /// Creates a Room from a Room Template
    /// </summary>
    /// <param name="roomTemplate"></param>
    /// <param name="roomNode"></param>
    /// <returns></returns>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // Initializes a Room from the Template
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStingList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        // Set the parent id for the room
        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// Select a Rnadom Room Node Graph from the list
    /// </summary>
    /// <param name="roomNodeGraphList"></param>
    /// <returns></returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No Room Node Graphs in " + roomNodeGraphList);
            return null;
        }
    }

    /// <summary>
    /// Deep copy of a Doorway List
    /// </summary>
    /// <param name="dorwayList"></param>
    /// <returns></returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> dorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in dorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    /// <summary>
    /// Deep copy of a list of Strings
    /// </summary>
    /// <param name="stringList"></param>
    /// <returns></returns>
    private List<string> CopyStingList(List<string> stringList)
    {
        List<string> copyStringList = new List<string>();

        foreach (string s in stringList)
            copyStringList.Add(s);

        return copyStringList;
    }

    /// <summary>
    /// Instantiante the dungeon room gameobjects from the prefabs
    /// </summary>
    private void InstantiateRoomGameObjects()
    {
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Calculate the Room Position (needs to be adjusted by the room template lowerbounds)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // Instantiate the room
            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // Get the instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();
            instantiatedRoom.room = room;

            // Initialize the Instantiated Room
            instantiatedRoom.Initialize(roomGameObject);

            // Save the game object reference
            room.instantiantedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// Gets a Room Template based on the Room Template ID
    /// </summary>
    /// <param name="RoomTemplateID"></param>
    /// <returns></returns>
    public RoomTemplateSO GetRoomTemplate(string RoomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(RoomTemplateID, out RoomTemplateSO roomTemplate))
            return roomTemplate;
        else
            return null;
    }

    /// <summary>
    /// Gets a Room based on the Room ID
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
            return room;
        else
            return null;
    }
    
    /// <summary>
    /// Clear the dungeon by destroying all the instantiated gameobjects in the dungeon builder room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        // Destroy instantiated dungeon gameobjects and clear the dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
            {
                Room room = keyValuePair.Value;

                if (room.instantiantedRoom != null)
                    Destroy(room.instantiantedRoom.gameObject);
            }

            dungeonBuilderRoomDictionary.Clear();
        }
    }
}
