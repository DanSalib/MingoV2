using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
public class NavigationController : MonoBehaviour {

    public delegate void KeyPress(directions d);
    public static event KeyPress OnKeyPress;
    public Text text;
    public VideoOptionsController videoOptions;

    public MainUIController uiController;
    public bool ftueActive = true;
    public GameObject backIndicator;
    public GameObject vidOptionsIndicator;
    public GameObject viewPort;
    public GameObject indicator;
    public NavObject curPanel;
    public NavObject prevPanel;
    public GameObject downNotice;
    private directions prevDirection;

    public bool reconnecting = false;

    private Stopwatch joystickTimer = new Stopwatch();
    private NavObject startPanel;

    private void Start()
    {
        SessionController.OnSessionReset += ResetNav;
        joystickTimer.Start();
        OnKeyPress += ChangeCurPanel;
    }

    private void OnEnable()
    {
        startPanel = curPanel;
    }

    private void OnDestroy()
    {
        SessionController.OnSessionReset -= ResetNav;
    }

    public void ResetNav()
    {
        ftueActive = true;
        curPanel = startPanel;
        vidOptionsIndicator.SetActive(false);
    }

    public enum directions
    {
        up = 0,
        down = 1,
        left = 2,
        right = 3,
        click = 4
    };

    private void ChangeCurPanel(directions d)
    {
        if(ftueActive || uiController.disableButtons || uiController.isSleeping)
        {
            return;
        }

        if(d == directions.up)
        {
            if (curPanel.isBack || ((curPanel.upNeighbor != null && curPanel.upNeighbor.isVideo) && videoOptions.mediaController.disableVideoOptions))
            {
                return;
            }
            else if (curPanel.upNeighbor != null && curPanel.upNeighbor.isVideo)
            {
                ShowVideoOptions();
                indicator.SetActive(false);
                prevPanel = curPanel;
                curPanel = curPanel.upNeighbor ?? curPanel;
                moveIndicator();
            } else if(prevPanel == null)
            {
                curPanel = curPanel.upNeighbor ?? curPanel;
                if (viewPort.activeInHierarchy && indicator.transform.localPosition.y > -84 && viewPort.transform.localPosition.y > 50)
                {
                    var curPos = viewPort.transform.localPosition;
                    viewPort.transform.localPosition = new Vector3(curPos.x, curPos.y - 131, curPos.z);
                }
                moveIndicator();
            }
        }
        else if (d == directions.right)
        {
            if (curPanel.isVideo)
            {
                curPanel = curPanel.rightNeighbor ?? curPanel;
                moveIndicator();
            }
            else if (prevPanel != null)
            {
                backIndicator.SetActive(false);
                indicator.SetActive(true);
                curPanel = prevPanel;
                prevPanel = null;
            }
            else
            {
                curPanel = curPanel.rightNeighbor ?? curPanel;
                moveIndicator();
            }
        }
        else if (d == directions.down)
        {
            if (curPanel.isBack)
            {
                return;
            }
            else if (prevPanel != null)
            {
                HideOptions();
                vidOptionsIndicator.SetActive(false);
                downNotice.SetActive(false);
                indicator.SetActive(true);
                curPanel = prevPanel;
                prevPanel = null;
            }
            else
            {
                curPanel = curPanel.downNeighbor ?? curPanel;
                if (viewPort.activeInHierarchy && indicator.transform.localPosition.y < -200 && viewPort.transform.localPosition.y < 720)
                {
                    var curPos = viewPort.transform.localPosition;
                    viewPort.transform.localPosition = new Vector3(curPos.x, curPos.y + 131, curPos.z);
                }
                moveIndicator();
            }
        }
        else if (d == directions.left)
        {
            if (curPanel.isVideo)
            {
                curPanel = curPanel.leftNeighbor ?? curPanel;
                moveIndicator();
            }
            else if (curPanel.leftNeighbor != null && curPanel.leftNeighbor.isBack)
            {
                backIndicator.SetActive(true);
                indicator.SetActive(false);
                prevPanel = curPanel;
                curPanel = curPanel.leftNeighbor ?? curPanel;
            }
            else
            {
                curPanel = curPanel.leftNeighbor ?? curPanel;
                moveIndicator();
            }
        }
        else if (d == directions.click)
        {
            indicator.SetActive(false);
            if (curPanel != null && !curPanel.isVideo)
            {
                prevPanel = null;
            }
            curPanel.thisButton.onClick.Invoke();
            backIndicator.SetActive(false);
        }
    }

