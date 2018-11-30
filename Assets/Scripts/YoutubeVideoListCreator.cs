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

    private string apiKey = "AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ";
    public int maxResults = 30;
    public Dictionary<string, List<VideoListItem>> CategoryVideos = new Dictionary<string, List<VideoListItem>>
    {
        { "music" , new List<VideoListItem>() },
        { "nature" , new List<VideoListItem>() },
        { "news" , new List<VideoListItem>() },
        { "comedy" , new List<VideoListItem>() },
        { "automotive" , new List<VideoListItem>() },
        { "cooking" , new List<VideoListItem>() },
        { "nba" , new List<VideoListItem>() },
        { "soccer" , new List<VideoListItem>() },
        { "nhl" , new List<VideoListItem>() },
        { "gaming" , new List<VideoListItem>() }
    };

    public Dictionary<string, string> ChannelIds = new Dictionary<string, string>
    {
        { "music" , "UC-9-kyTW8ZkZNDHQJ6FgpwQ"},
        { "nature" ,  "UC4UWbQ23IbWwU-XG7XIWxvQ" },
        { "news" , "UCuFFtHWoLl5fauMMD5Ww2jA" },
        { "comedy" , "UCwWhs_6x42TyRM4Wstoq8HA" },
        { "automotive" , "UCsAegdhiYLEoaFGuJFVrqFQ" },
        { "cooking" , "UCJFp8uSYCjXOMnkUyb3CQ3Q" },
        { "nba" , "UCWJ2lWNubArHWmf3FIHbfcQ" },
        { "soccer" , "UCOehRhbXyxRGsNO0nkLgZfA" },
        { "nhl" , "UCK3CHl-6e3hq4gQaz_TOyoQ" },
        { "gaming" , "UCbu2SsF-Or3Rsn3NxqODImw"}
    };

    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {

        foreach (string key in CategoryVideos.Keys)
        {
            Debug.Log(key);
            UnityWebRequest www = null;
            if (key == "music")
            {
                www = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId=PL55713C70BA91BD6E&maxResults=20&key=AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ");
            }
            else
            {
                www = UnityWebRequest.Get("https://www.googleapis.com/youtube/v3/search?q=" + key + "&type=video&maxResults=" + maxResults + "&part=snippet&channelId=" + ChannelIds[key] + "&key=AIzaSyCOu6VAoXIymLoI-5U5CWh3LFOAoVGXvIQ");
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
                if(key != "music")
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
                        if (videoItem.snippet.resourceId.kind != "youtube#video")
                        {
                            continue;
                        }
                        VideoListItem video = new VideoListItem
                        {
                            Title = videoItem.snippet.title,
                            Id = videoItem.snippet.resourceId.videoId,
                            ThumbnailUrl = videoItem.snippet.thumbnails.medium.url
                        };
                        Debug.Log(video.Title + " " + video.Id + " " + video.ThumbnailUrl);
                        CategoryVideos[key].Add(video);
                    }
                }
            }
        }
    }
}
