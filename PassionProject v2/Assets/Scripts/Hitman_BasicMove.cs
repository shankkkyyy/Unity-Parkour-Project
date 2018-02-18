using System.Collections;
using UnityEngine;

public class Hitman_BasicMove : MonoBehaviour {

    Animator                   m_Anim;
    RaycastHit                 hitInfo_Ground, hitInfo_pkrTrigger, hitInfo_pkrDir;
    Rigidbody                  m_RigBody;
    FreeCam                    m_FreeCam;
    Vector3                    m_moveDirWorld, m_moveDirBody;
    Combat                     m_Combat;
    CombatSlots                m_combatSlot;
    RagdollManager             m_rM;
    CapsuleCollider            m_collider;
    // for parkour
    Transform                  m_leftHand, m_rightHand, m_leftFeet, m_rightFeet;
    Vector3                    parkourDir, parkourHorizontalLandingPos, parkourHorizontalStartingPos;

    float m_vertical, m_horizontal;    //Getting Axis Input
    float m_turn;                      //Angle between Moving Direction (Cam Direction) and Charactor Direction;
    float m_speed;                     //Velocity Magnitude for different moving state, etc walking, jogging, running
    float m_animNormalizedTime;        //Current Animation Normalized Time;
    float m_colRad_reg, m_colHeight_reg;

    float m_inclination;
    const float onGroundCheckLength = 0.5f, inAirGroundCheckLength = 0.2f;
    float groundCheckRayLength;
    

    public float turnTunning, turningMax, dampTimeV = 0.1f;
    static AudioSource[] m_audioSource;
    //0: is footstep. 1: is vocal 
    [SerializeField]
    float m_StationaryTurnSpeed = 180f, m_MovingTurnSpeed = 360f, m_fullSpeed = 3f, m_charactorSpeed = 100f,
        parkourInAngle, jumpPower = 12, combatStrffingSpd = 3f, 
        jumpUpStepDis = 3f, ClimbUpStageDis = 2.5f, ClimbUpLevelDis = 2f;

    [SerializeField, ReadOnly]
    float parkourHeightAdjustTarget, obstacle_top, obstacle_length, jumpOverMaxBoosterSpd_hor, jumpOverMaxBoosterSpd_vert,
        jumpOverHandHolderToLandingPosition, handPosition_climbUp, feetPosition, lerpStartTime, lerpEndTime, distanceToParkourObject,
        m_footTouchGround = 0.4f;

    [SerializeField, ReadOnly]
    bool m_sprintPressed, m_isJogState, m_jumpClick, m_grounded, m_stickToCurrentRotation,
        amParkouring, amSliding, goingToParkour, HeightSmoothDamper, parkourHorizonLerp,
        goingToJump, lerpFromHandToFeet, jumpDownLerp, climbUpGroundCheck,
        NOROTATION, GROUND, FIGHTMOVEMENT, PARKOUR, NO_TRANSISTION, STANDUP, GETHIT;

    // Animation Flag
    bool inIncline, inDecline, inFlat;
    static public bool enablePlayerCtrl;

    // Use this for initialization
    void Start () {

        m_Anim                = GetComponent<Animator>();
        m_RigBody             = GetComponent<Rigidbody>();
        m_Combat              = GetComponent<Combat>();
        m_combatSlot          = GetComponentInChildren<CombatSlots>();
        m_FreeCam             = Camera.main.GetComponentInParent<FreeCam>();
        m_rM                  = GetComponent<RagdollManager>();
        m_collider            = GetComponent<CapsuleCollider>();
        m_leftHand            = m_Anim.GetBoneTransform(HumanBodyBones.LeftHand);
        m_rightHand           = m_Anim.GetBoneTransform(HumanBodyBones.RightHand);
        m_leftFeet            = m_Anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        m_rightFeet           = m_Anim.GetBoneTransform(HumanBodyBones.RightHand);
        m_audioSource         = GetComponents<AudioSource>();
        m_sprintPressed       = false;
        m_isJogState          = true;
        groundCheckRayLength  = onGroundCheckLength;

        m_vertical            = 0;
        m_horizontal          = 0;        
        m_turn                = 0;
        m_speed               = 1;
        m_fullSpeed           = 3f;
        m_grounded = true;
        amParkouring = false;
        amSliding = false;
    }
	
	// Update is called once per frame
	void Update () {
        GetInputFromPlayer();
        AnimatorStateUpdate();
        FootStepsAudio();
        if (JumpDown())
            return;
        else if (Parkour())
            return;
        else
        {
            bool noFreeJumpCondition
                = amParkouring || !m_grounded || m_Combat.IsTargetting
                || m_RigBody.constraints != RigidbodyConstraints.FreezeRotation;
            FreeJump(noFreeJumpCondition);
        }
    }
    void FixedUpdate()
    {
        moveMent();
        //Help character to rotate
        ApplyExtraTurnRotation();

        SpeedCtrl();
        //To check which foot am using
        RightLeftFootCycle();
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
    }

