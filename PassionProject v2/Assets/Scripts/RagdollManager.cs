using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour {

    // Use this for initialization
    public enum RagRollState
    {
        IsRagDoll,
        InTransistion,
        NotRagDoll,
    }
    class poseBodyPart
    {
        public Transform myTrans;
        public Vector3 myPos;
        public Quaternion myRot;
    }


    Animator anim;
    List<poseBodyPart> poseBody;
    Rigidbody m_body;
    CapsuleCollider m_Collider;
    [SerializeField] Rigidbody[]            dollRigidbody;
    [SerializeField, ReadOnly] RagRollState myState;
    [SerializeField] Transform              myHip;
    [SerializeField] characterStatus        myStatus;
    Vector3                                 ragDollHeadPos, ragDollFeetPos, ragDollHipPos;
    [SerializeField] float                  ragDollToStdupBlendTime = 0.5f, ragDollDuriation = 2f;
    float                                   ragDollEndTime, ragDollStartTime;
    Transform                               Head, LeftFoot, RightFoot, Hip;
    [SerializeField] bool DrawRagDollPose;

    bool shouldGetUp, rotAndPosMatched, faceUp;




    void Start () {
        rotAndPosMatched = false;
        myState = RagRollState.NotRagDoll;
        anim = GetComponent<Animator>();
        m_body = GetComponent<Rigidbody>();
        m_Collider = GetComponent<CapsuleCollider>();
        m_body.mass = 0;
        #region 1. Get All the ragdoll body transform
        poseBody = new List<poseBodyPart>();
        Transform[] transformCollection = myHip.GetComponentsInChildren<Transform>();
        foreach (var item in transformCollection)
        {
            poseBodyPart poseBodyPart = new poseBodyPart();
            poseBodyPart.myTrans = item;
            poseBody.Add(poseBodyPart);
        }
        Head = anim.GetBoneTransform(HumanBodyBones.Head);
        Hip = anim.GetBoneTransform(HumanBodyBones.Hips);
        LeftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        #endregion
        #region 2. Get all the ragdoll rigidbody
        foreach (var item in dollRigidbody)
        {
            item.isKinematic = true;
            m_body.mass += item.mass;
        }
        #endregion
    }

    private void Update()
    {
        GetUp();
        if (DrawRagDollPose)
        {
            Debug.DrawRay(ragDollHipPos, 5 * transform.up, Color.cyan);
            Debug.DrawRay(transform.position, 5 * transform.up, Color.blue);
        }
    }
    private void LateUpdate()
    {
        if (myState == RagRollState.InTransistion)
        {
            float ragdollBlendAmount = 1 - (Time.time - ragDollEndTime) / ragDollToStdupBlendTime;

            #region 1. Match the last pose of ragdoll and get up anim in transform
            if (!rotAndPosMatched)
            {
                // Match the Position
                RaycastHit hitInfo;
                if (Physics.Raycast(ragDollHipPos, Vector3.down, out hitInfo, 5, 1))
                {
                    Vector3 newRootPos = ragDollHipPos;
                    newRootPos.y = hitInfo.point.y;
                    transform.position = newRootPos;
                }
                else
                {
                    transform.position = ragDollHipPos;
                    Debug.Log("hip");

                }
                // Match the rotation
                Vector3 animatedDir = transform.forward;
                Vector3 ragDollDir = (faceUp) ? ragDollFeetPos - ragDollHeadPos : ragDollHeadPos - ragDollFeetPos;
                animatedDir.y = 0;
                ragDollDir.y = 0;

                transform.rotation *= Quaternion.FromToRotation(animatedDir.normalized, ragDollDir.normalized);
                // prevent it from reversing into the ground
                this.transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                rotAndPosMatched = true;
            }
            #endregion

            #region 2. Blend the body parts transform to the last pose of ragdoll for smooth transform
            if (!rotAndPosMatched)
                return;
            foreach (var item in poseBody)
            {
                if (item.myTrans == this.transform)
                {
                    Debug.Break();
                }
                if (item.myTrans == Hip)
                    item.myTrans.position = Vector3.Lerp(item.myTrans.position, item.myPos, ragdollBlendAmount);
                item.myTrans.rotation = Quaternion.Slerp(item.myTrans.rotation, item.myRot, ragdollBlendAmount);
            }

            #endregion

            if (ragdollBlendAmount <= 0)
            {
                myState = RagRollState.NotRagDoll;
            }
        }
    }
    public void BecomeRagDoll()
    {
        myState = RagRollState.IsRagDoll;
        anim.enabled = false;
        foreach (var item in dollRigidbody)
        {
            item.isKinematic = false;
        }
        m_body.isKinematic = true;
        m_Collider.enabled = false;
        rotAndPosMatched = false;
    }
    void CoverFromRagRoll()
    {
        ragDollEndTime = Time.time;

        foreach (var item in dollRigidbody)
        {
            item.isKinematic = true;
        }
        m_body.isKinematic = false;
        m_Collider.enabled = true;

        ragDollHipPos = Hip.position;
        ragDollHeadPos = Head.position;
        ragDollFeetPos = (LeftFoot.position + RightFoot.position) * 0.5f;
        foreach (poseBodyPart item in poseBody)
        {
            item.myPos = item.myTrans.position;
            item.myRot = item.myTrans.rotation;     
        }


        anim.enabled = true;
        faceUp = (Hip.forward.y >= 0) ? true : false;
        if (faceUp)
            anim.SetTrigger("face up get up");
        else
            anim.SetTrigger("face down get up");
        myState = RagRollState.InTransistion;
    }
    public RagRollState State
    {
        get { return myState; }
    }

    protected virtual void GetUp()
    {
        if (myState == RagRollState.IsRagDoll)
        {
            if (m_body.velocity.magnitude < 0.1f && !shouldGetUp)
            {
                ragDollStartTime = Time.time;
                shouldGetUp = true;
            }
            if (Time.time - ragDollStartTime >= ragDollDuriation)
            {
                CoverFromRagRoll();
                shouldGetUp = false;
            }

        }
    }

}
