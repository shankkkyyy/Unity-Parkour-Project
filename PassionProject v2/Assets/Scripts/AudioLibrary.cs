using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : MonoBehaviour {



    static AudioClip[]
        humanFootSteps,
        actionVocal,
        humanLand,
        combatAirWhoosh,
        combatBadManYell,
        combatGoodManYell,
        combatImpact,
        mutant,
        gems,
        BGM;

    static AudioClip
        humanJump,
        playNext;
    static AudioSource
        environmentSFX, 
        backgroundMusic;

    static int BGMindex;
    static bool BGMFadeOut, BGMFadeIn;

    void Awake()
    {
        humanFootSteps = new AudioClip[4];
        actionVocal    = new AudioClip[2];
        humanLand      = new AudioClip[3];
        combatAirWhoosh = new AudioClip[11];
        combatBadManYell = new AudioClip[12];
        combatGoodManYell = new AudioClip[7];
        combatImpact = new AudioClip[8];
        mutant       = new AudioClip[12];
        BGM          = new AudioClip[5];
        gems         = new AudioClip[3];

        gems[0] = Resources.Load<AudioClip>("Audio/SFX/Interact/LootGem");
        gems[1] = Resources.Load<AudioClip>("Audio/SFX/Interact/button");
        gems[2] = Resources.Load<AudioClip>("Audio/SFX/Interact/Teleport");

        humanFootSteps[0] = Resources.Load<AudioClip>("Audio/SFX/Movement/Footstep01");
        humanFootSteps[1] = Resources.Load<AudioClip>("Audio/SFX/Movement/Footstep02");
        humanFootSteps[2] = Resources.Load<AudioClip>("Audio/SFX/Movement/Footstep03");
        humanFootSteps[3] = Resources.Load<AudioClip>("Audio/SFX/Movement/Footstep04");

        actionVocal[0] = Resources.Load<AudioClip>("Audio/SFX/Movement/ActionVocal_1");
        actionVocal[1] = Resources.Load<AudioClip>("Audio/SFX/Movement/ActionVocal_2");

        humanLand[0] = Resources.Load<AudioClip>("Audio/SFX/Movement/FootStepLand");
        humanLand[1] = Resources.Load<AudioClip>("Audio/SFX/Movement/JumpLandingVocal_1");
        humanLand[2] = Resources.Load<AudioClip>("Audio/SFX/Movement/JumpLandingVocal_2");

        humanJump = Resources.Load<AudioClip>("Audio/SFX/Movement/FootStepJump");

        combatAirWhoosh[0] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/Dodge");
        combatAirWhoosh[1] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/Spin_1");
        combatAirWhoosh[2] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/Spin_2");
        combatAirWhoosh[3] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/Spin_3");
        combatAirWhoosh[4] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingFist_1");
        combatAirWhoosh[5] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingFist_2");
        combatAirWhoosh[6] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingLeg_fast");
        combatAirWhoosh[7] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingLeg_high1");
        combatAirWhoosh[8] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingLeg_high2");
        combatAirWhoosh[9] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingLeg_low1");
        combatAirWhoosh[10] = Resources.Load<AudioClip>("Audio/SFX/Fighting/AirWhoosh/SwingLeg_low2");

        combatGoodManYell[0] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Attack_1");
        combatGoodManYell[1] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Attack_2");
        combatGoodManYell[2] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Axe");
        combatGoodManYell[3] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/FlyKick");
        combatGoodManYell[4] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Pain_1");
        combatGoodManYell[5] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Pain_2");
        combatGoodManYell[6] = Resources.Load<AudioClip>("Audio/SFX/Fighting/GoodManFightYell/Dead");

        combatBadManYell[0] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Attack_1");
        combatBadManYell[1] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Attack_2");
        combatBadManYell[2] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Attack_3");
        combatBadManYell[3] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Attack_4");
        combatBadManYell[4] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Damage_1");
        combatBadManYell[5] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Damage_2");
        combatBadManYell[6] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Damage_3");
        combatBadManYell[7] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Damage_4");
        combatBadManYell[8] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Damage_5");
        combatBadManYell[9] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Dead_1");
        combatBadManYell[10] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Dead_2");
        combatBadManYell[11] = Resources.Load<AudioClip>("Audio/SFX/Fighting/BadManFightYell/Dead_3");


        combatImpact[0] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_1");
        combatImpact[1] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_2");
        combatImpact[2] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_3");
        combatImpact[3] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_4");
        combatImpact[4] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_5");
        combatImpact[5] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_Hard_1");
        combatImpact[6] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_Hard_2");
        combatImpact[7] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Impact/Punch_Hard_3");

        mutant[0]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Mutant_footstep_1");
        mutant[1]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Mutant_footstep_2");
        mutant[2]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Dumpster");
        mutant[3]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/attack_1");
        mutant[4]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/attack_2");
        mutant[5]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/attack_3");
        mutant[6]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Slash_1");
        mutant[7]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Slash_2");
        mutant[8]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/pain_1");
        mutant[9]  = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/pain_2");
        mutant[10] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/Recover");
        mutant[11] = Resources.Load<AudioClip>("Audio/SFX/Fighting/Monster/block_1");

        BGM[0] = Resources.Load<AudioClip>("Audio/BGM/fight_1");
        BGM[1] = Resources.Load<AudioClip>("Audio/BGM/fight_2");
        BGM[2] = Resources.Load<AudioClip>("Audio/BGM/fight_3");
        BGM[3] = Resources.Load<AudioClip>("Audio/BGM/parkour");
        BGM[4] = Resources.Load<AudioClip>("Audio/BGM/startMenu");

        AudioSource[] temp = GetComponents<AudioSource>();
        backgroundMusic = temp[0];
        environmentSFX = temp[1];
        environmentSFX.volume = 0.2f;
    }

    private void Update()
    {

        if (BGMFadeOut)
        {
            backgroundMusic.volume -= Time.deltaTime;
            if (backgroundMusic.volume <= 0f)
            {
                BGMFadeOut = false;
                BGMFadeIn = true;
                backgroundMusic.clip = playNext;
                backgroundMusic.Play();
            }
        }
        else if (BGMFadeIn)
        {
            backgroundMusic.volume += Time.deltaTime;
            if (backgroundMusic.volume >= 1)
                BGMFadeIn = false;
        }

        switch (GameManager.PlayerLocation)
        {
            case GameManager.GameLocation.energyYard:
                break;
            case GameManager.GameLocation.douglasYard:
            case GameManager.GameLocation.mutantYard:
                while (!backgroundMusic.isPlaying)
                {
                    BGMindex++;
                    if (BGMindex >= 3)
                        BGMindex = 0;
                    backgroundMusic.clip = BGM[BGMindex];
                    backgroundMusic.Play();
                }
                break;
        }
    }


    #region 1. Footsteps && Motion
    public static void PlayHumanGrunt(AudioSource _audioSource)
    {
        _audioSource.volume = Random.Range(0.1f, 0.2f);
        _audioSource.clip = actionVocal[Random.Range(0, actionVocal.Length)];
        _audioSource.Play();
    }
    public static void PlayHumanJump(AudioSource _audioSource)
    {
        _audioSource.volume = 0.2f;
        _audioSource.clip = humanJump;
        _audioSource.Play();

    }
    public static void PlayHumanJump(AudioSource[] _audioSource)
    {
        _audioSource[0].volume = 0.2f;
        _audioSource[0].clip = humanJump;
        _audioSource[0].Play();
        if (Random.Range(0f, 1f) > 0.5f)
        {
            _audioSource[1].volume = 0.2f;
            _audioSource[1].clip = actionVocal[Random.Range(0, 2)];
            _audioSource[1].Play();
        }
    }
    public static void PlayHumanJump(AudioSource[] _audioSource, int _index)
    {
        _audioSource[0].volume = 0.2f;
        _audioSource[0].clip = humanJump;
        _audioSource[0].Play();
        if (Random.Range(0f, 1f) > 0.5f)
        {
            _audioSource[1].volume = 0.2f;
            _audioSource[1].clip = actionVocal[_index];
            _audioSource[1].Play();
        }
    }
    public static void PlayHumanFootStep(AudioSource _audioSource)
    {
        _audioSource.volume = Random.Range(.1f, .2f);
        _audioSource.clip = humanFootSteps[Random.Range(0, humanFootSteps.Length)];
        _audioSource.Play();
    }
    public static void PlayHumanFootStep(AudioSource _audioSource, float _speed)
    {
        float minVol = 0;
        float segment = 0.03f;
        if (_speed < 1f)
            minVol = 0f;
        else if (_speed < 2.5f)
            minVol = .03f;
        else
        {
            segment = 0.06f;
            minVol = .1f;
        } 
        _audioSource.volume = Random.Range(minVol, segment * _speed);
        _audioSource.clip = humanFootSteps[Random.Range(0, humanFootSteps.Length)];
        _audioSource.Play();
    }
    public static void PlayHumanLanding(AudioSource _audioSource)
    {
        _audioSource.clip = humanLand[0];
        _audioSource.volume = Random.Range(0.15f, .25f);
        _audioSource.Play();
    }
    public static void PlayHumanLanding(AudioSource[] _audioSource, bool _fromHigh)
    {
        _audioSource[0].clip = humanLand[0];
        if (_fromHigh)
        {
            _audioSource[0].volume = Random.Range(0.15f, .25f);
            _audioSource[0].Play();
            _audioSource[1].clip = humanLand[2];
            _audioSource[1].volume = 0.2f;
            _audioSource[1].Play();
        }
        else
        {
            _audioSource[0].volume = Random.Range(0.1f, .15f);
            _audioSource[0].Play();
            if (Random.Range(0f, 1f) > .7f)
            {
                _audioSource[1].clip = humanLand[1];
                _audioSource[1].volume = 0.1f;
                _audioSource[1].Play();
            }
        }
    }
    #endregion

    static public void PlayDodge(AudioSource[] _audioSource)
    {
        //movement
        _audioSource[0].clip = combatAirWhoosh[9];
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        ////vocal
        PlayGoodManHmn(_audioSource[1], Random.Range(0.1f, 0.2f), 0.5f);
    }
    static public void PlayPunchAir(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = combatAirWhoosh[Random.Range(4, 6)];
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        PlayGoodManHmn(_audioSource[1], Random.Range(0.1f, 0.2f), 0.3f);
    }
    static public void PlayLowKick(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = combatAirWhoosh[Random.Range(9, 11)];
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        PlayGoodManHeh(_audioSource[1], Random.Range(0.1f, 0.2f), 0.3f);
    }
    static public void PlayHighKick(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = combatAirWhoosh[Random.Range(6, 9)];
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        PlayGoodManHeh(_audioSource[1], Random.Range(0.1f, 0.2f), 0.3f);
    }
    static public void PlaySpin(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = combatAirWhoosh[Random.Range(1, 4)];
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        PlayGoodManHmn(_audioSource[1], Random.Range(0.1f, 0.2f), 0.3f);
    }
    static public void PlayHeavyAttack(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = combatAirWhoosh[Random.Range(4, 6)]; 
        _audioSource[0].volume = Random.Range(0.05f, 0.1f);
        _audioSource[0].Play();
        PlayGoodManHah(_audioSource[1], Random.Range(0.1f, 0.2f), 1);

    }
    static public void PlayFistFightImpact(AudioSource _audioSource)
    {
        _audioSource.clip = combatImpact[Random.Range(0, 6)];
        _audioSource.volume = Random.Range(0.05f, 0.1f);
        _audioSource.Play();
    }
    static public void PlayBadManStruggle(AudioSource _as, float volumn, float chance)
    {

        if (chance >= 0.5f)
        {
            _as.clip = combatBadManYell[Random.Range(9, 12)];
            _as.volume = volumn;
            _as.Play();

        }
        else if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = combatBadManYell[Random.Range(4, 8)];
            _as.volume = volumn;
            _as.Play();
        }
    }
    static public void PlayGoodManStruggle(AudioSource _as, float volumn, float chance)
    {
        if (chance >= 0.5f)
        {
            _as.clip = combatGoodManYell[6];
            _as.volume = volumn;
            _as.Play();

        }
        else if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = combatGoodManYell[Random.Range(4, 6)];
            _as.volume = volumn;
            _as.Play();
        }
    }



    static public void PlayMonsterFootstep(AudioSource _audioSource)
    {
        _audioSource.volume = Random.Range(.15f, .22f);
        _audioSource.clip = mutant[Random.Range(0, 2)];
        _audioSource.Play();
    }
    static public void PlayMonsterSlash(AudioSource _audioSource)
    {
        _audioSource.volume = Random.Range(.15f, .22f);
        _audioSource.clip = mutant[Random.Range(6, 8)];
        _audioSource.Play();
    }
    static public void PlayMonsterSlash(AudioSource[] _audioSource)
    {
        _audioSource[0].clip = mutant[Random.Range(6, 8)];
        _audioSource[0].volume = Random.Range(.15f, .22f);
        _audioSource[0].Play();
        PlayMonsterInHah(_audioSource[1], Random.Range(0.1f, 0.2f), .3f);

    }

    static public void PlayMonsterBlock(AudioSource _audioSource)
    {
        _audioSource.volume = Random.Range(.15f, .22f);
        _audioSource.clip = mutant[11];
        _audioSource.Play();
    }
    static public void PlayMonsterInHah(AudioSource _as, float volumn)
    {
            _as.clip = mutant[Random.Range(3, 6)];
            _as.volume = volumn;
            _as.Play();
    }

    static public void PlayMonsterInHah(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = mutant[Random.Range(3, 6)];
            _as.volume = volumn;
            _as.Play();
        }
    }

    static public void PlayMonsterInPain(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = mutant[8];
            _as.volume = volumn;
            _as.Play();
        }
    }
    static public void PlayMonsterDead(AudioSource _as, float volumn)
    {
        _as.clip = mutant[9];
        _as.volume = volumn;
        _as.Play();

    }
    static public void PlayMonsterGrowl(AudioSource _as, float volumn)
    {
        _as.clip = mutant[10];
        _as.volume = volumn;
        _as.Play();
    }
    static public void PlayMonsterSlam(AudioSource _as, float volumn)
    {
        _as.clip = mutant[2];
        _as.volume = volumn;
        _as.Play();
    }

    // Helper 

    static void PlayGoodManHmn(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = actionVocal[0];
            _as.volume = volumn;
            _as.Play();
        }
    }
    static void PlayGoodManHeh(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = combatGoodManYell[Random.Range(0,2)];
            _as.volume = volumn;
            _as.Play();
        }
    }
    static void PlayGoodManHah(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = combatGoodManYell[Random.Range(2, 4)];
            _as.volume = volumn;
            _as.Play();
        }
    }
    static public void PlayBadManHeh(AudioSource _as, float volumn, float chance)
    {
        if (Random.Range(0f, 1f) < chance)
        {
            _as.clip = combatBadManYell[Random.Range(0, 4)];
            _as.volume = volumn;
            _as.Play();
        }
    }


    //BGM
    public static void PlayTheme()
    {
        backgroundMusic.clip = BGM[4];
        backgroundMusic.Play();
        backgroundMusic.loop = true;
    }
    public static void SwitchingBGM(GameManager.GameLocation _playerLocation)
    {
        switch (_playerLocation)
        {
            case GameManager.GameLocation.energyYard:
                playNext = BGM[3];
                backgroundMusic.loop = true;
                break;
            case GameManager.GameLocation.douglasYard:
            case GameManager.GameLocation.mutantYard:
                BGMindex = Random.Range(0, 3);
                playNext = BGM[BGMindex];
                backgroundMusic.loop = false;
                break;
            default:
                break;
        }
        BGMFadeOut = true;
    }

    //Environmental SFX
    public static void PlayEnvironmentSound(int _i)
    {
        environmentSFX.clip = gems[_i];
        environmentSFX.Play();
    }
    public void PlayButtonSound()
    {
        environmentSFX.clip = gems[1];
        environmentSFX.Play();
    }





}
