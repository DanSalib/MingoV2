using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Audio;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;


public class MediaController : MonoBehaviour {
    public NotificationController NotificationController;
    public YoutubePlayer IntermissionPlayer;
    public YoutubePlayer Player;
    public GameObject loadingImage;
    public GameObject loadingIndicator;
    public AudioSource videoPlayer;
    private Coroutine curCoroutine;
    public MainUIController uiController;
    public ListController listController;
    private string startingVidId;
    public bool disableVideoOptions = true;
    public bool videoReady = false;
    public bool firstTime = true;

    private string youtubeUrl = "https://www.youtube.com/watch?v=";

    private int qSamples = 1024;  // array size
    private float refValue = 0.1f; // RMS value for 0 dB
    private float rmsValue = 0;   // sound level - RMS
    private float dbValue = 0;    // sound level - dB
    private float volume = 2; // set how much the scale will vary
    private float[] samples; // audio samples'
    public bool isActuallyPlaying = false;

    // Use this for initialization
    void Start () {
        VideoItemController.OnClicked += OnVideoClick;
        samples = new float[qSamples];
        startingVidId = "cUrboDG8zYg";
        SessionController.OnSessionReset += ResetSimplePlayback;
    }

    public void ResetSimplePlayback()
    {
        Player.Pause();
    }

    public void ReplayVid()
    {
        Player.Pause();
        Player.LoadYoutubeVideo(youtubeUrl + startingVidId);

        disableVideoOptions = true;

        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }

        curCoroutine = StartCoroutine(VideoLoading());
    }

    private void OnEnable()
    {
        if(!firstTime)
        {
            Player.Pause();
            Player.LoadYoutubeVideo(youtubeUrl + startingVidId);
        }
        else
        {
            firstTime = false;
        }
        
        disableVideoOptions = true;
        IntermissionPlayer.gameObject.SetActive(true);
        if(SessionData.IntermissionVideoIds.Count > 0)
        {
            IntermissionPlayer.LoadYoutubeVideo(youtubeUrl + SessionData.IntermissionVideoIds[0].VideoId);
            IntermissionPlayer.startFromSecondTime = SessionData.IntermissionVideoIds[0].VideoStartTime;
        }
        IntermissionPlayer.gameObject.SetActive(false);

        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }
        
        curCoroutine = StartCoroutine(VideoLoading());
    }

    private void Update()
    {
      //  if(videoReady && loadingImage.activeInHierarchy)
       // {
           // GetVolume();
       //     if (dbValue > -160)
       ///     {
       //         VideoReady();
         //   }
       // }
    }

    private void GetVolume()
    {
        videoPlayer.GetOutputData(samples, 0); // fill array with samples
        float sum = 0;
        for (int i = 0; i < qSamples; i++)
        {
            sum += samples[i] * samples[i]; // sum squared samples
        }
        rmsValue = Mathf.Sqrt(sum / qSamples); // rms = square root of average
        dbValue = 20 * Mathf.Log10(rmsValue / refValue); // calculate dB
        if (dbValue < -160) dbValue = -160; // clamp it to -160dB min
    }

    private void VideoReady()
    {
        SessionData.VidCount++;
        StopCoroutine(curCoroutine);
        loadingImage.SetActive(false);
        uiController.TeaseVideoOptions();
        isActuallyPlaying = true;
        disableVideoOptions = false;
        //videoReady = false;
    }

    public void SetVideoReady()
    {
        if(Player.isActiveAndEnabled)
        {
            SessionData.VidCount++;
        }
        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }
        loadingImage.SetActive(false);
        uiController.TeaseVideoOptions();
        isActuallyPlaying = true;
        disableVideoOptions = false;
        // videoReady = true;
    }

    public bool ShouldIntermission()
    {
        return SessionData.VidCount > 1 && (SessionData.VidCount - 1) % 3 == 0;
    }

    public IntermissionVideoInfo GetNextIntermissionVideo()
    {
        int vidIndex = (SessionData.VidCount - 1) / 3;
        return SessionData.IntermissionVideoIds[vidIndex % (Mathf.Max(1, SessionData.IntermissionVideoIds.Count))];
    }

    public void OnVideoClick(VideoListItem video)
    {
        if (video != null && video.Id != string.Empty)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent("navigation", listController.curCategoryPanel.categoryId, SessionData.VidCount);
            SessionData.CategoryLogs.Add(new CategoryLog { Category = listController.curCategoryPanel.categoryId, LogTime = System.DateTime.UtcNow.ToLongTimeString() });
            isActuallyPlaying = false;
            disableVideoOptions = true;
            Player.Pause();
            Player.LoadYoutubeVideo(youtubeUrl + video.Id);
            if (curCoroutine != null)
            {
                StopCoroutine(curCoroutine);
            }
            curCoroutine = StartCoroutine(VideoLoading());
        }
        if(ShouldIntermission() && SessionData.IntermissionVideoIds.Count > 0)
        {
            NotificationController.ShowNotification("A hygiene tip from your dentist...");
            IntermissionPlayer.gameObject.SetActive(true);
            IntermissionPlayer.LoadYoutubeVideo(IntermissionPlayer.youtubeUrl);
            MatchIntermissionToPlayerAudio();
            Player.gameObject.SetActive(false);
        }
    }

    public void PlayPauseVideo()
    {
        if (IntermissionPlayer.isActiveAndEnabled)
        {
            IntermissionPlayer.PlayPause();
        } else
        {
            Player.PlayPause();
        }

    }

    private void MatchPlayerToIntermissionAudio()
    {
        Player.GetComponent<AudioSource>().volume = IntermissionPlayer.GetComponent<AudioSource>().volume;
    }

    private void MatchIntermissionToPlayerAudio()
    {
        IntermissionPlayer.GetComponent<AudioSource>().volume = Player.GetComponent<AudioSource>().volume;
    }

    public void OnIntermissionFinish()
    {
        Player.gameObject.SetActive(true);
        var intermissionVideo = GetNextIntermissionVideo();
        IntermissionPlayer.LoadYoutubeVideo(youtubeUrl + intermissionVideo.VideoId);
        IntermissionPlayer.startFromSecondTime = intermissionVideo.VideoStartTime;
        MatchPlayerToIntermissionAudio();
        IntermissionPlayer.gameObject.SetActive(false);
        isActuallyPlaying = false;
        disableVideoOptions = true;
        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }
        curCoroutine = StartCoroutine(VideoLoading());
    }

    private IEnumerator VideoLoading()
    {
        loadingImage.SetActive(true);
        loadingIndicator.SetActive(true);
        while (true)
        {
            loadingIndicator.transform.Rotate(Vector3.forward,-2f);
            yield return null;
        }
    }

    private string ParseVideoId(string url)
    {
        UnityEngine.Debug.Log(url.Substring(url.Length - 11, 11));

        return url.Substring(url.Length - 11, 11);
    }

    private void OnDestroy()
    {
        VideoItemController.OnClicked -= OnVideoClick;
        SessionController.OnSessionReset -= ResetSimplePlayback;
    }
}
