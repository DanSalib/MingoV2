using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class YoutubePlaylistResponseData
{
    public string kind;
    public string etag;
    public string nextPageToken;
    public PlaylistPageInfoItem pageInfo;
    public List<PlaylistItem> items = new List<PlaylistItem>();
}

[System.Serializable]
public class PlaylistItem
{
    public string kind;
    public string etag;
    public string id;
    public PlaylistSnippetInfo snippet;
}

[System.Serializable]
public class PlaylistSnippetInfo
{
    public string publishedAt;
    public string channelId;
    public string title;
    public string description;
    public PlaylistThumbnailInfo thumbnails;
    public string channelTitle;
    public string playlistId;
    public int position;
    public ResourceIdItem resourceId;
}

[System.Serializable]
public class PlaylistThumbnailInfo
{
    public PlaylistThumbnailInfoItem _default;
    public PlaylistThumbnailInfoItem medium;
    public PlaylistThumbnailInfoItem high;
    public PlaylistThumbnailInfoItem standard;
    public PlaylistThumbnailInfoItem maxres;
}

[System.Serializable]
public class PlaylistLocalizedData
{
    public string title;
    public string description;
}


[System.Serializable]
public class PlaylistThumbnailInfoItem
{
    public string url;
    public int width;
    public int height;
}

[System.Serializable]
public class PlaylistPageInfoItem
{
    public int totalResults;
    public int resultsPerPage;
}

[System.Serializable]
public class ResourceIdItem
{
    public string kind;
    public string videoId;
}

