using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ListController : MonoBehaviour {

    public GameObject SideBar;

    public PanelController curCategoryPanel;

    public YoutubeVideoListCreator VideoListCreator;

    public VideoListItem[] VideoList;

    // Use this for initialization
    void Start () {
        PanelController.OnClicked += RefreshList;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RefreshList(PanelController panel)
    {
        curCategoryPanel = panel;
        int i = 0;
        foreach (VideoListItem vid in VideoListCreator.CategoryVideos[panel.categoryId])
        {
            if (i < VideoList.Length)
            {
                VideoList[i].Title = vid.Title;
                VideoList[i].Id = vid.Id;
                VideoList[i].ThumbnailUrl = vid.ThumbnailUrl;
                VideoList[i].VideoTitle.text = vid.Title;
                if (VideoListCreator.CategoryIdToThumbnails[panel.categoryId].ContainsKey(vid.Id))
                {
                    VideoList[i].Thumbnail.texture = VideoListCreator.CategoryIdToThumbnails[panel.categoryId][vid.Id];
                }
                i++;
            }
        }
    }

    private void OnDestroy()
    {
        PanelController.OnClicked -= RefreshList;
    }
}
