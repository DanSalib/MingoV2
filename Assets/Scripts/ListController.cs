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

    private Dictionary<string, Dictionary<string, Texture>> CategoryIdToThumbnails = new Dictionary<string, Dictionary<string, Texture>>
    {
        { "music" , new Dictionary<string, Texture>() },
        { "nature", new Dictionary<string, Texture>() },
        { "automotive", new Dictionary<string, Texture>() },
        { "comedy", new Dictionary<string, Texture>() },
        { "news", new Dictionary<string, Texture>() },
        { "nba", new Dictionary<string, Texture>() },
        { "soccer", new Dictionary<string, Texture>()},
        { "nhl", new Dictionary<string, Texture>() },
        { "gaming", new Dictionary<string, Texture>() },
        { "cooking", new Dictionary<string, Texture>() }
    };

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
            VideoList[i].Title = vid.Title;
            VideoList[i].Id = vid.Id;
            VideoList[i].ThumbnailUrl = vid.ThumbnailUrl;
            VideoList[i].VideoTitle.text = vid.Title;
            if(CategoryIdToThumbnails[panel.categoryId].ContainsKey(vid.Id))
            {
                VideoList[i].Thumbnail.texture = CategoryIdToThumbnails[panel.categoryId][vid.Id];
            }
            else
            {
                StartCoroutine(GetImage(VideoList[i], panel.categoryId));
            }
            i++;
        }
    }

    IEnumerator GetImage(VideoListItem listItem, string categoryId)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(listItem.ThumbnailUrl);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            listItem.Thumbnail.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            CategoryIdToThumbnails[categoryId].Add(listItem.Id, listItem.Thumbnail.texture);
        }
    }

    private void OnDestroy()
    {
        PanelController.OnClicked -= RefreshList;
    }
}
