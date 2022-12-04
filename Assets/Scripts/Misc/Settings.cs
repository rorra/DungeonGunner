using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttempsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    public const int maxChildCorridors = 3; // Max number of child corridors a room can have
    #endregion ROOM SETTINGS
}
