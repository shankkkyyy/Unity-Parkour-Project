using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineSFX : StateMachineBehaviour {

    [System.Serializable]
    public struct SoundKey
    {
        public enum SoundType
        {
            landingVocal,
            landingVocal_high,
            movementVocal,
            footstepDynamic,
            footstepStatic,
            movement,
            grunt,
            landingFootStep,
            dodge,
            spin,
            combatFootStep,
            punch,
            lowKick,
            highKick,
            spinKick,
            heavyAttack
        }
        public SoundType m_soundType;
        public float     m_timeKey;
    }
    public enum trigger
    {
        inMid,
        atBegin
    }

    [SerializeField]
    SoundKey[] soundKeys;
    [SerializeField]
    trigger whenToTrigger;

    int KeyIndex;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        KeyIndex = 0;
        if (whenToTrigger == trigger.atBegin)
            PlaySound(animator);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToTrigger == trigger.atBegin)
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

    void PlaySound(Animator _animator)
    {     
        //AudioSource[] audioSouce = _animator.GetComponents<AudioSource>();
        switch (soundKeys[KeyIndex].m_soundType)
        {           
             
            case SoundKey.SoundType.landingVocal:
                AudioLibrary.PlayHumanLanding(Hitman_BasicMove.Audio, false);
                break;
            case SoundKey.SoundType.landingVocal_high:
                AudioLibrary.PlayHumanLanding(Hitman_BasicMove.Audio, true);
                break;
            case SoundKey.SoundType.movementVocal:
                AudioLibrary.PlayHumanJump(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.footstepDynamic:
                AudioLibrary.PlayHumanFootStep(Hitman_BasicMove.Audio[0], _animator.GetFloat("speed"));
                break;
            case SoundKey.SoundType.footstepStatic:
                AudioLibrary.PlayHumanFootStep(Hitman_BasicMove.Audio[0]);
                break;
            case SoundKey.SoundType.movement:
                AudioLibrary.PlayHumanJump(Hitman_BasicMove.Audio[0]);
                break;
            case SoundKey.SoundType.grunt:
                AudioLibrary.PlayHumanGrunt(Hitman_BasicMove.Audio[0]);
                break;
            case SoundKey.SoundType.landingFootStep:
                AudioLibrary.PlayHumanLanding(Hitman_BasicMove.Audio[0]);
                break;
            case SoundKey.SoundType.dodge:
                AudioLibrary.PlayDodge(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.spin:
                AudioLibrary.PlayHumanJump(Hitman_BasicMove.Audio, 0);
                break;
            case SoundKey.SoundType.combatFootStep:
                AudioLibrary.PlayHumanFootStep(Hitman_BasicMove.Audio[0], 1);
                break;
            case SoundKey.SoundType.punch:
                AudioLibrary.PlayPunchAir(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.lowKick:
                AudioLibrary.PlayLowKick(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.highKick:
                AudioLibrary.PlayHighKick(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.spinKick:
                AudioLibrary.PlaySpin(Hitman_BasicMove.Audio);
                break;
            case SoundKey.SoundType.heavyAttack:
                AudioLibrary.PlayHeavyAttack(Hitman_BasicMove.Audio);
                break;
            default:
                break;
        }
    }
}
