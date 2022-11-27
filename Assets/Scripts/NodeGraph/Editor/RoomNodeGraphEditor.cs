using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

public class RoomNodeGenerator : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeStyleSelected;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeTypeListSO roomNodeTypeList;
    private RoomNodeSO currentRoomNode = null;

    // Node layout values
    private const float nodeWidth = 250f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // Connecting line values
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 12f;

    // Grid spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<RoomNodeGenerator>("Room Node Graph Editor");
    }
    private void OnEnable()
    {
        // Subscribe to the inspector selection change event
        Selection.selectionChanged += OnInspectorSelectionChanged;

        // Define the node layout style
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Define the selected node layout style
        roomNodeStyleSelected = new GUIStyle();
        roomNodeStyleSelected.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        roomNodeStyleSelected.normal.textColor = Color.white;
        roomNodeStyleSelected.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyleSelected.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Load room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    private void OnDisabled()
    {
        Selection.selectionChanged -= OnInspectorSelectionChanged;
    }

    

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in the inspector
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    [OnOpenAsset(0)] // Need the namespace UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;
            
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            // Draw Grid
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridSmall, 0.3f, Color.gray);

            // Draw line if it is being dragged
            DrawDraggedLine();

            // Process Events
            ProcessEvents(Event.current);

            // Draw connections between room nodes
            DrawRoomConnections();

            // Draw Room Nodes
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        // Reset graph drag
        graphDrag = Vector2.zero;

        // Sets the current room node being dragged
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        if (!currentRoomNode || currentRoomNodeGraph.roomNodeDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// Checks whether the mouse is over a room node, if so, it returns the room node
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <returns></returns>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeList != null)
        {
            for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
            {
                if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
                {
                    return currentRoomNodeGraph.roomNodeList[i];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Process Room Node Graph Events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
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
                
            default:
                break;
        }
    }

    /// <summary>
    /// Process a mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
            ProcessLeftMouseDragEvent(currentEvent);
        else if (currentEvent.button == 1)
            ProcessRightMouseDragEvent(currentEvent);
    }

    /// <summary>
    /// Process a mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        graphDrag = currentEvent.delta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(graphDrag);
        }
    }

    /// <summary>
    /// Process a mouse drag event with the right click
    /// </summary>
    /// <param name="currentEvent"></param>
    void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeDrawLineFrom != null)
        {
            DrawConnectionLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    void DrawConnectionLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Process mouse down events on the room node graph
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Right click
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // Left click
        else if (currentEvent.button == 0)
        {
            ClearLineDraw();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Process mouse up events on the room node graph
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeDrawLineFrom != null)
        {
            // Check if it is over a room node
            RoomNodeSO roomNodeTo = IsMouseOverRoomNode(currentEvent);
            
            if (currentRoomNodeGraph.roomNodeDrawLineFrom.AddChildRoomNodeID(roomNodeTo.id))
                roomNodeTo.AddParentRoomNodeId(currentRoomNodeGraph.roomNodeDrawLineFrom.id);

            ClearLineDraw();
        }
    }

    /// <summary>
    /// Clear line drag from a room node
    /// </summary>
    public void ClearLineDraw()
    {
        currentRoomNodeGraph.roomNodeDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// Selects all room nodes
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    ///  Deletes the selected room node links
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeID(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeID(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// Deletes the selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodesToDelete = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodesToDelete.Enqueue(roomNode);

                // Iterate through child room nodes ids
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeID(roomNode.id);
                    }
                }

                // Iterate through parent room nodes ids
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeID(roomNode.id);
                    }
                }
            }
        }

        // Delete the queued room nodes
        while (roomNodesToDelete.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodesToDelete.Dequeue();
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Clear selection from all nodes
    /// </summary>
    public void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }            
        }
    }

    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        // Create a menu
        GenericMenu menu = new GenericMenu();

        // Add menu items
        menu.AddItem(new GUIContent("Add Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        // Show the menu
        menu.ShowAsContext();
    }

    /// <summary>
    /// Create a room node at the mouse position
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // If the current node graph is empty, create an entrance room node first
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }
        
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // Create a room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // Add the room node to the current room node graph room node list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // Set the room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // Add the room node to the room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // Refresh the graph node dictionary
        currentRoomNodeGraph.OnValidate();
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSize * i, -gridSize, 0) + gridOffset, 
                new Vector3(gridSize * i, position.height, 0f) + gridOffset
                );
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSize, gridSize * j, 0) + gridOffset,
                new Vector3(position.width, gridSize * j, 0f) + gridOffset
                );
        }
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(
                currentRoomNodeGraph.roomNodeDrawLineFrom.rect.center,
                currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeDrawLineFrom.rect.center,
                currentRoomNodeGraph.linePosition,
                Color.white,
                null,
                connectingLineWidth
            );
        }
    }

    /// <summary>
    /// Draws the connections between the rooms
    /// </summary>
    private void DrawRoomConnections()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws the connection line between two nodes
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="childRoomNode"></param>
    private void DrawConnectionLine(RoomNodeSO roomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = roomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        Vector2 midPosition = (endPosition + startPosition) / 2f;
        Vector2 direction = endPosition - startPosition;

        Vector2 perpendicularDirection = new Vector2(-direction.y, direction.x).normalized / 2;

        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint1 = midPosition - perpendicularDirection * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + perpendicularDirection * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);
    }

    /// <summary>
    /// Draw the room nodes in the graph window
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeStyleSelected);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Selection changed in the inspector
    /// </summary>
    private void OnInspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

}
