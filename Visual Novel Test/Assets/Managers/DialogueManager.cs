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
    protected float textSpeed = 0.025f;

    protected int currentLine;
    protected int currentCharacter;
    private float timer = 0;
    private bool active = false;
    private bool moveOn = false;
    private Vector3 originalBoxPosition;
    private List<DialogueLine> lines;

    const string DIALOGUE = "Dialogue";
    const string CHARACTER = "Character";
    const string FADE_IN_LIST = "Fade In (List Characters)";
    const string FADE_OUT_LIST = "Fade Out (List Characters)";
    const string BACKGROUND = "Background";
    const string EXCLAIM_TEXT_BOX = "Exclaim Text Box";
    const string SCREEN_FADE_IN = "Screen Fade In";
    const string SCREEN_FADE_OUT = "Screen Fade Out";

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
                line.Text = fields[fieldsDictionary[DIALOGUE]];
                line.Character = fields[fieldsDictionary[CHARACTER]];
                line.FadeInList = fields[fieldsDictionary[FADE_IN_LIST]].Split(',');
                line.FadeOutList = fields[fieldsDictionary[FADE_OUT_LIST]].Split(',');
                line.Background = fields[fieldsDictionary[BACKGROUND]];
                line.ExclaimTextBox = bool.Parse(fields[fieldsDictionary[EXCLAIM_TEXT_BOX]]);
                line.ScreenFadeIn = bool.Parse(fields[fieldsDictionary[SCREEN_FADE_IN]]);
                line.ScreenFadeOut = bool.Parse(fields[fieldsDictionary[SCREEN_FADE_OUT]]);
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

    void HandleInput()
    {
        if (moveOn)
        {
            currentLine++;
            if(currentLine < lines.Count)
            {
                //Next line
                currentCharacter = 0;
                BoxText.text = "";
                timer = 0;
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
