using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;

public class YoutubeVideoListCreator : MonoBehaviour
{
    public delegate void LoadCategory(string categoryId);
    public static event LoadCategory OnLoad;

    public ListController listController;
    private string apiKey = "AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ";
    public int maxResults = 32;
    public Dictionary<string, List<VideoListItem>> CategoryVideos = new Dictionary<string, List<VideoListItem>>
    {
        { "music" , new List<VideoListItem>() },
        { "nature" , new List<VideoListItem>() },
        { "news" , new List<VideoListItem>() },
        { "comedy" , new List<VideoListItem>() },
        { "automotive" , new List<VideoListItem>() },
        { "kids" , new List<VideoListItem>() },
        { "nba" , new List<VideoListItem>() },
        { "soccer" , new List<VideoListItem>() },
        { "nhl" , new List<VideoListItem>() },
        { "gaming" , new List<VideoListItem>() }
    };

    public Dictionary<string, string> ChannelIds = new Dictionary<string, string>
    {
        { "music" , "PLFgquLnL59alW3xmYiWRaoz0oM3H17Lth"},
        { "nature" ,  "PLdhB2hC90YEtAgR37NjCIqOcZI3QWGvR3" },
        { "news" , "PLS3XGZxi7cBVNadbxDqZCUgISvabEpu-g" },
        { "comedy" , "PLoXkGkpREHNBerh-2Ql6R5GqRk-Hz20O_" },
        { "automotive" , "PLMQrDNsc92hcP_j58H8XcO38UZVQld8Hz" },
        { "kids" , "UCGydrkfIhUDNCotYQI8TJhA" },
        { "nba" , "PL5j8RirTTnK5rfAPFJFwaqJvLweQynhjq" },
        { "soccer" , "PLVWqRc88TLrCvtzi-2HT1CJMtChnfIvUk" },
        { "nhl" , "PLo12SYwt93SQ4e-J9mANGoKTjQYbD3i04" },
        { "gaming" , "PLraFbwCoisJBTl0oXn8UoUam5HXWUZ7ES"}
    };
    
    public Dictionary<string, Dictionary<string, Texture>> CategoryIdToThumbnails = new Dictionary<string, Dictionary<string, Texture>>
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
        { "kids", new Dictionary<string, Texture>() }
    };

    void Start()
    {
        StartCoroutine(GetText());
    }

    public void RefreshList()
    {
        foreach (var key in CategoryVideos.Keys)
        {
            foreach (VideoListItem vid in CategoryVideos[key])
            {
                StartCoroutine(GetImage(vid, key));
            }
        }
    }


    IEnumerator GetImage(VideoListItem listItem, string categoryId)
    {
         if (listItem?.ThumbnailUrl != null)
         {
             UnityWebRequest www = UnityWebRequestTexture.GetTexture(listItem?.ThumbnailUrl);
             yield return www.Send();

             if (www.isNetworkError)
             {
                 Debug.Log(www.error);
             }
             else
             {
                 CategoryIdToThumbnails[categoryId].Add(listItem.Id, ((DownloadHandlerTexture)www.downloadHandler)?.texture);
             }
         }
        yield return null;
    }

    IEnumerator GetText()
    {

        foreach (string key in CategoryVideos.Keys)
        {
            Debug.Log(key);
            UnityWebRequest www = null;
            if (ChannelIds[key][0] == 'P')
            {
                www = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=" + ChannelIds[key]+ "&maxResults=" + maxResults + "&videoDefinition=standard&key=AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ");
            }
            else
            {
                www = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search?type=video&maxResults=" + maxResults + "&part=snippet&channelId=" + ChannelIds[key] + "&videoDefinition=standard&key=AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ");
            }
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonData = "";
                jsonData = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data, 3, www.downloadHandler.data.Length - 3);  // Skip thr first 3 bytes (i.e. the UTF8 BOM)
                JSONObject json = new JSONObject(jsonData);
                // Or retrieve results as binary data
                //Debug.Log(www.downloadHandler.text);
                if (ChannelIds[key][0] != 'P')
                {
                    YoutubeResponseData responseData = JsonUtility.FromJson<YoutubeResponseData>(www.downloadHandler.text);
                    foreach (var videoItem in responseData.items)
                    {
                        if (videoItem.id.kind != "youtube#video")
                        {
                            continue;
                        }
                        VideoListItem video = new VideoListItem
                        {
                            Title = videoItem.snippet.title,
                            Id = videoItem.id.videoId,
                            ThumbnailUrl = videoItem.snippet.thumbnails.medium.url
                        };
                        Debug.Log(video.Title + " " + video.Id + " " + video.ThumbnailUrl);
                        CategoryVideos[key].Add(video);
                    }
                }
                else
                {
                    YoutubePlaylistResponseData responseData = JsonUtility.FromJson<YoutubePlaylistResponseData>(www.downloadHandler.text);
                    foreach (var videoItem in responseData.items)
                    {
                        if (videoItem.snippet.resourceId.kind != "youtube#video" || videoItem.snippet.title == "Private video")
                        {
                            continue;
                        }
                        VideoListItem video = new VideoListItem
                        {
                            Title = videoItem.snippet.title,
                            Id = videoItem.snippet.resourceId.videoId,
                            ThumbnailUrl = videoItem.snippet.thumbnails?.medium?.url ?? videoItem.snippet.thumbnails?.standard?.url
                        };
                       // Debug.Log(video.Title + " " + video.Id + " " + video.ThumbnailUrl);
                        CategoryVideos[key].Add(video);
                    }
                }
            }
        }
        RefreshList();
    }
}
