using UnityEngine.UI;
using UnityEngine;

public class GameOptionManager : MonoBehaviour {



    static Dropdown difficulty;
    static Slider[] vols; // 0 = BGM
    static Toggle inGameTip;
    bool doOnce;
    
    void OnEnable()
    {

        if (!doOnce)
        {
            difficulty = this.GetComponentInChildren<Dropdown>();
            vols = this.GetComponentsInChildren<Slider>();
            inGameTip = this.GetComponentInChildren<Toggle>();
            difficulty.value = (int)GameManager.GameDifficulty;

            float BGMVolumn, SFXVolumn;
            GameManager.audioMixer.GetFloat("BGMVolume", out BGMVolumn);
            GameManager.audioMixer.GetFloat("SFXVolume", out SFXVolumn);
            vols[0].value = BGMVolumn + 80;
            vols[1].value = SFXVolumn + 80;
            inGameTip.isOn = GameManager.InGameTip;
            doOnce = true;
        }

    }

    public static void ApplySettingsOnEsc()
    {
        GameManager.GameDifficulty = (GameManager.GameMode)difficulty.value;
        GameManager.InGameTip = inGameTip.isOn;

        switch (GameManager.GameDifficulty)
        {
            case GameManager.GameMode.relaxed:
                CombatSlots.CommonEnemiesAttackFreq = 3f;
                Mutant.SetMutantCombatProp(1.5f, 7f, 50f, 4);
                break;
            case GameManager.GameMode.expert:
                CombatSlots.CommonEnemiesAttackFreq = 2f;
                Mutant.SetMutantCombatProp(1.2f, 6f, 40f, 4);
                break;
            case GameManager.GameMode.insane:
                CombatSlots.CommonEnemiesAttackFreq = 1f;
                Mutant.SetMutantCombatProp(1f, 5f, 33f, 3);
                break;
        }

    }

    public void ApplySettings()
    {
        GameManager.GameDifficulty = (GameManager.GameMode)difficulty.value;
        GameManager.InGameTip = inGameTip.isOn;


        switch (GameManager.GameDifficulty)
        {
            case GameManager.GameMode.relaxed:
                CombatSlots.CommonEnemiesAttackFreq = 3f;
                Mutant.SetMutantCombatProp(2f, 7f, 50f, 4);
                break;
            case GameManager.GameMode.expert:
                CombatSlots.CommonEnemiesAttackFreq = 2f;
                Mutant.SetMutantCombatProp(1.5f, 6f, 40f, 4);
                break;
            case GameManager.GameMode.insane:
                CombatSlots.CommonEnemiesAttackFreq = 1f;
                Mutant.SetMutantCombatProp(1f, 5f, 33f, 3);
                break;
        }

    }

    public void AdjustAudio()
    {
        GameManager.SetAudioVolumn(vols[1].value - 80, vols[0].value - 80);
    }
}