    public void moveIndicator()
    {
        if (curPanel != null && !curPanel.isBack && !curPanel.isVideo)
        {
            indicator.SetActive(true);
            indicator.transform.position = curPanel.thisObject.transform.position;
        } else if (curPanel != null && curPanel.isVideo)
        {
            downNotice.SetActive(true);
            vidOptionsIndicator.SetActive(true);
            vidOptionsIndicator.transform.position = curPanel.thisObject.transform.position;
        }
    }

    public void ShowVideoOptions()
    {
        videoOptions.ShowVideoOptions();
    }

    public void HideOptions()
    {
        videoOptions.HideVideoOptions();
    }

    // Update is called once per frame
    void Update () {
        //foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        //{
        //    if (Input.GetKeyDown(kcode))
        //        text.text += ("" + kcode);
        //}
        //if (Input.GetKeyDown(KeyCode.JoystickButton2))
        //{
        //    OnKeyPress(directions.up);
        //}
        //else if (Input.GetKeyDown(KeyCode.JoystickButton3))
        //{
        //    OnKeyPress(directions.right);
        //}
        //else if (Input.GetKeyDown(KeyCode.RightShift))
        //{
        //    OnKeyPress(directions.down);
        //}
        //else if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    OnKeyPress(directions.left);
        //}
        //else if (Input.GetKeyDown(KeyCode.JoystickButton7))
        //{
        //    OnKeyPress(directions.click);
        //}
        //   text.text += Input.GetAxisRaw("Oculus_GearVR_LThumbstickX") != 0 ? "x: " + Input.GetAxis("Horizontal") : "";
        //  text.text += Input.GetAxisRaw("Oculus_GearVR_LThumbstickY") != 0 ? "y: " + Input.GetAxis("Vertical") : "";

        //if (Input.GetKeyDown(KeyCode.JoystickButton2))
        //{
        //    OnKeyPress(directions.up);
        //}
        //else if (Input.GetKeyDown(KeyCode.JoystickButton3))
        //{
        //    OnKeyPress(directions.right);
        //}
        //else if (Input.GetKeyDown(KeyCode.JoystickButton0))
        //{
        //    OnKeyPress(directions.left);
        //}
        //else if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        //{
        //    OnKeyPress(directions.down);
        //}
        //else if (Input.GetKeyDown(KeyCode.RightShift))
        //{
        //    OnKeyPress(directions.click);
        //}
        //else if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    OnKeyPress(directions.click);
        //}

        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    UnityEngine.Debug.Log("up");
        //    OnKeyPress(directions.up);
        //}
        //else if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    OnKeyPress(directions.right);
        //}
        //else if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    OnKeyPress(directions.left);
        //}
        //else if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    OnKeyPress(directions.down);
        //}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnKeyPress(directions.click);
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            OnKeyPress(directions.click);
        }
        else if ((prevDirection != directions.right || joystickTimer.ElapsedMilliseconds > 200f) && Input.GetAxisRaw("Horizontal") == 1)
        {
            joystickTimer.Reset();
            joystickTimer.Start();
            prevDirection = directions.right;
            OnKeyPress(directions.right);
        }
        else if ((prevDirection != directions.left || joystickTimer.ElapsedMilliseconds > 200f) && Input.GetAxisRaw("Horizontal") == -1)
        {
            joystickTimer.Reset();
            joystickTimer.Start();
            prevDirection = directions.left;
            OnKeyPress(directions.left);
        }
        else if ((prevDirection != directions.down || joystickTimer.ElapsedMilliseconds > 200f) && Input.GetAxisRaw("Vertical") == -1)
        {
            joystickTimer.Reset();
            joystickTimer.Start();
            prevDirection = directions.down;
            OnKeyPress(directions.down);
        }
        else if ((prevDirection != directions.up || joystickTimer.ElapsedMilliseconds > 200f) && Input.GetAxisRaw("Vertical") == 1)
        {
            joystickTimer.Reset();
            joystickTimer.Start();
            prevDirection = directions.up;
            OnKeyPress(directions.up);
        }
    }
}
