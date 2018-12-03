using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Diagnostics;

public class FtueController : MonoBehaviour {
    public GameObject FtueGameObject;
    public NavigationController NavController;
    public GameObject MainUI;
    public NavObject startPanel;

    private Coroutine curCoroutine = null;
    public MainUIController uiController;
    public SessionController sessionController;

    private Vector3 originalFtueLocation;
    private Vector3 originalMainUILocation;
    private Vector3 originalFirstStepLocation;
    private Vector3 originalSecondStepLocation;
    private Vector3 originalThirdStepLocation;
    private Vector3 originalFourthStepLocation;


    public Image LastStepImage;
    public GameObject firstStep;
    public GameObject secondStep;
    public GameObject thirdStep;
    public GameObject fourthStep;
    public GameObject secondStepCheck;
    public GameObject thirdStepCheck;
    public Text countDownText;
    private int stepCount = 0;

    private Stopwatch sessionCountDown = new Stopwatch();
    private Stopwatch ftueTimer = new Stopwatch();


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
        if(stepCount == 0)
        {
            if (curCoroutine == null && Input.GetKeyDown(KeyCode.Space))
            {
                curCoroutine = StartCoroutine(TransitionToSecondStep());
            }
            else if (curCoroutine == null && Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                curCoroutine = StartCoroutine(TransitionToSecondStep());
                Firebase.Analytics.FirebaseAnalytics.LogEvent("walkthrough", "firstStepComplete", 0);
            }
        }
        else if (stepCount == 1)
        {
            if (curCoroutine == null && Input.GetAxisRaw("Horizontal") == -1)
            {
                curCoroutine = StartCoroutine(TransitionToThirdStep());
                Firebase.Analytics.FirebaseAnalytics.LogEvent("walkthrough", "secondStepComplete", ftueTimer.ElapsedMilliseconds);
            }
        }
        else if (stepCount == 2)
        {
            if (curCoroutine == null && Input.GetAxisRaw("Horizontal") == 1)
            {
                curCoroutine = StartCoroutine(TransitionToFourthStep());
                Firebase.Analytics.FirebaseAnalytics.LogEvent("walkthrough", "thirdStepComplete", ftueTimer.ElapsedMilliseconds);
            }
        }
        else if (stepCount == 3)
        {
            countDownText.text = (3 - Mathf.Floor(sessionCountDown.ElapsedMilliseconds / 1000f)).ToString() + "..";
            if (sessionCountDown.ElapsedMilliseconds > 3000)
            {
                countDownText.text = "0";
                curCoroutine = null;
                StartCoroutine(FadeOutFtue());
                Firebase.Analytics.FirebaseAnalytics.LogEvent("walkthrough", "fourthStepComplete", ftueTimer.ElapsedMilliseconds);
                OnStartClick();
                ftueTimer.Reset();
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

        Vector3 destination = new Vector3(originalFtueLocation.x, originalFtueLocation.y,-400);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.FtueGameObject.transform.localPosition = Vector3.Lerp(originalFtueLocation, destination, t);

            var c2 = LastStepImage.color;
            LastStepImage.color = new Color(c2.r, c2.g, c2.b, 1 - t);
            var c3 = countDownText.color;
            countDownText.color = new Color(c3.r, c3.g, c3.b, 1 - t);

            if (curCoroutine == null && t > 0.5)
            {
                MainUI.SetActive(true);
                curCoroutine = StartCoroutine(FadeInMainUI());
            }

            yield return null;
        }
        sessionCountDown.Reset();

        NavController.curPanel = startPanel;
        NavController.moveIndicator();
        curCoroutine = null;
    }

