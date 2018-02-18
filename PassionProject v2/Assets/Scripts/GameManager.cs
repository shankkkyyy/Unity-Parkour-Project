using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour {

    public enum GameLocation
    {
        energyYard,
        douglasYard,
        mutantYard
    }
    public enum Quest
    {
        CollectGems,
        DefeatDouglas, 
        DefeatMutant,
        JumptoRelay
    }
    public enum GameMode
    {
        relaxed, 
        expert,
        insane
    }

    [SerializeField]
    GameObject playerParent, startMenuCam;
    static HeroStatus player;
    [SerializeField]
    // the first three will be the destination of three location, 
    // the followings are the portal location for the corresponding location on each
    Transform[] checkPoints;
    [SerializeField]
    Spawner DouglasLevel, MutantLevel;
    [SerializeField]
    int eDneedToOpenDouglasCP = 5, eDneedToOpenMutantCP = 10;
    [SerializeField]
    PickUp pickUpClone;
    static GameLocation playerLocation;

    [SerializeField, ReadOnly]
    Quest currentQuest;
    [SerializeField, ReadOnly]
    int eneryDropCollect, 
        inGameTipsIndex;
    [SerializeField]
    Text 
        uiCurrentQuestOn, 
        uiInGameTip,
        uiTitleText;
    [SerializeField]
    GameObject[] 
        UiItemsWhenUnpause,
        UiPauseMenuLayer;
    [SerializeField]
    Image fadeScreen;
    [SerializeField]
    RectTransform
        UiPauseMenuParent,
        UiStartMenu;
    static GameMode 
        gameDifficulty;
    [SerializeField, ReadOnly]
    float inGameTipsUpdateTimeGap = 5f, inGameTipsUpdateTimer = 0;
    public static AudioMixer
    audioMixer;
    string[] inGameTips;
    [SerializeField, ReadOnly]
    bool 
        dCPopened,
        mCPopened;
    static bool 
        isPaused, 
        inGameTipsIsOn, 
        gameStarted;



    void Awake()
    {
        inGameTips = new string[10];
        inGameTips[0] = "Check <i><b>How To Play</b></i> in Pause Menu to know more tricks";
        inGameTips[1] = "Hold <i><b>SPACE</b></i> to climb up continuously";
        inGameTips[2] = "Tap <i><b>LALT or Mid click</b></i> to parkour";

        inGameTips[3] = "Hold <i><b>Left Mouse</b></i> to perform continuous attack";
        inGameTips[4] = "Heavy attack cost certain amount of Stamina";
        inGameTips[5] = "Tap <i><b>LALT or Mid click</b></i> to dodge in combat";
        inGameTips[6] = "Tap F can lock on your current target";

        inGameTips[7] = "Mutant has Weak Spot on his back";
        inGameTips[8] = "Mutant will not take damage unless his shield disposed";
        inGameTips[9] = "Mutant's jump attacks can stun you, dodge them";
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        // initialize the setting
        audioMixer = Resources.Load<AudioMixer>("Audio/Master");
        gameDifficulty = GameMode.relaxed;
        inGameTipsIsOn     = true;
        SetAudioVolumn(-5, -20);
        AudioLibrary.PlayTheme();

        player = playerParent.GetComponentInChildren<HeroStatus>();
        playerLocation = GameLocation.energyYard;
        UpdateCurrentQuest(Quest.CollectGems, false);
        eneryDropCollect = 0;
        inGameTipsIndex = 0;
        uiInGameTip.text = inGameTips[inGameTipsIndex];
        inGameTipsIsOn = true;
        dCPopened = false;
        mCPopened = false;
        isPaused = false;
    }


    #region UI Func
    void Update()
    {
        if (gameStarted)
        {
            PauseFunction();
            InGameTipsUpdate();
        }
        Debug.Log(Cursor.lockState);
    }
    void PauseFunction()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            if (isPaused)
                PauseMenuOff();
            else PauseMenuOn();
        }
    }
    void InGameTipsUpdate()
    {
        if (inGameTipsIsOn)
        {
            inGameTipsUpdateTimer -= Time.deltaTime;
            if (inGameTipsUpdateTimer <= 0)
            {
                inGameTipsIndex++;
                switch (playerLocation)
                {
                    case GameLocation.energyYard:
                        if (inGameTipsIndex >= 3)
                            inGameTipsIndex = 0;
                        break;
                    case GameLocation.douglasYard:
                        if (inGameTipsIndex >= 7)
                            inGameTipsIndex = 3;

                        break;
                    case GameLocation.mutantYard:
                        if (inGameTipsIndex >= 9)
                            inGameTipsIndex = 7;
                        break;
                    default:
                        break;
                }
                uiInGameTip.text = inGameTips[inGameTipsIndex];
                inGameTipsUpdateTimer = inGameTipsUpdateTimeGap;
                StartCoroutine(TextFadeIn(uiInGameTip));
            }
        }

    }


    public void PauseMenuOn()
    {
        Cursor.lockState = CursorLockMode.Confined;
        isPaused = true;
        Time.timeScale = 0;
        Hitman_BasicMove.enablePlayerCtrl = false;

        UiPauseMenuParent.gameObject.SetActive(true);
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0.7f);
        int i = 0;
        for (i = 0; i <  UiPauseMenuLayer.Length; i++)
        {
            // make sure the first menu is one that is active while pause
            if (i == 0)
                UiPauseMenuLayer[i].SetActive(true);
            else
                UiPauseMenuLayer[i].SetActive(false);
        }
        UpdateMenuTitleText("<b>Pause Menu</b>");
    }
    public void PauseMenuOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
        Time.timeScale = 1;
        Hitman_BasicMove.enablePlayerCtrl = true;

        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0);
        UiPauseMenuParent.gameObject.SetActive(false);
        GameOptionManager.ApplySettingsOnEsc();
        ShowInGameUI();
        ExitComboList();

    }
    public void UpdateMenuTitleText(string _title)
    {
        uiTitleText.text = _title;
    }
    public void EnterComboList()
    {
        StartCoroutine("ComboListJumpInAnim");
    }
    public void ExitComboList()
    {
        UiPauseMenuParent.localPosition
       = new Vector3(UiPauseMenuParent.localPosition.x, 0, UiPauseMenuParent.localPosition.z);

    }
    IEnumerator ComboListJumpInAnim()
    {
        for (float i = 0; i <= 80; i+=3f)
        {
            float y = i;
            UiPauseMenuParent.localPosition
                = new Vector3(UiPauseMenuParent.localPosition.x, y, UiPauseMenuParent.localPosition.z);
            yield return null;
        }
    }
    #region Start New Game Process Func
    public void StartNewGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Destroy(UiStartMenu.gameObject);
        AudioLibrary.SwitchingBGM(playerLocation);
        SetAudioVolumn(-5, -30);
        StartCoroutine(NewGame());
    }
    IEnumerator NewGame()
    {
        StartCoroutine(ImageFadeOut(fadeScreen));
        // give 2 seconds to go dark
        yield return new WaitForSeconds(2);
        playerParent.SetActive(true);
        Destroy(startMenuCam);
        ShowInGameUI();
        // 1 second to stay dark
        yield return new WaitForSeconds(1);
        StartCoroutine(ImageFadeIn(fadeScreen));
        gameStarted = true;
        Hitman_BasicMove.enablePlayerCtrl = true;
        yield return null;
    }
    IEnumerator ImageFadeOut(Image _image)
    {
        for (float i = _image.color.a; i <= 1; i+= Time.deltaTime)
        {
            _image.color
                = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, i);
            yield return null;
        }
    }
    IEnumerator ImageFadeIn(Image _image)
    {
        for (float a = _image.color.a; a >= 0; a -= Time.deltaTime)
        {
            _image.color
                = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, a);
            yield return null;
        }
    }
    #endregion

    public void CloseApp()
    {
        Application.Quit();
    }

    #endregion


    #region GamePlay Func
    public void UpdateCurrentQuest(Quest _currentQuest, bool fadeIn = true)
    {
        currentQuest = _currentQuest;
        #region 1. Update Quest Text
        switch (_currentQuest)
        {
            case Quest.CollectGems:
                if (mCPopened)
                    uiCurrentQuestOn.text = "You Unlock both Relays\nYou can <b>RETURN</b> to the previous Fight through Relays";
                else if (dCPopened)
                    uiCurrentQuestOn.text = "One Relay is Opened, Run to the <color=blue><b>BLUE</b></color> Relay\nOr Collect the <color=lime><b>GEM</b></color> to UNLOCK other Relay";
                else
                    uiCurrentQuestOn.text = "Collect the <color=lime><b>GEM</b></color> to UNLOCK the Relay ";
                break;
            case Quest.DefeatDouglas:
                uiCurrentQuestOn.text = "Survive the waves of enemies";
                break;
            case Quest.DefeatMutant:
                uiCurrentQuestOn.text = "Defeat the Mutant";
                break;
            case Quest.JumptoRelay:
                switch (playerLocation)
                {
                    case GameLocation.energyYard:
                        uiCurrentQuestOn.text 
                            = "You Unlock both Relay\nRun to <color=blue><b>BLUE</b></color> Relay to fight a group of enemy\nOr Run to <color=red><b>RED</b></color> Relay to fight a Mutant";
                        break;
                    case GameLocation.douglasYard:
                        if(mCPopened)
                            uiCurrentQuestOn.text = "You Won\nRun to <color=red><b>RED</b></color> Relay to fight a Mutant\nOr Run to <color=yellow><b>YELLOW</b></color> Relay return to parkour yard";
                        else
                            uiCurrentQuestOn.text = "You Won\nRun to <color=yellow><b>YELLOW</b></color> return to parkour yard";
                        break;
                    case GameLocation.mutantYard:
                        uiCurrentQuestOn.text 
                            = "You Won\nRun to <color=blue><b>BLUE</b></color> Relay to fight a group of enemies\nOr Run to <color=yellow><b>YELLOW</b></color> Relay return to parkour yard";
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        #endregion

        #region 2. VFX to make the changing of Quest Text Noticable
        if (fadeIn)
        {
            uiCurrentQuestOn.color
                = new Color(uiCurrentQuestOn.color.r, uiCurrentQuestOn.color.g, uiCurrentQuestOn.color.b, 0);
            StartCoroutine(TextFadeIn(uiCurrentQuestOn));
        }
        #endregion
    }
    IEnumerator TextFadeIn(Text _text)
    {
        for (float i = 0; i <= 1; i+=Time.deltaTime)
        {
            _text.color 
                    = new Color(uiCurrentQuestOn.color.r, uiCurrentQuestOn.color.g, uiCurrentQuestOn.color.b, i);
            yield return null;
        }
    }
    public IEnumerator TeleportPlayerTo(GameLocation _location, bool _isRevive = false)
    {

        // clear enemy clone
        if (playerLocation == GameLocation.douglasYard)
            DouglasLevel.CleanAllChildren();
        else if (playerLocation == GameLocation.mutantYard)
            MutantLevel.CleanAllChildren();

        // Initialise the spawner prop
        if (_location == GameLocation.mutantYard)
            MutantLevel.Initialization();
        else if (_location == GameLocation.douglasYard)
            DouglasLevel.Initialization();
        // disable player control and switch bgm
        AudioLibrary.SwitchingBGM(_location);
        Hitman_BasicMove.enablePlayerCtrl = false;
        StartCoroutine(ImageFadeOut(fadeScreen));
        yield return new WaitForSeconds(2);
        // teleport

        Quest nextQuest = Quest.CollectGems;
        int locationIndex = 0;
        switch (_location)
        {
            case GameLocation.energyYard:
                nextQuest = Quest.CollectGems;
                locationIndex = 0;
                inGameTipsIndex = 0;
                break;
            case GameLocation.douglasYard:
                nextQuest = Quest.DefeatDouglas;
                locationIndex = 1;
                inGameTipsIndex = 3;
                break;
            case GameLocation.mutantYard:
                nextQuest = Quest.DefeatMutant;
                locationIndex = 2;
                inGameTipsIndex = 7;
                break;
            default:
                break;
        }
        player.transform.position = checkPoints[locationIndex].position;
        player.transform.rotation = Quaternion.identity;
        UpdateCurrentQuest(nextQuest);
        playerLocation = _location;
        player.Recover();
        yield return new WaitForSeconds(1f);
        StartCoroutine(ImageFadeIn(fadeScreen));
        Hitman_BasicMove.enablePlayerCtrl = true;
        if (_isRevive)
        {
            player.GetComponent<RagdollManager>().enabled = true;
        }
        yield return null;
    }

    //void RevivePlayer()
    //{
    //    TeleportPlayerTo(GameLocation.energyYard);
    //    player.GetComponent<RagdollManager>().enabled = true;
    //}

    public void CollectEnergyDrop()
    {
        eneryDropCollect++;
        AudioLibrary.PlayEnvironmentSound(0);
        if (eneryDropCollect >= eDneedToOpenDouglasCP)
        {
            if (!dCPopened)
            {
                dCPopened = true;
                PickUp dCPcheckPoint = Instantiate(pickUpClone, checkPoints[3].position, Quaternion.identity, null);
                dCPcheckPoint.MyProp = PickUp.ItemProp.relayToDouglas;
                UpdateCurrentQuest(Quest.CollectGems);
            }
        }

        if (eneryDropCollect >= eDneedToOpenMutantCP)
        {
            if (!mCPopened)
            {
                mCPopened = true;
                PickUp dCPcheckPoint = Instantiate(pickUpClone, checkPoints[4].position, Quaternion.identity, null);
                dCPcheckPoint.MyProp = PickUp.ItemProp.relayToMutant;
                UpdateCurrentQuest(Quest.JumptoRelay);
            }
        }
    }
    public HeroStatus PlayerStatus
    {
        get { return player;}
    }
    public bool DCPAccessable
    {
        get { return dCPopened; }
    }
    public bool MCPAccessable
    {
        get { return mCPopened; }
    }

    #endregion

    void ShowInGameUI()
    {
        int i = 0;
        for (; i < UiItemsWhenUnpause.Length; i++)
        {
            if (i == UiItemsWhenUnpause.Length - 1)
            {
                UiItemsWhenUnpause[i].SetActive(inGameTipsIsOn);
            }
            else
            {
                UiItemsWhenUnpause[i].SetActive(true);
            }
        }
    }
    static public GameLocation PlayerLocation
    {
        get { return playerLocation; }
    }
    static public bool GameIsPaused
    {
        get { return isPaused; }
    }

    static public GameMode GameDifficulty
    {
        get { return gameDifficulty; }
        set { gameDifficulty = value; }
    }

    static public bool InGameTip
    {
        get{ return inGameTipsIsOn; }
        set { inGameTipsIsOn = value; }
    }
    static public bool GameIsStart
    {
        get { return gameStarted; }
    }
    static public void SetAudioVolumn(float _sfx, float _bgm)
    {
        audioMixer.SetFloat("BGMVolume", _bgm);
        audioMixer.SetFloat("SFXVolume", _sfx);
    }

    public void LoadURL(string _url)
    {
        Application.OpenURL(_url);
    }


}
