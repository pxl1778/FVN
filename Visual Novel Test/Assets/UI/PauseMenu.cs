﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Button PauseButton;
    [SerializeField]
    private Button FastForwardButton;

    private Tween rotateTween;
    private Tween positionTween;
    private bool opened = false;
    private EventSystem eventSystem;
    private bool onTop = true;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (onTop)
        {
            if (eventSystem.currentSelectedGameObject == null && (Input.GetAxis("Horizontal") > 0.1 || Input.GetAxis("Horizontal") < -0.1f))
            {
                eventSystem.SetSelectedGameObject(FastForwardButton.gameObject);
            }
            if (Input.GetButtonDown("Pause"))
            {
                PauseButtonClicked();
                if (!opened)
                {
                    eventSystem.SetSelectedGameObject(FastForwardButton.gameObject);
                }
            }
            if (eventSystem.currentSelectedGameObject != null && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        eventSystem.SetSelectedGameObject(null);
    }

    public void PauseButtonClicked()
    {
        if (opened)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OnFastForwardButtonClicked()
    {

    }

    public void OnSaveButtonClicked()
    {

    }

    public void OnLoadButtonClicked()
    {

    }

    public void OnSettingsButtonClicked()
    {

    }

    public void OnHomeButtonClicked()
    {
        //Do some saving stuff
        //maybe do a warning window
        onTop = false;
        GameObject a = GameManager.instance.UIUtility.CreateAlertBox("Would you like to go back to the title screen?\n(Any unsaved progress will be lost.)", this.transform.parent, () => {
            SceneManager.LoadScene("TitleMenu");
        }, () => { onTop = true; });
    }

    public void OnQuitButtonClicked()
    {
        //Do warning
        onTop = false;
        GameObject a = GameManager.instance.UIUtility.CreateAlertBox("Would you like to quit the game?\n(Any unsaved progress will be lost.)", this.transform.parent, () => {
            GameObject.Find("GameManager").GetComponent<GameManager>().ExitGame();
        }, () => { onTop = true; });
    }


    private void OpenPauseMenu()
    {
        opened = true;
        if (rotateTween != null) { rotateTween.Kill(); }
        if (positionTween != null) { positionTween.Kill(); }
        rotateTween = PauseButton.GetComponentsInChildren<Image>()[1].rectTransform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 0.2f);
        positionTween = this.GetComponent<RectTransform>().DOAnchorPosY(200.0f, 0.2f);
        GameManager.instance.EventManager.Pause.Invoke();
    }

    private void ClosePauseMenu()
    {
        opened = false;
        if (rotateTween != null) { rotateTween.Kill(); }
        if (positionTween != null) { positionTween.Kill(); }
        rotateTween = PauseButton.GetComponentsInChildren<Image>()[1].rectTransform.DORotate(new Vector3(0.0f, 0.0f, 180.0f), 0.2f);
        positionTween = this.GetComponent<RectTransform>().DOAnchorPosY(0.0f, 0.2f);
        eventSystem.SetSelectedGameObject(null);
        GameManager.instance.EventManager.Unpause.Invoke();
    }
}
