using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Net.NetworkInformation;

public class SessionController : MonoBehaviour {

    public Stopwatch SessionTimer = new Stopwatch();
    public Stopwatch CameraTimer = new Stopwatch();
    public Stopwatch LogTimer = new Stopwatch();
    public Stopwatch ControllerTimer = new Stopwatch();

    public NavigationController navController;
    public NotificationController NotificationController;

    public VuforiaBehaviour vuforiaBehaviour;
    public float sessionTimeout;
    private const float TURN_OFF_CAMERA_DELAY = 60000;
    private const float LOG_DELAY = 60000;
    private const float CONTROLLER_CHECK_DELAY = 15000;
    public bool waitingForNextSession;
    public Text text;

    public GameObject ReconnectDialog;
    public GameObject CheckMarkImage;
    public delegate void SessionReset();
    public static event SessionReset OnSessionReset;
    private Vector3 prevAccel;

    private List<Vector3> accelerations = new List<Vector3>();
    // Use this for initialization
    void Start () {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        app.SetEditorDatabaseUrl("https://mingo-f8508.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

        VideoOptionsController.OnPlay += StopSessionTimer;
        NavigationController.OnKeyPress += ResetSessionTimer;
        CameraTimer.Start();
        LogTimer.Start();
        ControllerTimer.Start();
        SetIntermissionVideos();
    }

    private void SetIntermissionVideos()
    {
        FirebaseDatabase.DefaultInstance.GetReference("Intermissions").GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var child in snapshot.Children)
                {
                    int startAt = 0;
                    int.TryParse(child.Value.ToString(), out startAt);
                    var vidInfo = new IntermissionVideoInfo(child.Key, startAt);
                    SessionData.IntermissionVideoIds.Add(vidInfo);
                }
            }
        });
    }

    private void OnDestroy()
    {
        VideoOptionsController.OnPlay -= StopSessionTimer;
        NavigationController.OnKeyPress -= ResetSessionTimer;
    }

    // Update is called once per frame
    void Update () {
		if(SessionTimer.IsRunning && SessionTimer.ElapsedMilliseconds > sessionTimeout)
        {
            SessionTimer.Reset();
            ResetSession();
        }

        if(LogTimer.IsRunning && LogTimer.ElapsedMilliseconds > 10000 && (LogTimer.ElapsedMilliseconds % 60000) >= 0 && (LogTimer.ElapsedMilliseconds) % 60000 <= 10)
        {
            NotificationController.ShowNotification("Please remember to keep your mouth open wide");
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

        if(waitingForNextSession && ControllerTimer.ElapsedMilliseconds > CONTROLLER_CHECK_DELAY)
        {
            if (!ReconnectDialog.activeInHierarchy && (Input.GetJoystickNames().Length == 0 || !Input.GetJoystickNames()[0].Contains("Fortune Tech")) && !Input.GetKeyDown(KeyCode.O))
            {
                Firebase.Analytics.FirebaseAnalytics.LogEvent("controllerStatus", "connected", 0);
                SessionData.ControllerStatus.Add(0);
                ReconnectDialog.SetActive(true);
            }
            else if (ReconnectDialog.activeInHierarchy && (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Contains("Fortune Tech")) || Input.GetKeyDown(KeyCode.O))
            {
                if(ReconnectDialog.activeInHierarchy)
                {
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("controllerStatus", "connected", 1);
                    SessionData.ControllerStatus.Add(1);

                    StartCoroutine(ShowCheckMark());
                }
                ReconnectDialog.SetActive(false);
                ControllerTimer.Restart();
            }
        }

        if(LogTimer.ElapsedMilliseconds > LOG_DELAY)
        {
            LogTimer.Restart();
            Firebase.Analytics.FirebaseAnalytics.LogEvent("headsetStatus", "inSession", (waitingForNextSession ? 0 : 1));
            SessionData.HeadsetStatus.Add(waitingForNextSession ? 0 : 1);
        }
    }

    private IEnumerator ShowCheckMark()
    {
        CheckMarkImage.SetActive(true);

        yield return new WaitForSeconds(0.75f);

        CheckMarkImage.SetActive(false);
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
      //  UnityEngine.Debug.Log("Start!!!!!");
        SessionTimer.Reset();
        SessionTimer.Start();
    }

    public void StopSessionTimer()
    {
      //  UnityEngine.Debug.Log("Stop!!!!!");
        SessionTimer.Reset();
        SessionTimer.Stop();
    }

    public void ResetSessionTimer(NavigationController.directions d)
    {
       // UnityEngine.Debug.Log("RESET!!!!!");

        if (SessionTimer.IsRunning)
        {
            SessionTimer.Restart();
        }
        else
        {
            SessionTimer.Reset();
        }
    }

    [Serializable]
    private struct SessionDataJson
    {
        public long SessionId;

        public double SessionDuration;

        public int VidCount;

        public string SessionStart;

        public string SessionEnd;

        public List<int> HeadsetStatus;

        public List<int> ControllerStatus;

        public List<CategoryLog> CategoryLog;

        public string MAC;

    }

    public string GetMacAddress()
    {
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = "";

        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string mac = null;
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + "-");
                }
            }
            info += mac + "/";
        }
        return info;
    }

    public void  ResetSession()
    {
        SessionData.SessionEnd = System.DateTime.UtcNow;
        SessionData.SessionDuration = (SessionData.SessionEnd.Ticks - SessionData.SessionStart.Ticks) / System.TimeSpan.TicksPerSecond;
        Firebase.Analytics.FirebaseAnalytics.LogEvent("session", "sessionEnd", SessionData.SessionDuration);
        SessionDataJson data;
        data.SessionId = SessionData.SessionId;
        data.SessionDuration = SessionData.SessionDuration;
        data.VidCount = SessionData.VidCount;
        data.HeadsetStatus = SessionData.HeadsetStatus;
        data.SessionStart = SessionData.SessionStart.ToString();
        data.SessionEnd = SessionData.SessionEnd.ToString();
        data.ControllerStatus = SessionData.ControllerStatus;
        data.CategoryLog = SessionData.CategoryLogs;
        data.MAC = GetMacAddress();

        var json =  JsonUtility.ToJson(data);
        UnityEngine.Debug.Log(json);
        var reference = FirebaseDatabase.DefaultInstance.RootReference;
        var stats = reference.Child("SessionStats").Push().SetRawJsonValueAsync(json);
        SessionData.HeadsetStatus.Clear();
        SessionData.ControllerStatus.Clear();

        SessionData.VidCount = 0;
        SessionData.CategoryLogs.Clear();


        waitingForNextSession = true;
        vuforiaBehaviour.enabled = false;
        SessionTimer.Reset();
        SessionTimer.Stop();
        OnSessionReset();
    }
}
