﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SessionData {

    public static long SessionId;

    public static double SessionDuration;

    public static int VidCount;

    public static List<int> HeadsetStatus = new List<int>();

    public static List<int> ControllerStatus = new List<int>();

    public static System.DateTime SessionStart;

    public static System.DateTime SessionEnd;
}