    private IEnumerator TransitionToSecondStep()
    {
        int rate = 2;
        float t = 0;
        originalFtueLocation = this.FtueGameObject.transform.localPosition;
        originalSecondStepLocation = this.secondStep.transform.localPosition;
        originalFirstStepLocation = this.firstStep.transform.localPosition;
        Vector3 secondStepDestination = new Vector3(originalFirstStepLocation.x, originalFirstStepLocation.y, originalFirstStepLocation.z);
        Vector3 firstStepDestination = new Vector3(originalFirstStepLocation.x, originalSecondStepLocation.y, -1000);

        secondStep.SetActive(true);
        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.secondStep.transform.localPosition = Vector3.Lerp(originalSecondStepLocation, secondStepDestination, t);
            this.firstStep.transform.localPosition = Vector3.Lerp(originalFirstStepLocation, firstStepDestination, t);

            var firstStepImage = firstStep.GetComponentInChildren<Image>();
            var c2 = firstStepImage.color;
            firstStepImage.GetComponentInChildren<Image>().color = new Color(c2.r, c2.g, c2.b, 1 - t);

            yield return null;
        }
        stepCount++;
        curCoroutine = null;
        firstStep.SetActive(false);
        ftueTimer.Start();
    }

    private IEnumerator TransitionToThirdStep()
    {
        int rate = 2;
        float t = 0;
        float s = 0;

        originalThirdStepLocation = this.thirdStep.transform.localPosition;
        var secondStepLoc = this.secondStep.transform.localPosition;
        var fourthStepLoc = this.fourthStep.transform.localPosition;
        Vector3 thirdStepDestination = new Vector3(originalFirstStepLocation.x, originalFirstStepLocation.y, originalFirstStepLocation.z);
        Vector3 secondStepDestination2 = new Vector3(originalFourthStepLocation.x, originalFourthStepLocation.y, -1000);
        Vector3 secondStepDestination1 = new Vector3(-1032, secondStepLoc.y, secondStepLoc.z);

        secondStepCheck.SetActive(true);
        while (s < 1)
        {
            s += Time.deltaTime * rate;

            this.secondStep.transform.localPosition = Vector3.Lerp(secondStepLoc, secondStepDestination1, s);

            yield return null;
        }

        yield return new WaitForSeconds(1);
        thirdStep.SetActive(true);
        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.thirdStep.transform.localPosition = Vector3.Lerp(originalThirdStepLocation, thirdStepDestination, t);
            this.secondStep.transform.localPosition = Vector3.Lerp(secondStepLoc, secondStepDestination2, t);

            var secondStepImage = secondStep.GetComponentInChildren<Image>();
            var c2 = secondStepImage.color;
            secondStepImage.GetComponentInChildren<Image>().color = new Color(c2.r, c2.g, c2.b, 1 - t);

            yield return null;
        }
        stepCount++;
        secondStep.SetActive(false);
        curCoroutine = null;

    }

    private IEnumerator TransitionToFourthStep()
    {
        int rate = 2;
        float t = 0;
        float s = 0;

        originalFourthStepLocation = this.fourthStep.transform.localPosition;
        var thirdStepLoc = this.thirdStep.transform.localPosition;
        Vector3 fourthStepDestination = new Vector3(originalFirstStepLocation.x, originalFirstStepLocation.y, originalFirstStepLocation.z);
        Vector3 thirdStepDestination2 = new Vector3(originalThirdStepLocation.x, originalThirdStepLocation.y, -1000);
        Vector3 thirdStepDestination1 = new Vector3(1026, thirdStepLoc.y, thirdStepLoc.z);

        thirdStepCheck.SetActive(true);
        while (s < 1)
        {
            s += Time.deltaTime * rate;

            this.thirdStep.transform.localPosition = Vector3.Lerp(thirdStepLoc, thirdStepDestination1, s);

            yield return null;
        }

        yield return new WaitForSeconds(1);

        fourthStep.SetActive(true);

        while (t < 1)
        {
            t += Time.deltaTime * rate;

            this.fourthStep.transform.localPosition = Vector3.Lerp(originalFourthStepLocation, fourthStepDestination, t);
            this.thirdStep.transform.localPosition = Vector3.Lerp(thirdStepLoc, thirdStepDestination2, t);

            var thirdStepImage = thirdStep.GetComponentInChildren<Image>();
            var c2 = thirdStepImage.color;
            thirdStepImage.GetComponentInChildren<Image>().color = new Color(c2.r, c2.g, c2.b, 1 - t);
            yield return null;
        }
        stepCount++;
        curCoroutine = null;
        sessionCountDown.Start();
    }

    public void ResetFtue()
    {
        FtueGameObject.SetActive(true);
        this.FtueGameObject.transform.localPosition = originalFtueLocation;
        firstStep.transform.localPosition = originalFirstStepLocation;
        secondStep.transform.localPosition = originalSecondStepLocation;
        thirdStep.transform.localPosition = originalThirdStepLocation;
        fourthStep.transform.localPosition = originalFourthStepLocation;
        stepCount = 0;
        sessionCountDown.Reset();
        ftueTimer.Reset();
        firstStep.SetActive(true);
        


        this.MainUI.transform.localPosition = originalMainUILocation;
        var c2 = LastStepImage.color;
        LastStepImage.color = new Color(c2.r, c2.g, c2.b, 1);
        var c3 = countDownText.color;
        countDownText.color = new Color(c3.r, c3.g, c3.b, 1);

        var firstStepImage = firstStep.GetComponentInChildren<Image>();
        var c4 = firstStepImage.color;
        firstStepImage.color = new Color(c4.r, c4.g, c4.b, 1);

        var secondStepImage = secondStep.GetComponentInChildren<Image>();
        c4 = secondStepImage.color;
        secondStepImage.color = new Color(c4.r, c4.g, c4.b, 1);

        var thirdStepImage = thirdStep.GetComponentInChildren<Image>();
        c4 = thirdStepImage.color;
        thirdStepImage.color = new Color(c4.r, c4.g, c4.b, 1);
        fourthStep.SetActive(false);
        secondStep.SetActive(false);
        thirdStep.SetActive(false);
        secondStepCheck.SetActive(false);
        thirdStepCheck.SetActive(false);
    }

    public void OnStartClick()
    {
        SessionData.SessionId = System.DateTime.UtcNow.Ticks;
        SessionData.SessionDuration = 0;
        SessionData.SessionStart = System.DateTime.UtcNow;

        Firebase.Analytics.FirebaseAnalytics.LogEvent("session", "sessionStart", 1);
        sessionController.waitingForNextSession = false;
        sessionController.vuforiaBehaviour.enabled = true;
        uiController.SleepTimer.Start();
        StartCoroutine(FadeOutFtue());
    }
}
