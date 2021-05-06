using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private Button PauseButton;

    private Tween rotateTween;
    private Tween positionTween;
    private bool opened = false;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    }

    public void OnQuitButtonClicked()
    {

    }

    private void OpenPauseMenu()
    {
        opened = true;
        if (rotateTween != null) { rotateTween.Kill(); }
        if (positionTween != null) { positionTween.Kill(); }
        rotateTween = PauseButton.GetComponentsInChildren<Image>()[1].rectTransform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 0.2f);
        positionTween = this.GetComponent<RectTransform>().DOAnchorPosY(200.0f, 0.2f);
    }

    private void ClosePauseMenu()
    {
        opened = false;
        if (rotateTween != null) { rotateTween.Kill(); }
        if (positionTween != null) { positionTween.Kill(); }
        rotateTween = PauseButton.GetComponentsInChildren<Image>()[1].rectTransform.DORotate(new Vector3(0.0f, 0.0f, 180.0f), 0.2f);
        positionTween = this.GetComponent<RectTransform>().DOAnchorPosY(0.0f, 0.2f);
    }
}
