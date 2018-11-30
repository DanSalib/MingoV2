using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class MainUIController : MonoBehaviour {

    public GameObject MainUI;

    public GameObject MediaUI;

    public GameObject VideoListUI;

    public GameObject CategoryUI;

    public ListController ListController;
    
    public NavigationController NavController;

    public MediaController mediaController;

    public delegate void PanelsLoaded(PanelController panel);
    public static event PanelsLoaded OnLoaded;

    private bool firstTime = true;

    public Stopwatch SleepTimer = new Stopwatch();

    public Stopwatch VidOptionsTimer = new Stopwatch();

    long duration = 2000;

    public bool disableButtons = false;
    public bool isSleeping = false;

    // Use this for initialization
    void Start ()
    {
        SleepTimer = new Stopwatch();
        PanelController.OnClicked += ShowListUI;
        VideoItemController.OnClicked += HideListUI;
        NavigationController.OnKeyPress += ActivateUI;
        SessionController.OnSessionReset += ResetUI;
    }

    private void OnDestroy()
    {
        PanelController.OnClicked -= ShowListUI;
        VideoItemController.OnClicked -= HideListUI;
        NavigationController.OnKeyPress -= ActivateUI;
    }
    // Update is called once per frame
    void Update () {
        if (firstTime && CategoryUI.activeInHierarchy && SleepTimer?.ElapsedMilliseconds > 7000f)
        {
            firstTime = false;
            NavController.indicator.SetActive(false);
            StartCoroutine(CenterVideo());
            StartCoroutine(FadeCategories());
            isSleeping = true;
            SleepTimer.Stop();
            SleepTimer.Reset();
        }
        else if (!firstTime && CategoryUI.activeInHierarchy && SleepTimer?.ElapsedMilliseconds > 3500f)
        {
            NavController.indicator.SetActive(false);
            StartCoroutine(CenterVideo());
            StartCoroutine(FadeCategories());
            StartCoroutine(FadeCategories());
            isSleeping = true;
            SleepTimer.Stop();
            SleepTimer.Reset();
        }

        if(VidOptionsTimer.ElapsedMilliseconds > 2000f)
        {
            HideVideoOptions();
        }
	}

    public void ResetUI()
    {
        ActivateUI(NavigationController.directions.click);
        firstTime = true;
        SleepTimer.Reset();
        disableButtons = true;
        NavController.prevPanel = null;
        NavController.curPanel = ListController?.curCategoryPanel?.gameObject?.GetComponent<NavObject>();
        NavController.indicator.transform.localScale = new Vector3(1.5f, 1.5f);
        StartCoroutine(HideVideos());
        StartCoroutine(ShowCategoriesAndDisableMain());
    }

    public void ActivateUI(NavigationController.directions d)
    {
        if(CategoryUI.activeInHierarchy)
        {
            SleepTimer?.Reset();
            SleepTimer?.Start();
            StartCoroutine(MoveVidToTop());
            StartCoroutine(UnfadeCategories());
        }
    }

    public void TeaseVideoOptions()
    {
        NavController.ShowVideoOptions();
        VidOptionsTimer?.Reset();
        VidOptionsTimer?.Start();
    }

    public void HideVideoOptions()
    {
        if (NavController.curPanel != null && !NavController.curPanel.isVideo)
        {
            NavController.HideOptions();
        }
        VidOptionsTimer?.Reset();
        VidOptionsTimer?.Stop();
    }

    public void ShowListUI(PanelController panel)
    {
        SleepTimer?.Stop();
        SleepTimer?.Reset();
        disableButtons = true;
        NavController.curPanel = ListController.VideoList[0].gameObject.GetComponent<NavObject>();
        VideoListUI.SetActive(true);
        StartCoroutine(ShowVideos());
        NavController.indicator.transform.localScale = new Vector3(1.95f, 2.4f);
        StartCoroutine(HideCategories());
    }

    public void HideListUI(VideoListItem vid)
    {
        disableButtons = true;
        NavController.prevPanel = null;
        NavController.curPanel = ListController?.curCategoryPanel?.gameObject?.GetComponent<NavObject>();
        NavController.indicator.transform.localScale = new Vector3(1.5f, 1.5f);
        StartCoroutine(HideVideos());
        if(vid != null)
        {
            StartCoroutine(ShowFadedCategories());
            StartCoroutine(CenterVideo());
        }
        else
        {
            StartCoroutine(ShowCategories());
            SleepTimer?.Reset();
            SleepTimer?.Start();
        }

    }

    private IEnumerator CenterVideo()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.MediaUI.transform.localPosition;
        Button[] buttons = this.MediaUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(originalLocation.x, -160f, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.MediaUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            yield return null;
        }
        disableButtons = false;
        NavController.indicator.SetActive(false);
    }

    private IEnumerator MoveVidToTop()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.MediaUI.transform.localPosition;
        Button[] buttons = this.MediaUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(originalLocation.x, 0, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.MediaUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            yield return null;
        }
    }

    private IEnumerator HideCategories()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.CategoryUI.transform.localPosition;
        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(-820, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.CategoryUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, 1 - t);
            }

            yield return null;
        }
        CategoryUI.SetActive(false);
        NavController.moveIndicator();
        disableButtons = false;
    }

    private IEnumerator HideVideos()
    {
        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.VideoListUI.transform.localPosition;
        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(1800, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.VideoListUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            yield return null;
        }
        VideoListUI.SetActive(false);
        NavController.moveIndicator();
    }

    private IEnumerator ShowVideos()
    {
        var curPos = NavController.viewPort.transform.localPosition;
        NavController.viewPort.transform.localPosition = new Vector3(curPos.x, 24.6f, curPos.z);

        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.VideoListUI.transform.localPosition;
        Vector3 destination = new Vector3(650, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.VideoListUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            yield return null;
        }
        NavController.moveIndicator();
    }

    private IEnumerator ShowCategories()
    {
        CategoryUI.SetActive(true);

        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.CategoryUI.transform.localPosition;
        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(-4, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

           this.CategoryUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, t);
            }
            yield return null;
        }
        disableButtons = false;
    }


    private IEnumerator ShowCategoriesAndDisableMain()
    {
        CategoryUI.SetActive(true);

        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.CategoryUI.transform.localPosition;
        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(-4, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.CategoryUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, t);
            }
            yield return null;
        }
        disableButtons = false;
        MainUI.SetActive(false);
    }

    private IEnumerator UnfadeCategories()
    {
        CategoryUI.SetActive(true);

        int rate = 4;
        float t = 0;

        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();

        while (t < 0.75)
        {
            t += Time.deltaTime * rate;

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, c.a + t);
            }
            yield return null;
        }
        if (isSleeping && mediaController.isActuallyPlaying)
        {
            TeaseVideoOptions();
        }
        isSleeping = false;
        NavController.moveIndicator();
    }

    private IEnumerator ShowFadedCategories()
    {
        CategoryUI.SetActive(true);

        int rate = 4;
        float t = 0;

        Vector3 originalLocation = this.CategoryUI.transform.localPosition;
        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        Vector3 destination = new Vector3(-4, originalLocation.y, originalLocation.z);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.CategoryUI.transform.localPosition = Vector3.Lerp(originalLocation, destination, t);

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, t*0.25f);
            }
            yield return null;
        }
        isSleeping = true;
        disableButtons = false;
    }

    private IEnumerator FadeCategories()
    {
        CategoryUI.SetActive(true);

        int rate = 4;
        float t = 0;

        Button[] buttons = this.CategoryUI.GetComponentsInChildren<Button>();
        while (t < 1)
        {
            t += Time.deltaTime * rate;

            foreach (var button in buttons)
            {
                var c = button.targetGraphic.color;
                button.targetGraphic.color = new Color(c.r, c.g, c.b, t * 0.25f);
            }
            yield return null;
        }
    }
}
