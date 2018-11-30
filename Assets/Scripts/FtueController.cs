using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FtueController : MonoBehaviour {
    public GameObject FtueGameObject;
    public NavigationController NavController;
    public GameObject MainUI;
    public NavObject startPanel;
    public Image FtueImage;
    public Image FtueIndicator;
    public Button StartButton;
    private Coroutine curCoroutine = null;
    private bool ftueClick = false;
    public MainUIController uiController;
    public SessionController sessionController;

    private Vector3 originalFtueLocation;
    private Vector3 originalMainUILocation;

	// Use this for initialization
	void Start () {
        SessionController.OnSessionReset += ResetFtue;
	}

    private void OnDestroy()
    {
        SessionController.OnSessionReset -= ResetFtue;
    }

    // Update is called once per frame
    void Update ()
    {
        if(!ftueClick)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ftueClick = true;
                OnStartClick();
            }
            else if (Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                ftueClick = true;
                OnStartClick();
            }
        }

    }

    private IEnumerator FadeInMainUI()
    {
        int rate = 2;
        float t = 0;

        originalMainUILocation = this.MainUI.transform.localPosition;
        Vector3 destination = new Vector3(originalMainUILocation.x, originalMainUILocation.y, 0);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.MainUI.transform.localPosition = Vector3.Lerp(originalMainUILocation, destination, t);

            yield return null;
        }
        NavController.ftueActive = false;
        FtueGameObject.SetActive(false);
        curCoroutine = null;
    }

    private IEnumerator FadeOutFtue()
    {
        int rate = 2;
        float t = 0;

        originalFtueLocation = this.FtueGameObject.transform.localPosition;
        Vector3 destination = new Vector3(originalFtueLocation.x, originalFtueLocation.y,-400);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.FtueGameObject.transform.localPosition = Vector3.Lerp(originalFtueLocation, destination, t);

            var c1 = StartButton.targetGraphic.color;
            StartButton.targetGraphic.color = new Color(c1.r, c1.g, c1.b, 1 - t * 0.75f);
            var c2 = FtueImage.color;
            FtueImage.color = new Color(c2.r, c2.g, c2.b, 1 - t);
            var c3 = FtueIndicator.color;
            FtueIndicator.color = new Color(c3.r, c3.g, c3.b, 1 - t);

            if(curCoroutine == null && t > 0.5)
            {
                MainUI.SetActive(true);
                curCoroutine = StartCoroutine(FadeInMainUI());
            }

            yield return null;
        }
        NavController.curPanel = startPanel;
        NavController.moveIndicator();
    }

    public void ResetFtue()
    {
        ftueClick = false;
        FtueGameObject.SetActive(true);
        this.FtueGameObject.transform.localPosition = originalFtueLocation;
        this.MainUI.transform.localPosition = originalMainUILocation;

        var c1 = StartButton.targetGraphic.color;
        StartButton.targetGraphic.color = new Color(c1.r, c1.g, c1.b, 1);
        var c2 = FtueImage.color;
        FtueImage.color = new Color(c2.r, c2.g, c2.b, 1);
        var c3 = FtueIndicator.color;
        FtueIndicator.color = new Color(c3.r, c3.g, c3.b, 1);
    }

    public void OnStartClick()
    {
        SessionData.SessionId = System.DateTime.UtcNow.Ticks;
        Firebase.Analytics.FirebaseAnalytics.LogEvent("session", "sessionStart", "sessionId="+SessionData.SessionId);
        sessionController.waitingForNextSession = false;
        sessionController.vuforiaBehaviour.enabled = true;
        uiController.SleepTimer.Start();
        StartCoroutine(FadeOutFtue());
    }
}
