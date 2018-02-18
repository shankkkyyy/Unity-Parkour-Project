using UnityEngine;
using System.Collections;

public class IK_Handling : MonoBehaviour {



    private Animator playerAnim;
    private CapsuleCollider playerCollider;
    private Transform Cam;
    private Hitman_BasicMove basicMove;

    private float neckRotationX, neckRotationY;
    // IK functions Enable Flags
    public bool groundIK;
    private bool InclinationGroundingIK_enable;
    private float IKweight;
    // Animator State Machines
    private bool standingIdle;
    // Tunning Parameters
    //public Vector3 leftFootOffset, rightFootOffset, leftKneeOffset, rightKneeOffset;
    //public float leftFootRotation, rightFootRotation;
    // Body Transform
    Transform rightshoulder, leftshoulder, LeftFoot, RightFoot;


    // Use this for initialization
    void Start () {
        IKweight = 0;
        playerCollider = GetComponent<CapsuleCollider>();
        playerAnim = GetComponent<Animator>();
        basicMove = GetComponent<Hitman_BasicMove>();
        Cam = Camera.main.transform;
        rightshoulder = playerAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        leftshoulder = playerAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        LeftFoot = playerAnim.GetBoneTransform(HumanBodyBones.LeftFoot);
        RightFoot = playerAnim.GetBoneTransform(HumanBodyBones.RightFoot);

    }
	
