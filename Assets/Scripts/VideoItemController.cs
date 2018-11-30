using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoItemController : MonoBehaviour {

    public delegate void VideoClick(VideoListItem panel);
    public static event VideoClick OnClicked;

    public VideoListItem video;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        OnClicked(this.video);
    }
}
