﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private string DialogueFileName;
    [SerializeField]
    protected GameObject BackgroundsParent;
    [SerializeField]
    protected GameObject SpritesParent;
    [SerializeField]
    protected GameObject BackgroundPrefab;
    [SerializeField]
    protected GameObject SpritesPrefab;
    [SerializeField]
    protected Text BoxText;
    [SerializeField]
    protected Text NameText;
    [SerializeField]
    protected Image Box;
    [SerializeField]
    protected Image FadeImage;
    [SerializeField]
    protected Image NamePlate;
    [SerializeField]
    protected AudioSource MusicTrack;
    [SerializeField]
    protected AudioSource SoundEffect;
    [SerializeField]
    protected float textSpeed = 0.025f;

    protected int currentLine = -1;
    protected int currentCharacter;
    private float timer = 0;
    private bool active = false;
    private bool moveOn = false;
    private Vector3 originalBoxPosition;
    private List<DialogueLine> lines;
    private Sequence tweenSequence;
    private Image currentBackground;
    private Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> backgroundDictionary = new Dictionary<string, Sprite>();
    private int dataLoaded = 0;
    private Dictionary<string, Image> characterDictionary = new Dictionary<string, Image>();

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
    const string EXCLAIM_TEXTBOX_NAME = "Background";

    // Start is called before the first frame update
    void Start()
    {
        LoadDialogue();
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
                line.FadeInList = fieldsDictionary.ContainsKey(FADE_IN_LIST) && fields[fieldsDictionary[FADE_IN_LIST]] != "" ? fields[fieldsDictionary[FADE_IN_LIST]].Split(',') : null;
                if(line.FadeInList != null)
                {
                    List<string> sList = line.FadeInList.ToList();
                    line.FadeInList.ToList().ForEach(s => {
                        spriteDictionary[s.Split(' ')[0]] = null; }); }
                line.FadeOutList = fieldsDictionary.ContainsKey(FADE_OUT_LIST) && fields[fieldsDictionary[FADE_OUT_LIST]] != "" ? fields[fieldsDictionary[FADE_OUT_LIST]].Split(',') : null;
                line.Background = fieldsDictionary.ContainsKey(BACKGROUND) ? fields[fieldsDictionary[BACKGROUND]] : "";
                if(line.Background != "") { backgroundDictionary[line.Background] = null; }
                line.Music = fieldsDictionary.ContainsKey(MUSIC) ? fields[fieldsDictionary[MUSIC]] : "";
                line.Sound = fieldsDictionary.ContainsKey(SOUND) ? fields[fieldsDictionary[SOUND]] : "";
                line.ExclaimTextBox = fieldsDictionary.ContainsKey(EXCLAIM_TEXT_BOX) ? bool.Parse(fields[fieldsDictionary[EXCLAIM_TEXT_BOX]]) : false;
                line.ScreenFadeIn = fieldsDictionary.ContainsKey(SCREEN_FADE_IN) ? bool.Parse(fields[fieldsDictionary[SCREEN_FADE_IN]]) : false;
                line.ScreenFadeOut = fieldsDictionary.ContainsKey(SCREEN_FADE_OUT) ? bool.Parse(fields[fieldsDictionary[SCREEN_FADE_OUT]]) : false;
                line.SpecialActions = fieldsDictionary.ContainsKey(SPECIAL_ACTIONS) ? fields[fieldsDictionary[SPECIAL_ACTIONS]].Split(';') : null;
                lines.Add(line);
            }
        }
        LoadBackgrounds();
        LoadSprites();
    }

    void LoadBackgrounds()
    {
        foreach(string s in backgroundDictionary.Keys)
        {
            AsyncOperationHandle<Sprite[]> spriteHandle = Addressables.LoadAssetAsync<Sprite[]>("Assets/Art/Backgrounds/" + s + ".png");
            spriteHandle.Completed += BackgroundsLoaded;
        }
    }
    void LoadSprites()
    {
        foreach (string s in spriteDictionary.Keys)
        {
            string characterName = s.Split('_')[0];
            AsyncOperationHandle<Sprite[]> spriteHandle = Addressables.LoadAssetAsync<Sprite[]>("Assets/Art/Sprites/" + characterName + "/" + s + ".png");
            spriteHandle.Completed += SpritesLoaded;
        }
    }

    void SpritesLoaded(AsyncOperationHandle<Sprite[]> handleToCheck)
    {
        if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite[] spriteArray = handleToCheck.Result;
            if (!spriteDictionary.ContainsKey(spriteArray[0].name))
            {
                Debug.LogWarning("Sprite name not consistent: " + spriteArray[0].name);
            }
            spriteDictionary[spriteArray[0].name] = spriteArray[0];
            dataLoaded++;
        }
        else
        {
            Debug.LogWarning("Issue with Loading Sprites: " + handleToCheck.Status);
        }
        CheckDoneLoading();
    }

    void BackgroundsLoaded(AsyncOperationHandle<Sprite[]> handleToCheck)
    {
        if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite[] spriteArray = handleToCheck.Result;
            if (!backgroundDictionary.ContainsKey(spriteArray[0].name))
            {
                Debug.LogWarning("Background name not consistent: " + spriteArray[0].name);
            }
            backgroundDictionary[spriteArray[0].name] = spriteArray[0];
            dataLoaded++;
        }
        else
        {
            Debug.LogWarning("Issue with Loading Backgrounds: " + handleToCheck.Status);
        }
        CheckDoneLoading();
    }

    void CheckDoneLoading()
    {
        if(dataLoaded == backgroundDictionary.Keys.Count + spriteDictionary.Keys.Count)
        {
            if (lines[0].Background != "")
            {
                currentBackground = GameObject.Instantiate(BackgroundPrefab, BackgroundsParent.transform).GetComponent<Image>();
                currentBackground.sprite = backgroundDictionary[lines[0].Background];
            }
            FadeImage.color = new Color(0, 0, 0, 1);
            FadeIn().onComplete = () =>
            {
                FadeBoxIn().onComplete = () => { ContinueDialogue(); };
            };
            originalBoxPosition = Box.rectTransform.anchoredPosition;
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
        if(tweenSequence != null) { tweenSequence.Kill(true); }
        tweenSequence = DOTween.Sequence();
        if (currentLine >= 0 && lines[currentLine].ScreenFadeOut)
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
                string characterName = character.Split('_')[0];
                if (characterDictionary.ContainsKey(characterName))
                {
                    tweenSequence.Join(characterDictionary[characterName].DOFade(0.0f, 1.0f).OnComplete(() => {
                        GameObject.Destroy(characterDictionary[characterName].gameObject);
                        characterDictionary.Remove(characterName);
                    }));
                }
                else
                {
                    Debug.LogWarning("Character Name not found in Fade Out List on line " + currentLine + 2);
                }
                //find character image and start fade out tween
            }
        }
        if (current.Background != "" && currentLine != 0)
        {
            //change backgroundimage image
            //new image on top, .DOFade image on top
            Image prevBackground = currentBackground;
            currentBackground = GameObject.Instantiate(BackgroundPrefab, BackgroundsParent.transform).GetComponent<Image>();
            currentBackground.sprite = backgroundDictionary[current.Background];
            currentBackground.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            currentBackground.DOFade(1, 2.0f).OnComplete(() =>
            {
                GameObject.Destroy(prevBackground.gameObject);
            });
        }
        if (current.Music != "")
        {
            //music
            Tween musicTween = Box.DOFade(Box.color.a, 0.1f);
            musicTween.onComplete = () => { Debug.Log("ChangeMusic: " + current.Music); };
            tweenSequence.Append(musicTween);
        }
        Image i = Box.GetComponent<Image>();
        Tween t = Box.DOFade(Box.color.a, 0.1f);
        t.onComplete = () => {
            if (current.Sound != "")
            {
                Debug.Log("ChangeSound: " + current.Sound);
            }
            if (current.ExclaimTextBox == (Box.GetComponent<Image>().sprite.name == NORMAL_TEXTBOX_NAME))
            {
                if (current.ExclaimTextBox)
                {
                    Debug.Log("ExclaimTextBox");
                }
                else
                {
                    Debug.Log("NormalTextBox");
                }
            }
        };
        tweenSequence.Append(t);
        if (current.FadeInList != null && current.FadeInList.Length > 0)
        {
            foreach (string character in current.FadeInList)
            {
                //find character image and start fade out tween
                string[] characterArray = character.Split(' ');
                Image currentCharacter;
                string characterName = characterArray[0].Split('_')[0];
                if (characterDictionary.ContainsKey(characterName)){
                    currentCharacter = characterDictionary[characterName];
                    tweenSequence.Join(Box.DOFade(Box.color.a, 0.5f).OnComplete(() => { currentCharacter.sprite = spriteDictionary[characterArray[0]]; }));                    
                }
                else
                {
                    currentCharacter = GameObject.Instantiate(SpritesPrefab, SpritesParent.transform).GetComponent<Image>();
                    currentCharacter.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    if (characterArray.Length > 1)
                    {
                        currentCharacter.rectTransform.anchoredPosition = new Vector2((int.Parse(characterArray[1]) * 500) - 1500, 0);
                    }
                    characterDictionary[characterName] = currentCharacter;
                    tweenSequence.Join(currentCharacter.DOFade(1.0f, 1.0f));
                    currentCharacter.sprite = spriteDictionary[characterArray[0]];
                }
            }
        }
        Tween nameTween = Box.DOFade(Box.color.a, 0.1f);
        nameTween.onComplete = () => {
            Tween plateTween = null;
            if(NameText.text != current.Character && current.Character != ""){
                if(NameText.text == ""){
                    //character from narrator, tween up from behind
                    plateTween = NamePlate.rectTransform.DOAnchorPosY(0, 0.3f, true);
                    plateTween.onComplete = () =>
                    {
                        currentCharacter = 0;
                        timer = 0;
                        active = true;
                    };
                    NameText.text = current.Character;
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(NamePlate.rectTransform);
                    Color plateColor;
                    ColorUtility.TryParseHtmlString(CharacterData.CharacterPlateColor(current.Character), out plateColor);
                    NamePlate.GetComponent<Image>().color = plateColor;
                }
                else{
                    //new character speaking, tween across and lift new plate up
                    NamePlate.rectTransform.DOAnchorPosX(100, 0.2f, true);
                    NameText.DOFade(0, 0.2f);
                    NamePlate.DOFade(0, 0.2f).OnComplete(() => {
                        NamePlate.rectTransform.anchoredPosition = new Vector2(0, -NamePlate.rectTransform.sizeDelta.y);
                        NamePlate.DOFade(1, 0.01f);
                        NameText.DOFade(1, 0.01f);
                        plateTween = NamePlate.rectTransform.DOAnchorPosY(0, 0.3f);
                        plateTween.onComplete = () =>
                        {
                            currentCharacter = 0;
                            timer = 0;
                            active = true;
                        };
                        NameText.text = current.Character;
                        Canvas.ForceUpdateCanvases();
                        LayoutRebuilder.ForceRebuildLayoutImmediate(NamePlate.rectTransform);
                        Color plateColor;
                        ColorUtility.TryParseHtmlString(CharacterData.CharacterPlateColor(current.Character), out plateColor);
                        NamePlate.GetComponent<Image>().color = plateColor;
                    });
                }
            }
            else if(current.Character == ""){
                //narrator, move nameplate underneath text box
                plateTween = NamePlate.rectTransform.DOAnchorPosY(-NamePlate.rectTransform.sizeDelta.y, 0.3f, true);
                plateTween.onComplete = () =>
                {
                    currentCharacter = 0;
                    timer = 0;
                    active = true;
                    NameText.text = "";
                };
            }
            else
            {
                currentCharacter = 0;
                timer = 0;
                active = true;
            }
            Canvas.ForceUpdateCanvases();
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
                moveOn = false;
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
