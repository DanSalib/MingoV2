using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class YoutubeResponseData
{
    public string kind;
    public string etag;
    public string nextPageToken;
    public PageInfoItem pageInfo;
    public List<VideoItem> items = new List<VideoItem>();
}

[System.Serializable]
public class VideoItem
{
    public string kind;
    public string etag;
    public IdInfo id;
    public SnippetInfo snippet;
}

[System.Serializable]
public class IdInfo
{
    public string kind;
    public string videoId;
}

[System.Serializable]
public class SnippetInfo
{
    public string publishedAt;
    public string channelId;
    public string title;
    public string description;
    public ThumbnailInfo thumbnails;
    public string channelTitle;
    public string[] tags;
    public string categoryId;
    public string liveBroadcastContent;
    public LocalizedData localized;
    public string defaultAudioLanguage;
}

[System.Serializable]
public class ThumbnailInfo
{
    public ThumbnailInfoItem _default;
    public ThumbnailInfoItem medium;
    public ThumbnailInfoItem high;
    public ThumbnailInfoItem standard;
    public ThumbnailInfoItem maxres;
}

[System.Serializable]
public class LocalizedData
{
    public string title;
    public string description;
}


[System.Serializable]
public class ThumbnailInfoItem
{
    public string url;
    public int width;
    public int height;
}

[System.Serializable]
public class PageInfoItem
{
    public int totalResults;
    public int resultsPerPage;
}