    void GetInputFromPlayer()
    {        
        bool jogWalkSwitch = false;
        bool parkour       = false;
        bool climb         = false;
        if (enablePlayerCtrl)
        {
            m_vertical   = Input.GetAxis("Vertical");
            m_horizontal = Input.GetAxis("Horizontal");
            m_sprintPressed = Input.GetButton("Sprint");
            m_jumpClick     = Input.GetButtonDown("Jump");
            jogWalkSwitch   = Input.GetButtonDown("Jog");
            parkour         = Input.GetButton("Dodge");
            climb           = Input.GetButton("Jump");
        }
        else
        {
            m_vertical   = 0;
            m_horizontal = 0;  
            m_sprintPressed = false;
            m_jumpClick = false;
        }

        if (!m_isJogState && jogWalkSwitch)
        {
            m_isJogState = true;
        }
        else if (m_isJogState && jogWalkSwitch)
        {
            m_isJogState = false;
        }

        if (m_sprintPressed)
        {
            m_speed = m_fullSpeed;
            //m_Combat.IsTargetting = false;
            m_isJogState = true;
            m_Anim.SetBool("sprint", true);
        }
        else
        {
            m_speed = (m_isJogState) ? 2 : 1;
            m_Anim.SetBool("sprint", false);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            m_Anim.SetBool("move", true);
        else
            m_Anim.SetBool("move", false);


        bool parkourGeneralCondition = !amParkouring && !m_Combat.IsTargetting && m_Anim.GetFloat("speed") >= 1.1f;
        if (parkour && parkourGeneralCondition)
            goingToParkour = true;
        else goingToParkour = false;

        if (climb && parkourGeneralCondition)
            goingToJump = true;
        else goingToJump = false;

    }
    void moveMent()
    {
        // 1. Getting the moving direction from main camera
        m_moveDirWorld = Vector3.Scale((m_vertical * m_FreeCam.transform.forward
                + m_horizontal * m_FreeCam.transform.right), new Vector3(1, 0, 1));

        m_moveDirWorld = m_moveDirWorld.normalized;
        // 2. Adjust the movement direction for declination or inclination
        #region 2. Adjust the movement direction for declination or inclination

        Vector3 groundCheckRayOriginal = transform.position + new Vector3(0, 0.1f, 0);
        Vector3 normalVector = Vector3.up;
        LayerMask theMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");

        if (Physics.Raycast(groundCheckRayOriginal, -transform.up, out hitInfo_Ground, groundCheckRayLength, theMask))
        {
            normalVector = hitInfo_Ground.normal;
            m_grounded = true;
            groundCheckRayLength = onGroundCheckLength;
        }
        else
        {
            normalVector = Vector3.up;
            m_grounded = false;
            groundCheckRayLength = inAirGroundCheckLength;
        }

        //Debug.DrawRay(rayStartingPos, -transform.up * groundCheckRayLength);
        m_inclination = Vector3.Angle(Vector3.up, hitInfo_Ground.normal);
        FlatInclineDecline(m_inclination);

        m_moveDirBody = transform.InverseTransformDirection(m_moveDirWorld);

        m_moveDirWorld = Vector3.ProjectOnPlane(m_moveDirWorld, normalVector);
        m_moveDirWorld.y = 0;
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), 20 * m_moveDirWorld, Color.yellow);    // debug: Check Moving Direction in x,z axis, to see the y axis, commment out line 219, m_moveDir.y = 0;
        //Debug.DrawRay(transform.position, 20 * hitInfo_Ground.normal, Color.red);                   // debug: Check NORMAL VECTOR      
        //Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), 20 * move, Color.blue);         // debug: Check Moving Direction related to Charactor, (0,0,1) means charactor moving towards moving direction;

