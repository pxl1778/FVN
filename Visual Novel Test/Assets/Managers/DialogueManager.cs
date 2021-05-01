using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private string DialogueFileName;
    //[SerializeField]
    //private TextAsset CSVFile;
    [SerializeField]
    protected Text BoxText;
    [SerializeField]
    protected Image Box;
    [SerializeField]
    protected Image FadeImage;
    [SerializeField]
    protected AudioSource MusicTrack;
    [SerializeField]
    protected AudioSource SoundEffect;
    [SerializeField]
    protected float textSpeed = 0.025f;

    protected int currentLine;
    protected int currentCharacter;
    private float timer = 0;
    private bool active = false;
    private bool moveOn = false;
    private Vector3 originalBoxPosition;
    private List<DialogueLine> lines;
    private Sequence tweenSequence;

    const string DIALOGUE = "Dialogue";
    const string CHARACTER = "Character";
    const string FADE_IN_LIST = "Fade In (List Characters)";
    const string FADE_OUT_LIST = "Fade Out (List Characters)";
    const string BACKGROUND = "Background";
    const string MUSIC = "Music";
    const string SOUND = "Sound";
    const string EXCLAIM_TEXT_BOX = "Exclaim Text Box";
    const string SCREEN_FADE_IN = "Screen Fade In";
    const string SCREEN_FADE_OUT = "Screen Fade Out";
    const string SPECIAL_ACTIONS = "Special Actions";

    const string NORMAL_TEXTBOX_NAME = "Background";

    // Start is called before the first frame update
    void Start()
    {
        FadeImage.color = new Color(0, 0, 0, 1);
        FadeIn().onComplete = () =>
            {
                FadeBoxIn().onComplete = () => { active = true; };
            };
        LoadDialogue();
        originalBoxPosition = Box.rectTransform.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && active)
        {
            HandleInput();
        }
        TextAnimation();
    }

    void LoadDialogue()
    {
        using (TextFieldParser parser = new TextFieldParser(Path.Combine(Application.streamingAssetsPath, "Dialogue/" + DialogueFileName + ".csv")))
        {
            parser.SetDelimiters(",");
            lines = new List<DialogueLine>();
            string[] titles = parser.ReadFields();
            Dictionary<string, int> fieldsDictionary = new Dictionary<string, int>();
            for (int i = 0; i < titles.Length; i++)
            {
                fieldsDictionary[titles[i]] = i;
            }
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                DialogueLine line = new DialogueLine();
                line.Text = fieldsDictionary.ContainsKey(DIALOGUE) ? fields[fieldsDictionary[DIALOGUE]] : "";
                line.Character = fieldsDictionary.ContainsKey(CHARACTER) ? fields[fieldsDictionary[CHARACTER]] : "";
                line.FadeInList = fieldsDictionary.ContainsKey(FADE_IN_LIST) ? fields[fieldsDictionary[FADE_IN_LIST]].Split(',') : null;
                line.FadeOutList = fieldsDictionary.ContainsKey(FADE_OUT_LIST) ? fields[fieldsDictionary[FADE_OUT_LIST]].Split(',') : null;
                line.Background = fieldsDictionary.ContainsKey(BACKGROUND) ? fields[fieldsDictionary[BACKGROUND]] : "";
                line.Music = fieldsDictionary.ContainsKey(MUSIC) ? fields[fieldsDictionary[MUSIC]] : "";
                line.Sound = fieldsDictionary.ContainsKey(SOUND) ? fields[fieldsDictionary[SOUND]] : "";
                line.ExclaimTextBox = fieldsDictionary.ContainsKey(EXCLAIM_TEXT_BOX) ? bool.Parse(fields[fieldsDictionary[EXCLAIM_TEXT_BOX]]) : false;
                line.ScreenFadeIn = fieldsDictionary.ContainsKey(SCREEN_FADE_IN) ? bool.Parse(fields[fieldsDictionary[SCREEN_FADE_IN]]) : false;
                line.ScreenFadeOut = fieldsDictionary.ContainsKey(SCREEN_FADE_OUT) ? bool.Parse(fields[fieldsDictionary[SCREEN_FADE_OUT]]) : false;
                line.SpecialActions = fieldsDictionary.ContainsKey(SPECIAL_ACTIONS) ? fields[fieldsDictionary[SPECIAL_ACTIONS]].Split(';') : null;
                lines.Add(line);
            }
        }
    }

    void TextAnimation()
    {
        if (active && currentCharacter < lines[currentLine].Text.Length)
        {
            timer += Time.deltaTime;
            if (timer >= textSpeed)
            {
                BoxText.text = lines[currentLine].Text.Substring(0, currentCharacter + 1);
                timer = 0;
                currentCharacter++;
                if (currentCharacter >= lines[currentLine].Text.Length)
                {
                    //end of line
                    moveOn = true;
                }
            }
        }
    }

    void ContinueDialogue()
    {
        tweenSequence.Kill(true);
        tweenSequence = DOTween.Sequence();
        if (lines[currentLine].ScreenFadeOut)
        {
            tweenSequence.Append(FadeOut());
        }
        currentLine++;
        DialogueLine current = lines[currentLine];
        if (current.ScreenFadeIn)
        {
            tweenSequence.Append(FadeIn());
        }
        if(current.FadeOutList != null && current.FadeOutList.Length > 0)
        {
            foreach(string character in current.FadeOutList)
            {
                Debug.Log("Fade Out: " + character);
                //find character image and start fade out tween
            }
        }
        if (current.Background != "")
        {
            //change backgroundimage image
            //new image on top, .DOFade image on top
        }
        if (current.Music != "")
        {
            //music
            Tween musicTween = Box.DOFade(Box.color.a, 0.1f);
            musicTween.onComplete = () => { Debug.Log("ChangeMusic");/*Change music*/ };
            tweenSequence.Append(musicTween);
        }
        Image i = Box.GetComponent<Image>();
        Tween t = Box.DOFade(Box.color.a, 0.1f);
        t.onComplete = () => {
            if (current.Sound != "")
            {
                Debug.Log("ChangeSound");/*Change music*/
            }
            if (current.ExclaimTextBox == (Box.GetComponent<Image>().sprite.name == NORMAL_TEXTBOX_NAME))
            {
                Debug.Log("ChangeTextBox");
            }
        };
        tweenSequence.Append(t);
        if (current.FadeInList != null && current.FadeInList.Length > 0)
        {
            foreach (string character in current.FadeInList)
            {
                //find character image and start fade out tween
                Debug.Log("Fade In: " + character);
            }
        }
        Tween nameTween = Box.DOFade(Box.color.a, 0.1f);
        nameTween.onComplete = () => {
            Debug.Log("Change Nameplate");
            currentCharacter = 0;
            timer = 0;
            active = true;
        };
        active = false;
        tweenSequence.Append(nameTween);
        BoxText.text = "";
    }

    void HandleInput()
    {
        if (moveOn)
        {
            if(currentLine + 1 < lines.Count)
            {
                //Next line
                ContinueDialogue();
            }
            else
            {
                SceneEnd();
            }
        }
        else
        {
            //Skip animation.
            BoxText.text = lines[currentLine].Text;
            currentCharacter = lines[currentLine].Text.Length;
            moveOn = true;
        }
    }

    void SceneEnd()
    {
        active = false;
        currentCharacter = 0;
        timer = 0;
        FadeBoxOut().onComplete = () => { FadeOut(); };
    }

    Tween FadeIn()
    {
        return FadeImage.DOFade(0, 1);
    }

    Tween FadeOut()
    {
        return FadeImage.DOFade(1, 1);
    }

    Tween FadeBoxIn()
    {
        Box.rectTransform.anchoredPosition = originalBoxPosition - new Vector3(0, 300, 0);
        Box.GetComponent<CanvasGroup>().DOFade(1, 0.3f);
        return Box.rectTransform.DOAnchorPosY(originalBoxPosition.y, 0.5f).SetEase(Ease.OutBack);
    }

    Tween FadeBoxOut()
    {
        Vector3 fadePos = originalBoxPosition - new Vector3(0, 50, 0);
        Box.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        return Box.rectTransform.DOAnchorPosY(fadePos.y, 0.3f);
    }
}
