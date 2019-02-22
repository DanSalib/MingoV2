using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class NotificationController : MonoBehaviour {

    public Text notificationText;
    public GameObject notifcationPanel;
    public Stopwatch notificationTimer = new Stopwatch();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(notificationTimer.IsRunning && notificationTimer.ElapsedMilliseconds > 5000)
        {
            notifcationPanel.SetActive(false);
            notificationTimer.Reset();
        }
	}

    public void ShowNotification(string message)
    {
        if(notifcationPanel.activeInHierarchy)
        {
            return;
        }
        notificationText.text = message;
        notifcationPanel.SetActive(true);
        notificationTimer.Restart();
    }
}