        #endregion
        // 3. determine the paramater for controlling the direction of movement 
        #region 3. Determine the paramater for controlling the direction of movement and update to animator
        m_turn = Mathf.Atan2(m_moveDirBody.x, m_moveDirBody.z);                                                                                             //tell charactor how much to turn in free run mode
        m_Anim.SetFloat("speed", m_speed * m_moveDirBody.z, 0.1f, Time.deltaTime);
        m_Anim.SetFloat("moveDirBody_turnAngle", (m_Anim.GetBool("move")) ? m_turn : 0, 0.1f, Time.deltaTime );
        m_Anim.SetFloat("moveDirBody_x_damper", m_moveDirBody.x, 0.3f, Time.deltaTime);
        m_Anim.SetFloat("moveDirBody_x", m_moveDirBody.x, 0.1f, Time.deltaTime);
        m_Anim.SetFloat("moveDirBody_z", m_moveDirBody.z, 0.1f, Time.deltaTime);
        m_Anim.SetFloat("moveDirBody_x_ctrl", m_moveDirBody.x);
        m_Anim.SetBool("grounded", m_grounded);
        #endregion
    }
    void ApplyExtraTurnRotation()
    {
        if (m_rM.State != RagdollManager.RagRollState.NotRagDoll || STANDUP || m_stickToCurrentRotation)
            return;
        if (amParkouring)
        {
            if (m_Anim.GetInteger("paoKuStyle") == 35)
            {
                Vector3 newDir = Vector3.RotateTowards(transform.forward, m_moveDirWorld, Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
            else
            {
                Vector3 newDir = Vector3.RotateTowards(transform.forward, parkourDir, 20 * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
        else if (m_Combat.IsDodging)
        {
            Vector3 newDir = Vector3.RotateTowards(transform.forward, m_Combat.DodgeDir, 20 * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
        else if (m_Combat.IsTargetting)
        {
            float step = m_Anim.GetFloat ("rotating spd") * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, m_combatSlot.TargetDir, step, 0);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
        else
        {
            if (NOROTATION)
                return;
            //help the character turn faster(this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_Anim.GetFloat("speed"));
            float turnRegulator = (m_Anim.GetBool("move")) ? 1 : 0; // && !standingIdle to avoid extra turn disturbence when charactor is standing still
            transform.Rotate(0, turnRegulator * m_turn * turnSpeed * Time.deltaTime, 0);
        }
    }
    void SpeedCtrl()
    {

        if (m_Combat.CombatMovement())
            return;

        Vector3 velocity;
        float speed = m_charactorSpeed * Time.fixedDeltaTime;


        if (FIGHTMOVEMENT)
        {
            velocity = m_moveDirWorld * combatStrffingSpd;
            velocity.y = m_RigBody.velocity.y;
            m_RigBody.velocity = velocity;
        }
        else if (GROUND)
        {
            velocity = m_moveDirWorld * m_Anim.GetFloat("speed") * speed;
            velocity.y = m_RigBody.velocity.y;
            m_RigBody.velocity = velocity;
        }




        //if (m_Anim.GetCurrentAnimatorStateInfo(0).IsName("FightMovement"))
        //    velocity = m_moveDirWorld * combatStrffingSpd;
        //else if (m_Anim.GetCurrentAnimatorStateInfo(0).IsName("FreeJump"))
        //    velocity = this.transform.forward * Mathf.Abs( m_Anim.GetFloat("speed") * 2);
        //else
        //    velocity = m_Anim.deltaPosition / Time.deltaTime;


        // for parkour
        ParkourPositionHandler();
    }

    #region Parkour Helper Func
    bool Parkour()
    {
        if (m_Combat.InCombatAction || (!goingToParkour && !goingToJump))
        {
            return false;
        }
        int paoKuStyle = -1;
        LayerMask layer = 1 << LayerMask.NameToLayer("parkourTrigger") | 1 << LayerMask.NameToLayer("parkour");
        // Shoot A ray to check if there is any parkour game object ahead
        Physics.Raycast(this.transform.position + 0.2f * transform.up, this.transform.forward, out hitInfo_pkrTrigger, 5f, layer);
        if (hitInfo_pkrTrigger.collider != null)
        {
            parkourDir = new Vector3(-hitInfo_pkrTrigger.normal.x, 0, -hitInfo_pkrTrigger.normal.z);
            float angle = Vector3.Angle(transform.forward, -hitInfo_pkrTrigger.normal);
            if (angle > parkourInAngle)
                return false;
            Physics.Raycast(this.transform.position + 0.1f * transform.up, parkourDir, out hitInfo_pkrDir, 5f, layer);
            // check the first ray shoot at the same object as the second ray
            if (hitInfo_pkrDir.collider != hitInfo_pkrTrigger.collider)
                return false;
            // Get the actual(perpendicular) distance between me and parkour object
            distanceToParkourObject = hitInfo_pkrDir.distance;

            //Debug.Log(Mathf.Cos(angle * Mathf.Deg2Rad) + " " + hitInfo_parkour.distance + " " + distanceToParkourObject);
            if (goingToParkour)
            {
                if (hitInfo_pkrTrigger.collider.tag == "obstacle")
                {
                    #region 1. If this is a obstacle
                    #region 1.1 Get Obstacle Info, and obstacle type we are dealing with
                    //get the obstacle size
                    float obstacle_width = 0;
                    GetObstacleFullSizeInfo(ref obstacle_width, ref obstacle_top, ref obstacle_length);
                    // get the obstacle type
                    ObstacleType obType;
                    if (obstacle_length > 3f)
                        return false;
                    if (obstacle_length >= 1.7f && obstacle_length <= 3f)
                        obType = ObstacleType.longbox;
                    else if (obstacle_length >= .25f && obstacle_length <= .5f)
                        obType = ObstacleType.fence;
                    else if (obstacle_length >= .5f && obstacle_length <= 1.7f)
                        obType = ObstacleType.box;
                    else obType = ObstacleType.slidingPlatform;
                    #endregion

                    #region 1.2 pick the proper jump by obstacle type
                    switch (obType)
                    {
                        case ObstacleType.fence:
                            #region 1.2.1 pick a jump for fence by height 0 ~ 2
                            if (distanceToParkourObject < 1.5f)
                                // RightLegRise_Jog  jumpOverMaxBoosterSpd_vert = 8f
                                paoKuStyle = JumpOver(Random.Range(0, 3), 8);
                            else return false;

                            #endregion
                            break;
                        case ObstacleType.box:
                            #region 1.2.2 pick a jump for box 5 ~ 10
                            if (obstacle_top - transform.position.y <= 1.8f)
                            {
                                if (distanceToParkourObject < 0.6f) ////// Sliding Over
                                    paoKuStyle = JumpOverSliding();
                                else if (distanceToParkourObject > 1f && distanceToParkourObject < 2f)
                                    paoKuStyle = JumpOver(5, 8);
                                else if (distanceToParkourObject > 2.3f && distanceToParkourObject < 2.5f)
                                    // MidAirFlip360 
                                    paoKuStyle = JumpOver(Random.Range(6, 8), 8);
                                else return false;
                            }
                            else
                            {
                                // Jump Over Wall > . <
                                if (distanceToParkourObject > 2.0f && distanceToParkourObject < 2.8f)
                                {
                                    int rand = Random.Range(0, 9);
                                    if (rand > 5)
                                        paoKuStyle = JumpOver(10, 15);
                                    else
                                        paoKuStyle = JumpOver(9, 30);
                                }
                                else return false;
                            }
                            #endregion
                            break;
                        case ObstacleType.longbox:
                            #region 1.2.3 pick a jump for long box by distance 11 ~ 15

                            if (distanceToParkourObject <= .6f)
                                paoKuStyle = JumpOverSliding();
                            else if (distanceToParkourObject >= 2f && distanceToParkourObject < 2.2f)
                                //else if (distanceToParkourObject >= 2.3f && distanceToParkourObject < 2.5f)
                                paoKuStyle = JumpOver(Random.Range(11, 13), 10);
                            else if (distanceToParkourObject >= 3.8f && distanceToParkourObject < 4f)
                                paoKuStyle = JumpOver(13, 10);
                            else return false;

                            #endregion
                            break;
                        case ObstacleType.slidingPlatform:

                            if (distanceToParkourObject <= .5f)
                                //// Sliding Over
                                paoKuStyle = JumpOverSliding();
                            else return false;
                            break;
                    }
                    #endregion
                    #endregion
                }
                else if (hitInfo_pkrTrigger.collider.tag == "slider")
                {
                    #region 2. If this is slider
                    #region 2.1 Get the slider size info, and type give by the size info

                    // Get the size info
                    float obstacle_width = 0;
                    obstacle_top = hitInfo_pkrTrigger.collider.transform.localScale.y + hitInfo_pkrTrigger.collider.transform.position.y
                        - hitInfo_pkrTrigger.point.y;
                    if (Mathf.Abs(Vector3.Dot(hitInfo_pkrTrigger.collider.transform.forward, parkourDir)) >= 0.9f)
                    {
                        // only side in blue axis
                        obstacle_length = hitInfo_pkrTrigger.collider.transform.localScale.x;
                        obstacle_width = hitInfo_pkrTrigger.collider.transform.localScale.z;
                    }
                    else
                        return false;
                    // Get the type
                    SliderTppe slidertype = SliderTppe.crouching;
                    if (obstacle_top <= 1.2f)
                        slidertype = SliderTppe.laying;


                    #endregion

                    #region 2.2 Pick proper style given by the sliding type and distance
                    switch (slidertype)
                    {
                        case SliderTppe.laying:
                            #region 2.2.1 when Laying 20 ~ 21
                            if (distanceToParkourObject < 0.8f)
                            {
                                paoKuStyle = -2;
                                ParkourSliding();
                            }
                            else if (distanceToParkourObject < 2.2f && distanceToParkourObject > 2f)
                            {
                                paoKuStyle = Random.Range(20, 22);
                                ParkourSliding();
                            }
                            else return false;
                            #endregion
                            break;
                        case SliderTppe.crouching:
                            #region 2.2.2 when Laying 25 ~ 26

                            if (distanceToParkourObject < 0.8f)
                            {
                                paoKuStyle = -2;
                                ParkourSliding();
                            }
                            else if (distanceToParkourObject < 2.2f && distanceToParkourObject > 2f)
                            {
                                paoKuStyle = Random.Range(25, 27);
                                ParkourSliding();
                            }
                            else return false;

                            #endregion
                            break;
                    }
                    #endregion
                    #endregion
                }
                else return false;
            }
            else if (goingToJump)
            {
                //angle > 45 ||
                if (distanceToParkourObject < 0.3f)
                    return false;
                #region 3. If this is something I can Climb Up
                #region 3.1 Get climb up Object info
                if (hitInfo_pkrTrigger.collider.tag == "slider")
                    return false;
                float obstacle_width = 0;
                float obstacle_height = 0;
                GetObstacleFullSizeInfo(ref obstacle_width, ref obstacle_top, ref obstacle_length);
                obstacle_height = obstacle_top - transform.position.y;
                if (obstacle_height <= 0.3f || obstacle_height > 4.1f)
                    return false;
                ClimbUpType climbUpType;
                if (obstacle_height <= 1.6f)
                    climbUpType = ClimbUpType.step;
                else if (obstacle_height <= 2.6f)
                    climbUpType = ClimbUpType.stage;
                else
                    climbUpType = ClimbUpType.wall;
                #endregion
                //Debug.Log(climbUpType + " " + distanceToParkourObject );
                #region 3.2 Pick Up proper vault type
                switch (climbUpType)
                {
                    case ClimbUpType.step:
                        if (distanceToParkourObject < jumpUpStepDis)
                            paoKuStyle = ClimbUp(30, 10);
                        else return false;
                        break;
                    case ClimbUpType.stage:
                        if (distanceToParkourObject < ClimbUpStageDis)
                            paoKuStyle = ClimbUp(31, 15);
                        else return false;
                        break;
                    case ClimbUpType.wall:
                        if (distanceToParkourObject <= .5f)
                            paoKuStyle = ClimbUp(33, 8);
                        else if (distanceToParkourObject < ClimbUpLevelDis)
                            paoKuStyle = ClimbUp(34, 8);
                        else return false;
                        break;
                }
                #endregion
                #endregion
            }
            else return false;
            m_Anim.SetInteger("paoKuStyle", paoKuStyle);
            return amParkouring;
        }
        else return false;
    }
    bool JumpDown()
    {
        if (amParkouring)
            return false;
        if (!m_grounded)
        {
            if (m_Combat.InCombatAction)
            {
                m_Combat.ResetOnQuitCombat(true);
            }
            LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");
            Physics.Raycast(this.transform.position, -transform.up, out hitInfo_Ground, Mathf.Infinity, mask);
            if (hitInfo_Ground.distance < .5f)
                return false;
            // Set the jumpDown Direction
            parkourDir = m_moveDirWorld;
            #region Get the Action Style based on given height
            int paoKuStyle = -1;

            // walking or jogging
            if (m_Anim.GetFloat("speed") <= 2.1f)
            {
                if (hitInfo_Ground.distance <= 2.1f)
                    paoKuStyle = 40;
                else paoKuStyle = 45;
            }
            else
            {
                if (hitInfo_Ground.distance <= 2.5f)
                    paoKuStyle = 43;
                else paoKuStyle = 44;
            }
            #endregion
            amParkouring = true;
            m_Anim.applyRootMotion = false;
            m_Anim.SetTrigger("jump down");
            m_Anim.SetInteger("paoKuStyle", paoKuStyle);
            return true;
        }
        else return false;
    }
    void ParkourPositionHandler()
    {
        if (HeightSmoothDamper || parkourHorizonLerp || lerpFromHandToFeet || jumpDownLerp)
        {
            #region 1. Stop the transition if the target is reached

            float speed = 0;
            Vector3 newRootPosition = this.transform.position;

            if (Mathf.Abs(transform.position.y - parkourHeightAdjustTarget) < 0.01f)
                HeightSmoothDamper = false;

            if (Mathf.Abs(newRootPosition.x - parkourHorizontalLandingPos.x) < 0.01f && Mathf.Abs(newRootPosition.z - parkourHorizontalLandingPos.z) < 0.01f)
                parkourHorizonLerp = false;
            #endregion

            #region 2. Transform the position
            #region 2.1 Used for Jump Down
            if (jumpDownLerp)
            {
                RaycastHit groundHit;
                LayerMask theMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");
                if (Physics.Raycast(this.transform.position + m_collider.height * Vector3.up, -transform.up, out groundHit, Mathf.Infinity, theMask))
                {
                    feetPosition = this.transform.position.y;
                    parkourHeightAdjustTarget = groundHit.point.y;
                }

                float period = lerpEndTime - lerpStartTime;// - 0.02f;
                float friction = (m_animNormalizedTime - lerpStartTime) / period;
                newRootPosition.y = Mathf.Lerp(feetPosition, parkourHeightAdjustTarget, friction);
                transform.position = newRootPosition;
                if (friction > 1f)
                    jumpDownLerp = false;
                //Debug.Log(friction);
                return;
            }
            #endregion

            #region 2.2 Only for Climb upon wall edge
            if (lerpFromHandToFeet)
            {
                float friction = 1 - (0.5f * (m_leftHand.position.y + m_rightHand.position.y) - this.transform.position.y)
                    / (handPosition_climbUp - feetPosition);
                newRootPosition.y = Mathf.Lerp(feetPosition, handPosition_climbUp, friction);
                if (climbUpGroundCheck)
                {
                    climbUpGroundCheck = false;
                    lerpFromHandToFeet = false;
                    newRootPosition.y = obstacle_top + 0.05f;
                }
                transform.position = newRootPosition;
            }
            #endregion

            #region 2.3 Used for jump up
            if (HeightSmoothDamper)
                newRootPosition.y = Mathf.SmoothDamp(newRootPosition.y, parkourHeightAdjustTarget, ref speed, Time.deltaTime, jumpOverMaxBoosterSpd_vert);

            #endregion

            #region 2.4 Used for horizontal lerp
            if (parkourHorizonLerp)
            {
                float period = lerpEndTime - lerpStartTime - 0.02f;
                float friction = (m_animNormalizedTime - lerpStartTime) / period;
                newRootPosition.x = Mathf.Lerp(parkourHorizontalStartingPos.x, parkourHorizontalLandingPos.x, friction);
                newRootPosition.z = Mathf.Lerp(parkourHorizontalStartingPos.z, parkourHorizontalLandingPos.z, friction);
                if (friction > 1f)
                    parkourHorizonLerp = false;
            }
            #endregion

            transform.position = newRootPosition;
            #endregion
        }
    }
    int JumpOverSliding()
    {
        //// Sliding Over
        parkourHeightAdjustTarget = obstacle_top - 1.2f;
        jumpOverMaxBoosterSpd_vert = 5;
        HeightSmoothDamper = true;
        m_RigBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        m_Anim.SetTrigger("paoKu");
        amParkouring = true;  // to prevent animation call continuously
        return -1;
    }
    int JumpOver(int _style, int _increment)
    {      
        m_Anim.SetTrigger("paoKu");
        jumpOverMaxBoosterSpd_vert = obstacle_top * 10;
        amParkouring = true; // to prevent animation call continuously
        lerpEndTime = GetHorizontalLerpEndTime(_style);
        return _style;
    }
    int ClimbUp(int _style, int _increment)
    {
        float offset = 0;
        switch (_style)
        {
            case 31:
                offset = 1.1f;
                break;
            case 32:
                offset = 2.2f;
                break;
            case 33:
                offset = 1.9f;
                break;
            case 34:
                offset = 1.75f;
                break;
            default: // step , 30
                break;
        }
        parkourHeightAdjustTarget = obstacle_top - offset;
        jumpOverMaxBoosterSpd_vert = _increment * (parkourHeightAdjustTarget - transform.position.y);
        //For Horizontal Lerp
        lerpEndTime = GetHorizontalLerpEndTime(_style);
        m_Anim.SetTrigger("paoKu");
        amParkouring = true;  // to prevent animation call continuously
        return _style;
    }
    void ParkourSliding()
    {
        m_Anim.SetTrigger("paoKu");
        amParkouring = true;
        amSliding = true;
    }
    void GetObstacleFullSizeInfo(ref float _width, ref float _height, ref float _length)
    {
        _height = hitInfo_pkrTrigger.collider.transform.localScale.y + hitInfo_pkrTrigger.collider.transform.position.y;
        float dotPro_z = Mathf.Abs(Vector3.Dot(hitInfo_pkrTrigger.collider.transform.forward, parkourDir));
        //float dotPro_x = Mathf.Abs(Vector3.Dot(hitInfo_parkour.collider.transform.right, parkourDir));
        if (dotPro_z <= 0.01f)
        {
            // From X axis
            _length = hitInfo_pkrTrigger.collider.transform.localScale.x;
            _width = hitInfo_pkrTrigger.collider.transform.localScale.z;

        }
        else
        {
            // From Z axis

            _length = hitInfo_pkrTrigger.collider.transform.localScale.z;
            _width = hitInfo_pkrTrigger.collider.transform.localScale.x;
        }
    }
    float GetHorizontalLerpEndTime(int _paoKuStyle)
    {
        switch (_paoKuStyle)
        {
            case 0:
            case 1:
                return 0.31f;
            case 31:
            case 2:
                return 0.26f;
            case 5:
            case 33:
                return 0.33f;
            case 6:
                return 0.37f;
            case 7:
                return 0.417f;
            case 9:
            case 30:
                return 0.463f;
            case 10:
                return 0.52f;
            case 11:
                return 0.5f;                
            case 12:
                return 0.486f;
            case 13:
                return 0.568f;
            case 34:
                return 0.206f;
            default:
                return 0.5f;
        }
    }
    #endregion

    void FreeJump(bool _disable)
    {
        m_Anim.SetFloat("vertSpd", m_RigBody.velocity.y);
        if (amParkouring && m_RigBody.velocity.y < -10)
            m_Anim.SetTrigger ("RollLanding");
        else if (!amParkouring)
            m_Anim.ResetTrigger("RollLanding");

        if (_disable)
            return;
        if (m_jumpClick)
        {
            amParkouring = true;
            m_Anim.applyRootMotion = false;
            m_Anim.SetTrigger("paoKu");
            m_Anim.SetInteger("paoKuStyle", 35);
            //AudioLibrary.PlayHumanLanding(m_audioSource[0], m_audioSource[1], false);

            AudioLibrary.PlayHumanJump(m_audioSource);
        }
    }
    void AnimatorStateUpdate()
    {
        AnimatorStateInfo animatorState_0 = m_Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo animatorTran_0 = m_Anim.GetAnimatorTransitionInfo(0);
        GROUND = animatorState_0.IsName("Ground");
        
        FIGHTMOVEMENT =  animatorState_0.IsName("FightMovement");
        PARKOUR = animatorState_0.IsTag("Parkour");
        NO_TRANSISTION = !m_Anim.IsInTransition(0);
        NOROTATION = animatorState_0.IsTag("NoRotation");
        STANDUP = animatorState_0.IsTag("standup") && m_animNormalizedTime < 0.6f;
        GETHIT = animatorState_0.IsTag("GetHit");
        m_animNormalizedTime = animatorState_0.normalizedTime;


        m_Anim.SetFloat("normalized time", m_animNormalizedTime);
    }
    void RightLeftFootCycle()
    {
        float footCycle = Mathf.Repeat(m_animNormalizedTime, 1);


        footCycle = (footCycle > 0.5f ? 1 : -1) * MoveDirWorld.magnitude;

        if (m_grounded & !amParkouring)
        {
            m_Anim.SetFloat("LeftOrRightFoot", footCycle);
        }


    }
    void FootStepsAudio()
    {

        if (!NO_TRANSISTION)
        {
            if (m_animNormalizedTime > 0.4f)
                m_footTouchGround = .9f;
            else
                m_footTouchGround = 0.4f;
        }
        else if (GROUND)
        {
            if (m_animNormalizedTime > m_footTouchGround)
            {
                if (m_Anim.GetCurrentAnimatorStateInfo(0).IsName("FightMovement"))
                    AudioLibrary.PlayHumanFootStep(m_audioSource[0], 1);
                else
                    AudioLibrary.PlayHumanFootStep(m_audioSource[0], m_Anim.GetFloat("speed"));

                m_footTouchGround += 0.5f;
            }
        }
    }
    void FlatInclineDecline(float angle)
    {
        if (angle > 12)
        {
            if (m_RigBody.velocity.y > 0)
            {
                inIncline = true;
                inFlat = false;
                inDecline = false;
            }
            else
            {
                inIncline = false;
                inFlat = false;
                inDecline = true;
            }
        }
        else
        {
            inIncline = false;
            inFlat = true;
            inDecline = false;
        }
    }
    void parkourHorizontalLerpLandingSetUp(Vector3 _landingPos)
    {
        lerpStartTime = m_animNormalizedTime;
        parkourHorizonLerp = true;
        parkourHorizontalStartingPos = this.transform.position;
        parkourHorizontalLandingPos = _landingPos;
    }

    public static AudioSource[] Audio
    {
        get { return m_audioSource; }
    }
    /////////////////////////////////////////////// Class Properties ////////////////////////////////////////// 
    #region Class Property
    public bool isFlat
    {
        get { return inFlat; }
    }
    public float GetInclination
    {
        get { return m_inclination; }
    }
    public Vector3 MoveDirWorld
    {
        get { return m_moveDirWorld; }
    }
    public Vector3 MoveDirBody
    {
        get { return m_moveDirBody; }
    }
    public bool IsSprinting
    {
        get { return m_sprintPressed; }
    }
    public bool IsStandingUp
    {
        get { return STANDUP; }
    }
    public bool IsGettingHit
    {
        get { return GETHIT; }
    }
    public bool AmParkouring
    {
        get { return amParkouring; }
    }
    public bool AmSliding
    {
        get { return amSliding; }
    }
    public bool StickToCurrentRotation
    {
        get { return m_stickToCurrentRotation; }
        set { m_stickToCurrentRotation = value; }
    }
    #endregion

    #region Animation Event
    // Resets
    void FinishSliding()
    {
        amParkouring = false;
        amSliding = false;
        CollisionRecover();

    }
    void FinishJumpOver()
    {
        amParkouring = false;
        FinishLerping();
        CollisionRecover();
    }
    void FinishClimbUp()
    {
        CollisionRecover();
        climbUpGroundCheck = false;
        parkourHorizonLerp = false;
        amParkouring = false;
        lerpFromHandToFeet = false;
        m_RigBody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    void FinishJumpUp()
    {
        // only for style 30;
        HeightSmoothDamper = false;
        m_RigBody.constraints = RigidbodyConstraints.FreezeRotation;
        this.transform.position 
            = new Vector3 (this.transform.position.x, obstacle_top + 0.05f, this.transform.position.z);
        parkourHorizonLerp = false;
        amParkouring = false;
    }
    public void FinishJumpDown()
    {
        amParkouring = false;
        m_Anim.applyRootMotion = true;
        CollisionRecover();
    }
    public void UseAnimationMotion()
    {
        Vector3 velocity = m_Anim.deltaPosition / Time.deltaTime;
        velocity.y = m_RigBody.velocity.y;
        m_RigBody.velocity = velocity;

    }
    void FinishLerping()
    {
        jumpDownLerp = false;
    }

    // Ground Check
    void SlidingOverGroundCheck()
    {
        RaycastHit groundHit;
        LayerMask theMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");
        Debug.DrawRay(this.transform.position + m_collider.height * Vector3.up, -transform.up * 2 * transform.position.y, Color.cyan);
        if (Physics.Raycast(this.transform.position + m_collider.height * Vector3.up, -transform.up, out groundHit, 2 * (transform.position.y + 2), theMask))
        {
            if (groundHit.collider != hitInfo_pkrTrigger.collider)
            {
                m_Anim.SetTrigger("paoKu Landing");
            }
        }
    }
    void SlidingunderlyingGroundCheck()
    {
        LayerMask theMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");
        if (!Physics.Raycast(m_Anim.GetBoneTransform(HumanBodyBones.Hips).position, transform.up, out hitInfo_Ground, 3, theMask))
        {
            m_Anim.SetTrigger("paoKu Landing");
        }
    }
    void JumpDownGroundCheck(float _lerpEndTime)
    {
        // only used for jump over style
        HeightSmoothDamper = false;
        m_RigBody.constraints = RigidbodyConstraints.FreezeRotation;
        parkourHorizonLerp = false;
        lerpStartTime = m_animNormalizedTime;
        lerpEndTime = _lerpEndTime;
        jumpDownLerp = true;

        //RaycastHit groundHit;
        //LayerMask theMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("parkour");
        //if (Physics.Raycast(this.transform.position + m_collider.height * Vector3.up, -transform.up, out groundHit, Mathf.Infinity, theMask))
        //{
        //    parkourHorizonLerp = false;
        //    lerpStartTime = m_animNormalizedTime;
        //    lerpEndTime = _lerpEndTime;
        //    feetPosition = this.transform.position.y;
        //    jumpDownLerp = true;
        //    parkourHeightAdjustTarget = groundHit.point.y;
        //}
        }
    void ClimbUpGroundCheck()
    {
        climbUpGroundCheck = true;
    }

    // parkour start ups
    void ClimbUpStartUp()
    {
        // only for style 33
        HeightSmoothDamper = true;
        m_RigBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }
    void JumpUpStartUp()
    {
        if (!HeightSmoothDamper)
        {
            HeightSmoothDamper = true;
            m_RigBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
        //CollisionCancel();
        // horizontal
        switch (m_Anim.GetInteger("paoKuStyle"))
        {
            case 31:
            case 34:
                parkourHorizontalLerpLandingSetUp(hitInfo_pkrDir.point - 0.2f * transform.forward);
                break;
            case 33:
                parkourHorizontalLerpLandingSetUp(hitInfo_pkrDir.point - 0.3f * transform.forward);
                break;
            default:
                parkourHorizontalLerpLandingSetUp(hitInfo_pkrDir.point);
                break;
        }
    }
    void JumpDownStartUp(float _animheight)
    {
        parkourHeightAdjustTarget = this.transform.position.y + _animheight;
        HeightSmoothDamper = true;
        jumpOverMaxBoosterSpd_vert = 10;
    }
    void JumpDownAirBrone()
    {
        HeightSmoothDamper = false;
    }
    public void FreeJumpStartUp()
    {
        m_RigBody.velocity = new Vector3(m_RigBody.velocity.x, jumpPower, m_RigBody.velocity.z);
    }
    void JumpOverJumpUp(float _animOffset)
    {
        CollisionCancel();
        parkourHeightAdjustTarget = obstacle_top - _animOffset;
        HeightSmoothDamper = true;
        m_RigBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }
    void JumpOverJumpAhead()
    {
        parkourHorizontalLerpLandingSetUp(transform.forward * (obstacle_length + 0.5f) + hitInfo_pkrDir.point);
    }
    void JumpOverStartup(float _animOffset)
    {
        Vector3 targetLandingPosition = transform.forward * (obstacle_length + 0.5f) + hitInfo_pkrDir.point;
        JumpOverJumpUp(_animOffset);
        parkourHorizontalLerpLandingSetUp(targetLandingPosition);
    }
    void JumpOverFenceStartUp(float _animOffset)
    {
        Vector3 targetLandingPosition = transform.forward * obstacle_length * 0.5f + hitInfo_pkrDir.point;
        JumpOverJumpUp(_animOffset);
        parkourHorizontalLerpLandingSetUp(targetLandingPosition);
    }

    // Helpers
    void CollisionRecover()
    {
        m_collider.isTrigger = false;
        m_RigBody.useGravity = true;
    }
    void HookUp()
    {
        CollisionCancel();
        feetPosition = this.transform.position.y;
        HeightSmoothDamper = false;
        lerpFromHandToFeet = true;
        handPosition_climbUp = 0.5f * (m_rightHand.position.y + m_leftHand.position.y);//obstacle_top;//
    }
    void CollisionCancel()
    {
        m_RigBody.useGravity = false;
        m_collider.isTrigger = true;

    }


    // Sound


    #endregion








    enum ObstacleType
    {
        fence, 
        box,
        longbox,
        slidingPlatform,
    }
    enum SliderTppe
    {
        laying,
        crouching
    }
    enum ClimbUpType
    {
        step, 
        stage, 
        wall
    }
}
