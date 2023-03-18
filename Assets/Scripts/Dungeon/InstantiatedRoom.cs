using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Save the room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    // Triggers room changed event when the player enters into the room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom()) 
        {
            this.room.isPreviouslyVisited = true;
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    /// <summary>
    /// Initialize the instantiated room
    /// </summary>
    /// <param name="roomGameObject"></param>
    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);

        BlockOffUnusedDoorways();

        AddDoorsToRooms();

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// Populate the tilemap member variables
    /// </summary>
    /// <param name="roomGameObject"></param>
    public void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = GetComponentInChildren<Grid>();

        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap") groundTilemap = tilemap;
            if (tilemap.gameObject.tag == "decoration1Tilemap") decoration1Tilemap = tilemap;
            if (tilemap.gameObject.tag == "decoration2Tilemap") decoration2Tilemap = tilemap;
            if (tilemap.gameObject.tag == "frontTilemap") frontTilemap = tilemap;
            if (tilemap.gameObject.tag == "collisionTilemap") collisionTilemap = tilemap;
            if (tilemap.gameObject.tag == "minimapTilemap") minimapTilemap = tilemap;
        }
    }

    /// <summary>
    /// Block Off Unused Doorways
    /// </summary>
    private void BlockOffUnusedDoorways()
    {
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected) continue;

            // Block unconnected doorways
            if (collisionTilemap != null)
                BlockADoorwayTilemapLayer(collisionTilemap, doorway);
            if (minimapTilemap != null)
                BlockADoorwayTilemapLayer(minimapTilemap, doorway);
            if (groundTilemap != null)
                BlockADoorwayTilemapLayer(groundTilemap, doorway);
            if (decoration1Tilemap != null)
                BlockADoorwayTilemapLayer(decoration1Tilemap, doorway);
            if (decoration2Tilemap != null)
                BlockADoorwayTilemapLayer(decoration2Tilemap, doorway);
            if (frontTilemap != null)
                BlockADoorwayTilemapLayer(frontTilemap, doorway);

        }
    }

    /// <summary>
    /// Blocks a doorway on a tilemap layer
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockADoorwayTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
                
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;
        }
    }

    /// <summary>
    /// Blocks a doorway horizontally
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Blocks a doorway vertically
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="doorway"></param>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // loop through all tiles to copy
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Add opening doors if this is not a corridor room
    /// </summary>
    public void AddDoorsToRooms()
    {
        if (room.roomNodeType.isCorridorNS || room.roomNodeType.isCorridorEW) return;

        // Instatiate door prefabs at doorway positions
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float titleDistance = Settings.titleSizePixels / Settings.pixelPerUnit;
                GameObject door = null;
                
                if (doorway.orientation == Orientation.north)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + titleDistance / 2f, doorway.position.y + titleDistance, 0f);
                }
                else if (doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + titleDistance / 2f, doorway.position.y, 0f);
                }
                else if (doorway.orientation == Orientation.east)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + titleDistance, doorway.position.y + titleDistance * 1.25f, 0f);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x, doorway.position.y + titleDistance * 1.25f, 0f);
                }

                Door doorComponent = door.GetComponent<Door>();
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;
                    doorComponent.LockDoor();
                }
            }
        }
    }

    /// <summary>
    /// Disables the collision tilemap renderer
    /// </summary>
    public void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}
