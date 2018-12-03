using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOptionsController : MonoBehaviour {

    public MediaController mediaController;
    public GameObject volControls;
    public GameObject volSlider;
    public GameObject background;
    public AudioSource audioSource;
    private Coroutine curCoroutine;

    public Image muteImage;
    public Image unmuteImage;
    public Image playImage;
    public Image pauseImage;

    public Button muteButton;
    public Button playButton;

    private bool muted = false;
    private bool paused = false;
    private float prevSliderX;
    private float prevVol;

    public delegate void Play();
    public static event Play OnPlay;

    // Use this for initialization
    void Start () {
        SessionController.OnSessionReset += ResetVideoOptions;
    }

    public void ResetVideoOptions()
    {
        if (paused)
        {
            PlayPause();
        }

        if (muted)
        {
            MuteUnmute();
        }

        if (audioSource.volume < 0.4f)
        {
            while (audioSource.volume < 0.39)
            {
                VolUp();
            }
        }
        else if (audioSource.volume > 0.4f)
        {
            while (audioSource.volume > 0.41)
            {
                VolDown();
            }
        }
        HideVideoOptions();
    }

    public void ShowVideoOptions()
    {
        curCoroutine = StartCoroutine(ShowOptionsBar());
    }

    public void HideVideoOptions()
    {
        if(curCoroutine == null)
        {
            this.volControls.SetActive(false);
            curCoroutine = StartCoroutine(HideOptionsBar());
        }
    }

    private IEnumerator ShowOptionsBar()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.background.transform.localPosition;
        Vector3 originalScale = this.background.transform.localScale;
        Vector3 destination = new Vector3(originalLocation.x, -150, originalLocation.z);
        Vector3 destinationScale = new Vector3(originalScale.x, 1, originalScale.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.background.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);
            this.background.transform.localScale = Vector3.Lerp(originalScale, destinationScale, t);

            yield return null;
        }
        this.volControls.SetActive(true);
        curCoroutine = null;
    }

    private IEnumerator HideOptionsBar()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.background.transform.localPosition;
        Vector3 originalScale = this.background.transform.localScale;
        Vector3 destination = new Vector3(originalLocation.x, -175, originalLocation.z);
        Vector3 destinationScale = new Vector3(originalScale.x, 0.1f, originalScale.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.background.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);
            this.background.transform.localScale = Vector3.Lerp(originalScale, destinationScale, t);

            yield return null;
        }
        curCoroutine = null;
    }

    private IEnumerator ChangeVolume(bool volUp)
    {
        int rate = 6;
        float t = 0;

        Vector3 originalLocation = this.volSlider.transform.localPosition;
        var sign = volUp ? 1 : -1;
        var mag = (!volUp && originalLocation.x <= -357)
            || (volUp && originalLocation.x >= -263) 
            ? 0f : 18;

        audioSource.volume += sign * mag * (0.14f / 18f);

        Vector3 destination = new Vector3((sign * mag) + originalLocation.x, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.volSlider.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            yield return null;
        }
    }

    public void VolUp()
    {
        if(muted)
        {
            MuteUnmute();
        }
        StartCoroutine(ChangeVolume(true));
    }

    public void VolDown()
    {
        if(muted)
        {
            MuteUnmute();
        }
        StartCoroutine(ChangeVolume(false));
    }

    public void MuteUnmute()
    {
        Vector3 originalLocation = this.volSlider.transform.localPosition;
        if (!muted)
        {
            prevSliderX = originalLocation.x;
            prevVol = audioSource.volume;
            muteImage.gameObject.SetActive(false);
            unmuteImage.gameObject.SetActive(true);
            muteButton.image = unmuteImage;
            volSlider.transform.localPosition = new Vector3(-370, originalLocation.y, originalLocation.z);
            audioSource.volume = 0.01f;
        }
        else
        {
            volSlider.transform.localPosition = new Vector3(prevSliderX, originalLocation.y, originalLocation.z);
            audioSource.volume = prevVol;
            muteImage.gameObject.SetActive(true);
            unmuteImage.gameObject.SetActive(false);
            muteButton.image = muteImage;
        }
        muted = !muted;
    }

    public void PlayPause()
    {
        if (!paused)
        {
            playImage.gameObject.SetActive(true);
            pauseImage.gameObject.SetActive(false);
            playButton.image = playImage;
        }
        else
        {
            OnPlay();
            playImage.gameObject.SetActive(false);
            pauseImage.gameObject.SetActive(true);
            playButton.image = pauseImage;
        }
        paused = !paused;
        mediaController.PlayPauseVideo();
    }

    public void SetPlay()
    {
        playImage.gameObject.SetActive(false);
        pauseImage.gameObject.SetActive(true);
        playButton.image = pauseImage;
        paused = false;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
