using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBodyYPose : StateMachineBehaviour {

    [SerializeField]
    bool freezeWhenIn = true, unFreezeWhenOut = true;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (freezeWhenIn)
        {
            Rigidbody rbody = animator.GetComponent<Rigidbody>();
            rbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unFreezeWhenOut)
        {
            Rigidbody rbody = animator.GetComponent<Rigidbody>();
            rbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