	// Update is called once per frame
	void Update () {
        updateAnimatorState();
        iKFlagUpdate();
    }
    void OnAnimatorIK()
    {
        headRotation(!playerAnim.GetBool("move"));
        //jumpOverIK(true);
        InclinationGroundingIK(groundIK && InclinationGroundingIK_enable);
    }
    private void InclinationGroundingIK(bool enable)
    {
        if (enable)
        {
            Vector3 leftFootOffset = new Vector3(-0.07f, 0.25f, 0.1f);
            Vector3 rightFootOffset = new Vector3(0.07f, 0.25f, 0.1f);
            Vector3 leftKneeOffset = new Vector3(-0.13f, 0, 0.15f);
            Vector3 rightKneeOffset = new Vector3(0.13f, 0, 0.15f);
            float leftFootRotationOffset = -20;
            float rightFootRotationOffset = 20;

            Ray leftFoot = new Ray(LeftFoot.position + 0.35f * transform.up, -0.5f * transform.up);
            Ray rightFoot = new Ray(RightFoot.position + 0.35f * transform.up, -0.5f * transform.up);
            Debug.DrawRay(LeftFoot.position + 0.35f * transform.up, -0.5f * transform.up, Color.white);
            Debug.DrawRay(RightFoot.position + 0.35f * transform.up, -0.5f * transform.up, Color.white);
            RaycastHit leftFootHit, rightFootHit;
            Physics.Raycast(leftFoot, out leftFootHit, 1f);
            Physics.Raycast(rightFoot, out rightFootHit, 1f);

            float HeighDiff_LF_RF = leftFootHit.point.y - rightFootHit.point.y; // height difference between left and right
            //Debug.Log(HeighDiff_LF_RF);

            // Apply the IK on the foot with higher posistion if the height difference between left foot and right foot is big
            if (Mathf.Abs(HeighDiff_LF_RF)< 0.15f)
            {
                playerCollider.center = Vector3.Lerp(playerCollider.center, new Vector3(0, playerCollider.center.y, playerCollider.center.z), 2 * Time.fixedDeltaTime);
                return;
            }
            else if (leftFootHit.point.y > rightFootHit.point.y) // Left foot is higher
            {
                //Debug.Log("leftup");
                //Foot Rotation
                playerAnim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, IKweight);
                playerAnim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.FromToRotation(transform.up, leftFootHit.normal) * Quaternion.Euler(0, leftFootRotationOffset, 0) * transform.rotation);
                //Foot Position
                playerAnim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, IKweight);
                //Set Offset for Foot relate to charactor
                playerAnim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootHit.point + transform.TransformDirection(leftFootOffset));
                //Knee Position
                playerAnim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, IKweight);
                //Set Offset for Knee relate to charactor
                playerAnim.SetIKHintPosition(AvatarIKHint.LeftKnee, playerAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position + transform.TransformDirection(leftKneeOffset));

                //Adjust center of collider on lower foot _ right
                playerCollider.center = Vector3.Lerp(playerCollider.center, new Vector3(0.35f, playerCollider.center.y, playerCollider.center.z), 2*Time.fixedDeltaTime);
            }
            else  // Right foot is higher
            {
                //Debug.Log("rightup");
                //Foot Rotation
                playerAnim.SetIKRotationWeight(AvatarIKGoal.RightFoot, IKweight);
                playerAnim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.FromToRotation(transform.up, rightFootHit.normal) * Quaternion.Euler(0, rightFootRotationOffset, 0) * transform.rotation);
                //Foot Position
                playerAnim.SetIKPositionWeight(AvatarIKGoal.RightFoot, IKweight);
                //Set Offset for Foot relate to charactor
                playerAnim.SetIKPosition(AvatarIKGoal.RightFoot, rightFootHit.point + transform.TransformDirection(rightFootOffset));
                //Knee Position
                playerAnim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, IKweight);
                //Set Offset for Knee relate to charactor
                playerAnim.SetIKHintPosition(AvatarIKHint.RightKnee, playerAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position + transform.TransformDirection(rightKneeOffset));
                //Adjust center of collider on lower foot _ right
                playerCollider.center = Vector3.Lerp(playerCollider.center, new Vector3(-0.35f, playerCollider.center.y, playerCollider.center.z), 2*Time.fixedDeltaTime);
            }
        }
        else
        {
            playerCollider.center = Vector3.Lerp(playerCollider.center, new Vector3(0, playerCollider.center.y, playerCollider.center.z), 2*Time.fixedDeltaTime);     
            return;
        }
            
    }
    //private void stepOn()
    //{
    //    RaycastHit hitInfoZ;
    //    Physics.Raycast(transform.position + transform.up * 0.5f, transform.forward, out hitInfoZ, 1.2f);
    //    Debug.DrawRay(transform.position + transform.up * 0.5f, 1.2f * transform.forward, Color.blue);

    //    if (hitInfoZ.collider == null)
    //    {
    //        playerAnim.SetBool("stepOn", false);
    //        return;
    //    }
    //    else
    //    {
    //        RaycastHit hitInfoY;
    //        Physics.Raycast(transform.position + transform.up * 1.5f + (hitInfoZ.distance + 0.3f) * transform.forward, -transform.up, out hitInfoY, 1.1f);
    //        Debug.DrawRay(transform.position + transform.up * 1.5f + (hitInfoZ.distance + 0.3f) * transform.forward, -1.1f * transform.up, Color.red);

    //        if (hitInfoY.collider == null || !playerAnim.GetBool("move"))
    //        {
    //            playerAnim.SetBool("stepOn", false);
    //            return;
    //        }
    //        else
    //        {
    //            float x,z;
    //            playerAnim.SetBool("stepOn", true);
    //            stepOnPos = hitInfoY.point;

    //            x = Mathf.Lerp(transform.position.x, stepOnPos.x, Time.fixedDeltaTime);
    //            z = Mathf.Lerp(transform.position.z, stepOnPos.z, Time.fixedDeltaTime);
    //            transform.position = new Vector3(x, transform.position.y, z);
                


    //            //transform.Translate(Time.fixedDeltaTime*10 * (new Vector3 (2*(hitInfoY.point.x - transform.position.x), hitInfoY.point.y - transform.position.y, 5*(hitInfoY.point.z- transform.position.z))),transform);
    //        }
    //    }
    //}

    // Head Rotation
    private void headRotation(bool enable)
    {
        float rotationRef = 0;
        
        if (enable)
        {
            neckRotationUpdate();
        }
        else
        {
            neckRotationX = Mathf.SmoothDamp(neckRotationX, 0, ref rotationRef, 0.05f);
        }
        playerAnim.SetBoneLocalRotation(HumanBodyBones.Neck, Quaternion.Euler(neckRotationY, neckRotationX, 0));
    }
    private void neckRotationUpdate()
    {
        float rotationX, rotationY, angleX, angleY, directionX = 0, directionY = 0, diffX, diffY = 0, rotationRefX = 0, rotationRefY = 0;
        diffX = Quaternion.FromToRotation(transform.forward, Cam.forward).eulerAngles.y;
        // direction for turning right or left
        directionX = (diffX < 90 || (diffX > 180 && diffX < 270)) ? 1 : -1;

        if (Vector3.Angle(Vector3.Scale(Cam.forward, new Vector3(1, 0, 1)), transform.forward) <= 90 && Vector3.Angle(Vector3.Scale(Cam.forward, new Vector3(1, 0, 1)), transform.forward) >= -90)
        {
            angleX = Mathf.Clamp(Vector3.Angle(Vector3.Scale(Cam.forward, new Vector3(1, 0, 1)), transform.forward), -60, 60);
            directionY = -1;
        }
        else 
        {
            angleX = Mathf.Clamp(180 - Vector3.Angle(Vector3.Scale(Cam.forward, new Vector3(1, 0, 1)), transform.forward), -60, 60);
            directionY = 1;
        }
        rotationX = directionX * angleX;


        //direction for rising or lowing head
        diffY = Cam.transform.rotation.eulerAngles.x - transform.rotation.eulerAngles.x;
        angleY = diffY;
        if (diffY < 50 && diffY > 30)
            {
                rotationY = -angleY / 1.5f * (directionY);
            }
        else if (angleY < 350 && angleY > 300) //up
            rotationY = (360 - angleY) * (directionY);
        else rotationY = 0;             
        neckRotationX = Mathf.SmoothDamp(neckRotationX, rotationX, ref rotationRefX, 0.1f);
        neckRotationY = Mathf.SmoothDamp(neckRotationY, rotationY, ref rotationRefY, 0.1f);
    }
    private void jumpOverIK(bool enable)
    {
        

        if (!enable)
        {
            return;
        }
        else
        {
            RaycastHit rightHandHit, leftHandHit;
            Physics.Raycast(rightshoulder.position, rightshoulder.up, out rightHandHit, 1.2f);
            Physics.Raycast(leftshoulder.position, leftshoulder.up, out leftHandHit, 1.2f);

            if (rightHandHit.collider == null)
            {
                if (leftHandHit.collider == null)
                {
                    return;
                }
                else
                {
                    float leftHandHeightOffset = (leftHandHit.collider.GetComponent<Transform>().localScale.y > 1) ? (leftHandHit.collider.GetComponent<Transform>().localScale.y - 1) * 0.05f : 0;
                    playerAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, playerAnim.GetFloat("IKweight"));
                    playerAnim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHit.point + new Vector3(0, leftHandHeightOffset, 0));
                }
            }
            else
            {
                float rightHandHeightOffset = (rightHandHit.collider.GetComponent<Transform>().localScale.y > 1) ? (rightHandHit.collider.GetComponent<Transform>().localScale.y - 1) * 0.05f : 0;
                playerAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, playerAnim.GetFloat("IKweight"));
                playerAnim.SetIKPosition(AvatarIKGoal.RightHand, rightHandHit.point + new Vector3(0, rightHandHeightOffset, 0));
            }



        }

    }
    private void updateAnimatorState()
    {

        AnimatorStateInfo animatorState_0 = playerAnim.GetCurrentAnimatorStateInfo(0);
        standingIdle = animatorState_0.IsName("StandingIdel");
    }
    private void iKFlagUpdate()
    {
        InclinationGroundingIK_enable = standingIdle && !basicMove.isFlat;
        if (InclinationGroundingIK_enable)
        {
            IKweight = Mathf.Lerp(IKweight, 1, 10 * Time.fixedDeltaTime);
        }
        else
        {
            IKweight = Mathf.Lerp(IKweight, 0.5f, 10 * Time.fixedDeltaTime);
        }        
    }

}
