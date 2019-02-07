using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntermissionVideoInfo : MonoBehaviour {

    public IntermissionVideoInfo(string id, int startTime)
    {
        VideoId = id;
        VideoStartTime = startTime;
    }

    public string VideoId { get; set; }

    public int VideoStartTime { get; set; }
}
