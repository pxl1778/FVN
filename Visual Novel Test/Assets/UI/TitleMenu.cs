using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    [SerializeField]
    GameObject TitleObject;
    [SerializeField]
    GameObject MainObject;
    [SerializeField]
    GameObject SelectSceneObject;
    [SerializeField]
    TransitionLines TransitionObject;
    [SerializeField]
    ParticleSystem ClickParticles;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Canvas myCanvas = this.GetComponent<Canvas>();
            Vector2 newPos = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, Camera.main, out newPos);
            RectTransform r = ClickParticles.GetComponent<RectTransform>();
            r.localPosition = newPos;
            r.localPosition += new Vector3(0, 0, -200);
            Debug.Log(Input.mousePosition);
            ClickParticles.Play();
        }
    }

    public void TransitionTitleScreen()
    {
        GameManager.instance.EventManager.TransitionLinesMidMovement.AddListener(ShowMainMenu);
        TransitionObject.StartTransition();
    }

    public void ShowMainMenu()
    {
        GameManager.instance.EventManager.TransitionLinesMidMovement.RemoveListener(ShowMainMenu);
        TitleObject.SetActive(false);
        MainObject.SetActive(true);
        TransitionObject.EndTransition();
    }
}
