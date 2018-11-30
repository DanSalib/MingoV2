using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;


public class SessionController : MonoBehaviour {

    public Stopwatch SessionTimer = new Stopwatch();
    public Stopwatch CameraTimer = new Stopwatch();
    public Stopwatch LogTimer = new Stopwatch();

    public VuforiaBehaviour vuforiaBehaviour;
    private const float SESSION_TIMOUT = 60000;
    private const float TURN_OFF_CAMERA_DELAY = 60000;
    private const float LOG_DELAY = 60000;
    public bool waitingForNextSession = true;
    public Text text;

    public delegate void SessionReset();
    public static event SessionReset OnSessionReset;
    private Vector3 prevAccel;

    private List<Vector3> accelerations = new List<Vector3>();
    // Use this for initialization
    void Start () {
        SimplePlayback.OnFinished += StartSessionTimer;
        SimplePlayback.OnReady += StopSessionTimer;
        NavigationController.OnKeyPress += ResetSessionTimer;
        waitingForNextSession = true;
        CameraTimer.Start();
        LogTimer.Start();
    }

    private void OnDestroy()
    {
        SimplePlayback.OnFinished -= StartSessionTimer;
        SimplePlayback.OnReady -= StopSessionTimer;
        NavigationController.OnKeyPress -= ResetSessionTimer;
    }

    // Update is called once per frame
    void Update () {
		if(SessionTimer.IsRunning && SessionTimer.ElapsedMilliseconds > SESSION_TIMOUT)
        {
            ResetSession();
        }

        if (waitingForNextSession && vuforiaBehaviour.enabled == false && (Input.GetKeyDown(KeyCode.V) || (accelerations.Count > 120 && !IsLast120AccelSame())))
        {
            CameraTimer.Start();
            vuforiaBehaviour.enabled = true;
            accelerations = new List<Vector3>();
        } else if(accelerations.Count < 125)
        {
            accelerations.Add(Input.acceleration);
        }
        else
        {
            accelerations = new List<Vector3>();
        }

        if (waitingForNextSession && CameraTimer.ElapsedMilliseconds > TURN_OFF_CAMERA_DELAY && vuforiaBehaviour.enabled == true
            && (Input.GetKeyDown(KeyCode.V) || (accelerations.Count > 120 && IsLast120AccelSame())))
        {
            CameraTimer.Reset();
            vuforiaBehaviour.enabled = false;
            accelerations = new List<Vector3>();
        }
        else if (waitingForNextSession && CameraTimer.ElapsedMilliseconds > TURN_OFF_CAMERA_DELAY && vuforiaBehaviour.enabled == true
            && (Input.GetKeyDown(KeyCode.V) || (accelerations.Count > 120 && !IsLast120AccelSame())))
        {
            CameraTimer.Restart();
            accelerations = new List<Vector3>();
        }

        if(LogTimer.ElapsedMilliseconds > LOG_DELAY)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent("headsetStatus", "inSession", (!waitingForNextSession).ToString());
            LogTimer.Restart();
            SessionData.SessionDuration += (long)LOG_DELAY;
        }
    }

    private bool IsLast120AccelSame()
    {
        var tol = 0.005f;
        var prevAccel = accelerations[120];
        for(int i = accelerations.Count - 1; i > accelerations.Count - 120; i--)
        {
            if(Mathf.Abs(accelerations[i].x - prevAccel.x) > tol
                || Mathf.Abs(accelerations[i].y - prevAccel.y) > tol
                || Mathf.Abs(accelerations[i].z - prevAccel.z) > tol)
            {
                accelerations = new List<Vector3>();
                return false;
            }
            prevAccel = accelerations[i];
        }
        return true;
    }

    public void StartSessionTimer()
    {
        SessionTimer.Start();
    }

    public void StopSessionTimer()
    {
        SessionTimer.Reset();
        SessionTimer.Stop();
    }

    public void ResetSessionTimer(NavigationController.directions d)
    {
        if(SessionTimer.IsRunning)
        {
            SessionTimer.Restart();
        }
        else
        {
            SessionTimer.Reset();
        }
    }

    public void  ResetSession()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("session", "sessionEnd", "sessionDuration="+SessionData.SessionDuration+ " sessionId="+SessionData.SessionId);
        waitingForNextSession = true;
        vuforiaBehaviour.enabled = false;
        SessionTimer.Reset();
        SessionTimer.Stop();
        OnSessionReset();
    }
}
