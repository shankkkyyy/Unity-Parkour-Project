using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachineSFX : StateMachineBehaviour {

    [System.Serializable]
    public struct SoundKey
    {
        public enum SoundType
        {
            footstep,
            slash,
            punch,
            block,
            slam, 
            attackVocal,
            growl,
            slashVocal  
        }
        public SoundType m_soundType;
        public float m_timeKey;
    }


    [SerializeField]
    SoundKey[] soundKeys;
    [SerializeField]
    StateMachineSFX.trigger whenToTrigger;

    int KeyIndex;

    private void Awake()
    {
        
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        KeyIndex = 0;
        if (whenToTrigger == StateMachineSFX.trigger.atBegin)
            PlaySound(animator);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToTrigger == StateMachineSFX.trigger.atBegin)
            return;

        float normalizedTime = stateInfo.normalizedTime;
        if (KeyIndex > soundKeys.Length - 1)
            return;
        normalizedTime = Mathf.Repeat(normalizedTime, 1);
        if (normalizedTime > soundKeys[KeyIndex].m_timeKey)
        {
            PlaySound(animator);
            KeyIndex++;
        }
    }

    void PlaySound(Animator animator)
    {
        AudioSource[] _audios = animator.gameObject.GetComponentsInChildren<AudioSource>();
        switch (soundKeys[KeyIndex].m_soundType)
        {
            case SoundKey.SoundType.footstep:
                AudioLibrary.PlayMonsterFootstep(_audios[0]);
                break;
            case SoundKey.SoundType.slash:
                AudioLibrary.PlayMonsterSlash(_audios[0]);
                break;
            case SoundKey.SoundType.punch:
                AudioLibrary.PlayMonsterInHah(_audios[1], Random.Range(0.1f, 0.2f));
                break;
            case SoundKey.SoundType.block:
                AudioLibrary.PlayMonsterBlock(_audios[0]);
                break;
            case SoundKey.SoundType.slam:
                AudioLibrary.PlayMonsterSlam(_audios[1], Random.Range(0.15f, 0.2f));                
                break;
            case SoundKey.SoundType.attackVocal:
                AudioLibrary.PlayMonsterInHah(_audios[1], Random.Range(0.1f, 0.2f), .5f);
                break;
            case SoundKey.SoundType.growl:
                AudioLibrary.PlayMonsterGrowl(_audios[1], Random.Range(0.15f, 0.2f));
                break;
            case SoundKey.SoundType.slashVocal:
                AudioLibrary.PlayMonsterSlash(_audios);
                break;

            default:
                break;
        }


    }

}
