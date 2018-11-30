using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Audio;


public class MediaController : MonoBehaviour {

    public SimplePlayback SimplePlayback;
    public GameObject loadingImage;
    public GameObject loadingIndicator;
    public AudioSource videoPlayer;
    private Coroutine curCoroutine;
    public MainUIController uiController;
    public ListController listController;
    private string startingVidId;
    public bool disableVideoOptions = true;

    private int qSamples = 1024;  // array size
    private float refValue = 0.1f; // RMS value for 0 dB
    private float rmsValue = 0;   // sound level - RMS
    private float dbValue = 0;    // sound level - dB
    private float volume = 2; // set how much the scale will vary
    private float[] samples; // audio samples'

    public bool videoReady = false;
    public bool isActuallyPlaying = false;

    // Use this for initialization
    void Start () {
        VideoItemController.OnClicked += OnVideoClick;
        samples = new float[qSamples];
        SimplePlayback.OnReady += VideoPrepared;
        startingVidId = SimplePlayback.videoId;
        SessionController.OnSessionReset += ResetSimplePlayback;
    }

    public void ResetSimplePlayback()
    {
        SimplePlayback.videoId = startingVidId;
    }

    private void OnEnable()
    {
        disableVideoOptions = true;
        SimplePlayback.videoId = "PMVAi1j9TQ0";
        SimplePlayback.PlayYoutubeVideo("PMVAi1j9TQ0");
        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }
        curCoroutine = StartCoroutine(VideoLoading());
    }

    private void VideoPrepared()
    {
        videoReady = true;
    }

    private void Update()
    {
        if(videoReady && loadingImage.activeInHierarchy)
        {
            GetVolume();
            if (dbValue > -160)
            {
                VideoReady();
            }
        }
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
        videoReady = false;
    }

    public void OnVideoClick(VideoListItem video)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("navigation", listController.curCategoryPanel.categoryId, "vidCount=" + SessionData.VidCount + " sessionId=" + SessionData.SessionId);
        if (video != null && video.Id != string.Empty)
        {
            isActuallyPlaying = false;
            disableVideoOptions = true;
            SimplePlayback.Play_Pause();
            SimplePlayback.videoId = video.Id;
            SimplePlayback.PlayYoutubeVideo(video.Id);
            if (curCoroutine != null)
            {
                StopCoroutine(curCoroutine);
            }
            curCoroutine = StartCoroutine(VideoLoading());
        }
    }

    public void PlayPauseVideo()
    {
        SimplePlayback.Play_Pause();
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
