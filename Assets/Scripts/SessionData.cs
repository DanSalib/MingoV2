using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SessionData {

    public static long SessionId;

    public static double SessionDuration;

    public static int VidCount;

    public static List<int> HeadsetStatus = new List<int>();

    public static List<int> ControllerStatus = new List<int>();

    public static System.DateTime SessionStart;

    public static System.DateTime SessionEnd;

    public static List<CategoryLog> CategoryLogs = new List<CategoryLog>();

}

[Serializable]
public class CategoryLog
{
    public string Category;

    public string LogTime;
}
