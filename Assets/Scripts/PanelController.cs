using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PanelController : MonoBehaviour {

    public delegate void PanelClick(PanelController panel);
    public static event PanelClick OnClicked;
    public string categoryId;

    public bool startPanel;

    // Use this for initialization
    void Start ()
    {
        YoutubeVideoListCreator.OnLoad += LoadPanel;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void LoadPanel(string id)
    {
        if(this.categoryId == id)
        {
            OnClicked(this);
            YoutubeVideoListCreator.OnLoad -= LoadPanel;
        }
    }

    public void OnPanelClick()
    {
        OnClicked(this);
    }
}
