using UnityEngine;
using System.Collections;

public class Hitman_IK : MonoBehaviour {

    Animator m_Anim;
    [SerializeField, ReadOnly] float weight;
    Vector3 leftHandIKpos, rightHandIKpos;
    Transform rightHand, leftHand;
    bool useIK;
    


	// Use this for initialization
	void Start () {

        m_Anim = GetComponent<Animator>();
        rightHand = m_Anim.GetBoneTransform(HumanBodyBones.RightHand);
        leftHand = m_Anim.GetBoneTransform(HumanBodyBones.LeftHand);
        useIK = false;

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnAnimatorIK(int layerIndex)
    {
        if (!useIK)
            return;
        weight = m_Anim.GetFloat("IKWeight");
        m_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
        m_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
        m_Anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKpos);
        m_Anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKpos);
    }





    void KeepHandsAtHere()
    {
        useIK = true;
        leftHandIKpos = leftHand.position;
        rightHandIKpos = rightHand.position;
    }
    void ReleaseHands()
    {
        useIK = false;
    }
}
