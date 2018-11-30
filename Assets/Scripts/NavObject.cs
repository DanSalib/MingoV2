using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavObject : MonoBehaviour {
    public Button thisButton;
    public GameObject thisObject;
    public NavObject upNeighbor;
    public NavObject downNeighbor;
    public NavObject leftNeighbor;
    public NavObject rightNeighbor;
    public bool isBack = false;
    public bool isVideo = false;
}
