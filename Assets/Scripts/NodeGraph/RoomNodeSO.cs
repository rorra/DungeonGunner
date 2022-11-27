using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// Initialise the room node
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="roomNodeGraph"></param>
    /// <param name="roomNodeType"></param>
    public void Initialise(Rect rect, RoomNodeGraphSO roomNodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = roomNodeGraph;
        this.roomNodeType = roomNodeType;

        // Load the room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw the node with the nodeStyle
    /// </summary>
    /// <param name="nodeStyle"></param>
    public void Draw(GUIStyle nodeStyle)
    {
        // Draw the node box using the begin area
        GUILayout.BeginArea(rect, nodeStyle);

        // Start the region to detect popup selection changes
        EditorGUI.BeginChangeCheck();

        // If the room node has a parent, or if its type is entrance, then display a label, else a popup
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // Display a label that cannot be changed
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // If the room type selection has changed making child connections potentially invalid
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // If a room node type has been changed and it already has children then delete the parent child links since we need to revalidate any
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // Get child room node
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // If the child room node is not null
                        if (childRoomNode != null)
                        {
                            // Remove childID from parent room node
                            RemoveChildRoomNodeID(childRoomNode.id);

                            // Remove parentID from child room node
                            childRoomNode.RemoveParentRoomNodeID(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Populate the room node types to display in the popup
    /// </summary>
    /// <returns></returns>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomNodeTypesToDisplay = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
                roomNodeTypesToDisplay[i] = roomNodeTypeList.list[i].name;
        }
        
        return roomNodeTypesToDisplay;
    }

    /// <summary>
    ///  Processes events for the room node
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
        }
    }
    
    /// <summary>
    /// Process a mouse click event
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftClickDownEvent();

        if (currentEvent.button == 1)
            ProcessRightClickDownEvent(currentEvent);
    }

    /// <summary>
    /// Process a left click down event
    /// </summary>
    public void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        // Toggle node selection
        isSelected = !isSelected;
    }

    /// <summary>
    /// Process a right click down event
    /// </summary>
    public void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    ///  Process a mouse up event
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftClickUpEvent();
    }

    /// <summary>
    /// Process a left click mouse up event
    /// </summary>
    public void ProcessLeftClickUpEvent()
    {
        isLeftClickDragging = false;
    }

    /// <summary>
    /// Process a mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftMouseDragEvent(currentEvent);
    }

    /// <summary>
    /// Process a mouse drag event on the left mouse button
    /// </summary>
    /// <param name="currentEvent"></param>
    public void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drag the node
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Add a parentID to the node (returns true on successful)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddParentRoomNodeId(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// Add a childID to the node (returns true on successful)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildRoomNodeID(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the childID can be added to the parent
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        
        // Check if there is already a connected boss room in the node graph
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
                break;
            }
        }

        // If the child node has a type of boss room and there is already a connected boss room, then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        // If the child node has a type of none, return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;

        // If the node already has a child with this child ID, return false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        // If this node ID and the child ID are the same, return false
        if (id == childID)
            return false;

        // If the child node already has a parent, return false
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        // If the child is a corridor, and this room is a corridor, return false
        if (roomNodeType.isCorridor && roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor)
            return false;

        // If the child is not a corridor, and this room is not a corridor, return false
        if (!roomNodeType.isCorridor && !roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor)
            return false;

        // If adding a corridor, check that this room doesn't have the maximum amount of corridors already
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        // If the child node is an entrance, return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;

        // If adding a room to a corridor, check that this corridor node doesn't already have a room added
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// Removes a childID from the node (returns true on successful)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool RemoveChildRoomNodeID(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a parentID from the node (returns true on successful)
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool RemoveParentRoomNodeID(string parentID)
    {
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }
#endif
}
