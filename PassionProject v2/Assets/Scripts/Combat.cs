using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Combat : MonoBehaviour{

    // Use this for initialization

    [SerializeField, ReadOnly]
    bool m_punch,
                     m_attacking,
                     m_heavy,
                     m_dodge,
                     m_amHavingAOpenFlank,
                     m_amTargetting,
                     m_amGoingToHit,
                     m_amDamageImmune,
                     m_amDodging,
                     m_detourLeft, m_detourRight,
                     m_amSpecialAttack;

    [SerializeField, ReadOnly] float m_impact, m_distanceToTarget,damageImpact;
    [SerializeField] SphereCollider m_rightFist, m_leftFist, m_rightKick, m_leftKick;

    Vector3          m_DodgeDir;
    Animator         m_anim;
    Hitman_BasicMove m_basicMove;
    CombatSlots      m_combatSlot;
    CapsuleCollider  m_bodyCollider;
    Rigidbody        m_body;
    RagdollManager   m_RM;
    HeroStatus       m_stats;
    Enemy            theOneWhoHitMe;
    GameManager      m_GM;


    // Use this for initialization
    void Start()
    {
        m_RM           = GetComponentInChildren<RagdollManager>();
        m_combatSlot   = GetComponentInChildren<CombatSlots>();
        m_bodyCollider = GetComponent<CapsuleCollider>();
        m_body         = GetComponent<Rigidbody>();
        m_stats        = GetComponent<HeroStatus>();
        m_GM           = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        m_anim = GetComponent<Animator>();
        m_basicMove    = GetComponent<Hitman_BasicMove>();

        m_punch     = false;
        m_attacking      = false;
        m_heavy     = false;
        m_amDamageImmune = false;
        m_amHavingAOpenFlank = false;

        m_amTargetting    = false;
        m_amGoingToHit    = false;

        m_distanceToTarget = 0;
        // use trigger for damage register
        //m_leftKick.isTrigger = true;
        //m_rightKick.isTrigger = true;
        //m_leftFist.isTrigger = true;
        //m_rightFist.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckHeroIsDead())
            return;

        //Debug.Log(m_leftFist.tag + m_leftKick.tag + m_rightFist.tag + m_rightKick.tag);
        // check combat mode condition
        if (m_combatSlot.Count > 0 && !m_basicMove.IsSprinting && !m_basicMove.AmParkouring)
        {
            m_amTargetting = true;
        }
        else if (!m_anim.GetBool("Hit Response"))
        {
            if (m_amTargetting)
            {
                ResetOnQuitCombat();
            }
        }
        // if I am being hit, reset the combat flags
        if (m_anim.GetBool("Hit Response"))
        {
            if (m_attacking)
                ResetOnBeingHit();
            if (m_amDodging)
            {
                ResetDodging();
            }
        }
        GetUserInput();
    }
    void FixedUpdate()
    {

        //if (m_amGoingToHit)
        //{
        //    if (m_distanceToTarget > 1.3f)// && m_combatSlot.TheTarget != null)
        //        transform.position += transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd");
        //}
        //else if (m_amDodging || m_anim.GetBool("Hit Response"))
        //{
        //    transform.position -= transform.forward * m_anim.GetFloat("sliding spd") * Time.deltaTime;
        //}
        //else if (m_amSpecialAttack)
        //{
        //    transform.position += transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd");
        //}
    }

    #region Combat Helper Func

    void GetUserInput()
    {
        // Check target distance     

        Vector3 targetDir_hor = (m_combatSlot.TheNextTarget) ? 
            m_combatSlot.TheNextTarget.transform.position - this.transform.position: transform.forward;
        targetDir_hor.y = 0;
        m_distanceToTarget = targetDir_hor.magnitude;


        Vector3 enemyDirInHeroBodySpace = this.transform.InverseTransformDirection(targetDir_hor).normalized;

        #region 1. Button Check


        if (Hitman_BasicMove.enablePlayerCtrl)
        {
            bool ctrlDisable = m_basicMove.AmParkouring || m_amDodging || m_basicMove.IsStandingUp || m_anim.GetBool("Hit Response") || m_amSpecialAttack;
            m_punch = (Input.GetButton("Fire1") && !ctrlDisable) ? true : false;
            m_heavy = (Input.GetButton("Fire2") && !ctrlDisable && m_stats.MyStamina >= 0.4f) ? true : false;
            m_dodge = (Input.GetButtonDown("Dodge") && !ctrlDisable && m_amTargetting) ? true : false;
        }
        else
        {
            m_punch = false;
            m_heavy = false;
            m_dodge = false;
        }

        #endregion

        #region 2. Attack Genre Check

        bool m_backAttack = false,
             m_flyKick = false,
             m_slidingKick = false,
             m_normalAttack = false;
    
        // check whether this is a special attack, otherwise it is a normal attack
        if (m_basicMove.IsSprinting && m_anim.GetBool("move") && m_anim.GetFloat("speed") >= 2f)
        {
            // Fly Kick
            if (m_stats.MyStamina >= 0.6f)
            {
                m_slidingKick = true;
                m_flyKick = true;
            }
            else if (m_stats.MyStamina >= 0.35f)
                m_slidingKick = true;

        }
        else if (m_combatSlot.TheNextTarget && m_combatSlot.TheNextTarget.MyClass == Enemy.EnemyClass.Douglas)
        {
            
            if (m_distanceToTarget <= 4 && m_combatSlot.TheNextTarget.MYSTATE != Enemy.BehavivorState.KnockDown 
                && enemyDirInHeroBodySpace.z > .7f
                && Vector3.Dot(m_combatSlot.TheNextTarget.transform.forward, transform.forward) > 0.7f)
                // Back Attack
                m_backAttack = true;
            else
                m_normalAttack = true;
        }
        else m_normalAttack = true;

        #endregion

        #region 3. detour type && Normal Attack direction check

        bool attackahead = false,
             attackleft = false,
             attackright = false;
             m_detourLeft = false;
             m_detourRight = false;

        // detour enable and direction check
        if (m_dodge && m_distanceToTarget <= 3f && m_basicMove.MoveDirBody.z > 0f)
        {
            m_dodge = false;
            Vector3 detourDirVector;
            detourDirVector = transform.InverseTransformDirection(targetDir_hor);
            if (detourDirVector.x > 0)
                m_detourLeft = true;
            else m_detourRight = true;
        }

        if (m_normalAttack)
        {
            //if (m_combatSlot.Count <= 1)
            if (m_combatSlot.LockOn || m_combatSlot.TheNextTarget == null)            
            {
                // When lock on a single enemy, we choose our attack direction based on input
                if (m_basicMove.MoveDirBody.magnitude < 0.1f)
                {
                    attackahead = true;
                }
                else
                {
                    if (m_basicMove.MoveDirBody.z > 0.7f)
                    {
                        attackahead = true;
                    }
                    else if (m_basicMove.MoveDirBody.x >= 0)
                        attackright = true;
                    else attackleft = true;
                }
            }
            else
            {
                // when on lock down, we choose our attack direction based on enemy position
                if (enemyDirInHeroBodySpace.z > 0.7f)
                    attackahead = true;
                else if (enemyDirInHeroBodySpace.x >= 0)
                    attackright = true;
                else attackleft = true;
            }
        }
        #endregion

        #region 4. Animation Calls
        m_anim.SetBool("enemy orbit", m_amTargetting);
        m_anim.SetBool("flykick", m_flyKick);
        m_anim.SetBool("slidingKick", m_slidingKick);

        m_anim.SetBool("back Attack", m_backAttack);
        m_anim.SetBool("normal attack", m_normalAttack);
        m_anim.SetBool("punch", m_punch);
        m_anim.SetBool("heavy", m_heavy);
        m_anim.SetBool("dodge", m_dodge);
        m_anim.SetBool("detour left", m_detourLeft);
        m_anim.SetBool("detour right", m_detourRight);
        m_anim.SetBool("attacking", m_attacking);
        m_anim.SetBool("attack ahead", attackahead);
        m_anim.SetBool("attack left", attackleft);
        m_anim.SetBool("attack right", attackright);
        m_anim.SetFloat("distanceToTarget", m_distanceToTarget);

        #endregion
    }
    void ResetOnBeingHit()
    {
        // Reset every thing What the player is doing due to being hit by enemy        
        //m_amGoingToHit = false;
        CanNotDamage();
        m_attacking = false;
        m_amSpecialAttack = false;
        m_amHavingAOpenFlank = false;
        m_basicMove.StickToCurrentRotation = false;

    }
    bool CheckHeroIsDead()
    {
        bool gameShouldShutDown = m_stats.CurrentHp <= 0;
        if (gameShouldShutDown)
        {
            if (m_RM.State == RagdollManager.RagRollState.NotRagDoll)
            {                
                m_RM.BecomeRagDoll();
            }
            if (m_RM.enabled)
            {
                ResetOnQuitCombat(true);
                m_combatSlot.ClearSlot();
                StartCoroutine(m_GM.TeleportPlayerTo(GameManager.GameLocation.energyYard, true));
            }
        }
        return gameShouldShutDown;
    }
    #endregion

    #region Animation Event

    void Avoid(Vector3 _avoidDir)
    {
        // reset current normal attack
        CanNotDamage();
        m_attacking = false;
        // set up dodging property
        if (_avoidDir.sqrMagnitude < 0.1f)
            m_DodgeDir = -this.transform.forward;
        else
            m_DodgeDir = _avoidDir;
        m_amDamageImmune = true;
        m_amDodging = true;
        //m_bodyCollider.isTrigger = true;

    }
    //  Dodging
    void Dodging()
    {
        //Avoid(-m_basicMove.MoveDirWorld);
        Avoid(m_basicMove.MoveDirWorld);

    }
    void ResetDodging()
    {
        m_amDamageImmune = false;
        m_amDodging = false;
        //m_bodyCollider.isTrigger = false;
    }
    // Detour
    void Detour()
    {
        if (m_basicMove.AmParkouring)
            return;
        Vector3 detourDir;
        detourDir = m_combatSlot.TheNextTarget.transform.position + m_combatSlot.TheNextTarget.Width * ((m_detourLeft) ? -1 : 1) * transform.right - this.transform.position;

        //if (m_combatSlot.TheNextTarget.Width > 0.3f)
        //    detourDir = m_combatSlot.TheNextTarget.transform.position + m_combatSlot.TheNextTarget.Width * ((m_detourLeft) ? -1 : 1) * transform.right - this.transform.position;
        //else
        //    detourDir = m_combatSlot.TheNextTarget.transform.position - this.transform.position;
        Avoid(detourDir);
    }

    // Flying Kick
    void FlyingKick(float _impact)
    {
        if (m_basicMove.AmParkouring)
            return;
        m_amSpecialAttack = true;
        m_impact = _impact;
        m_basicMove.StickToCurrentRotation = true;
        m_amHavingAOpenFlank = true;
        m_leftKick.radius = 0.25f;
        m_stats.StaminaChangedBy(-0.6f);
    }
    void ResetFlyKick()
    {
        m_basicMove.StickToCurrentRotation = false;
        m_amHavingAOpenFlank = false;
        m_amSpecialAttack = false;
        m_leftKick.radius = 0.11f;
    }

    // Slide Kick
    void SlidingKick(float _impact)
    {
        if (m_basicMove.AmParkouring)
            return;

        m_impact = _impact;
        m_amSpecialAttack = true;
        m_basicMove.StickToCurrentRotation = true;
        m_leftKick.radius = 0.35f;
        m_amDamageImmune = true;
        m_stats.StaminaChangedBy(-0.35f);
    }
    void ResetSlidingKick()
    {
        m_basicMove.StickToCurrentRotation = false;
        m_amDamageImmune = false;
        m_amSpecialAttack = false;
        m_leftKick.radius = 0.11f;
    }

    // Normal Attack
    void Sliding(float _impact)
    {

        if (m_basicMove.AmParkouring)
            return;

        m_impact = _impact;
        m_amGoingToHit = true;
        m_attacking = true;
    }

    // Heavy Attack
    void SlidingH(float _impact)
    {
        if (m_basicMove.AmParkouring)
            return;

        m_impact = _impact;
        m_amGoingToHit = true;
        m_attacking = true;
        m_stats.StaminaChangedBy(-0.4f);
    }
    // enable and disable the trigger on fist and kick
    void CanDamage(int _bodyPartIndex)
    {
        if (m_basicMove.AmParkouring || m_anim.GetBool("Hit Response"))
            return;

        switch (_bodyPartIndex)
        {
            
            case 1:
                m_leftKick.enabled  = true;
               // m_leftKick.isTrigger = false;
                break;
            case 2:
                m_rightKick.enabled = true;
                // m_rightKick.isTrigger = false;
                break;
            case 3:
                m_leftFist.enabled = true;
                //m_leftFist.isTrigger = false;

                break;
            case 4:
                m_rightFist.enabled = true;
                //m_rightFist.isTrigger = false;

                break;
        }
    }
    void CanNotDamage()
    {
        m_leftKick.enabled = false;
        m_rightKick.enabled = false;
        m_leftFist.enabled = false;
        m_rightFist.enabled = false;
        m_amGoingToHit = false;
    }
    void AttackEnd()
    {
        m_attacking = false;
        Debug.Log("shuashua");
    }
    #endregion



    public bool CombatMovement()
    {
        if (m_amGoingToHit)
        {
            if (m_combatSlot.TheNextTarget == null)
                transform.position += transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd") * 0.5f;
            else if (m_distanceToTarget > 1.3f)
                transform.position += transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd");
            return true;
        }
        else if (m_anim.GetBool("Hit Response")) // m_amDodging || 
        {
            transform.position -= transform.forward * m_anim.GetFloat("sliding spd") * Time.deltaTime;
            return true;
        }
        else if (m_amDodging || m_amSpecialAttack)
        {
            transform.position += transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd");
            return true;
        }
        else return false;
    }
    public float TakingDamageFromEnemies(Collider collider)
    {
        // just behind and front
        bool takingHitFromBehind = false;
        bool takingHitFromFront = false;
        theOneWhoHitMe = collider.GetComponentInParent<Enemy>();
        if (theOneWhoHitMe == null)
            return 0;

        if (theOneWhoHitMe.StunAttack)
        {
            if (!m_anim.GetCurrentAnimatorStateInfo(0).IsName ("beingStunned"))
                m_anim.SetTrigger("being blocked");
        }
        else
        {
            Vector3 hitComingFromDir = theOneWhoHitMe.transform.position - this.transform.position;
            hitComingFromDir.y = 0;
            hitComingFromDir = transform.InverseTransformDirection(hitComingFromDir);

            if (hitComingFromDir.z >= -0.7f)
            {
                takingHitFromFront = true;
                Vector3 hitDir = collider.transform.position - transform.position;
                hitDir.y = 0;
                // get the hir direction in body space
                hitDir = transform.InverseTransformDirection(hitDir).normalized;
                m_anim.SetFloat("take hit direction x", hitDir.x);
            }
            else
                takingHitFromBehind = true;

            m_anim.SetBool("take hit from behind", takingHitFromBehind);
            m_anim.SetBool("take hit from front", takingHitFromFront);
            m_anim.SetFloat("taking hit level", collider.transform.position.y - theOneWhoHitMe.transform.position.y);
            m_anim.SetFloat("taking hit Impact", theOneWhoHitMe.MyImpact);
            m_anim.SetTrigger("taking hit");
        }

        damageImpact = theOneWhoHitMe.MyImpact;
        damageImpact = (!m_amHavingAOpenFlank) ? damageImpact : 2 * damageImpact;

        ResetOnBeingHit();

        // Audio Call
        return 15 * damageImpact;
    }
    public void BeatenToARagdoll(Collider  collider, float _hp = 100)
    {
        if (damageImpact > 1f || _hp < 0)
        {
            if (m_RM.State == RagdollManager.RagRollState.NotRagDoll)
                m_RM.BecomeRagDoll();

            Vector3 contact = collider.transform.position;
            Vector3 hitDir = transform.position - collider.transform.position;
            hitDir.y = 0;
            Ray ray = new Ray(contact, hitDir);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 5, LayerMask.GetMask("H_Rag")))
                hitInfo.rigidbody.AddForce(hitDir.normalized * 50 * theOneWhoHitMe.MyImpact, ForceMode.VelocityChange);
        }

    }
    public void ResetOnQuitCombat(bool _resetAnimBlock = false)
    {
        m_amGoingToHit = false;
        m_amSpecialAttack = false;
        m_amHavingAOpenFlank = false;

        ResetDodging();
        CanNotDamage();


        m_amTargetting = false;
        if(_resetAnimBlock)
          m_attacking = false;
        m_basicMove.StickToCurrentRotation = false;
        m_anim.SetBool("Hit Response", false);
    }


    #region Combat Properties
    public bool IsTargetting
    {
        get { return m_amTargetting; }
    }
    public bool IsDodging
    {
        get { return m_amDodging; }
    }    
    public Vector3 DodgeDir
    {
        get { return m_DodgeDir; }
    }
    public bool AttackButtonOnClick
    {
        get {
            return (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"));
        }
    }
    public bool AttackButtonHold
    {
        get { return m_punch || m_heavy; }

    }
    public float MyImpact
    {
        get { return m_impact; }
    }
    public bool DamageImmuen
    {
        get { return m_amDamageImmune; }
    }

    public bool InCombatAction
    {
        get { return m_attacking || m_amSpecialAttack || m_amDodging; }
    }

    #endregion
    //Event manager stops and starts the player
    void EventManager_pause()
    {
        if (m_anim)
            m_anim.enabled = false;
        if (m_body)
            m_body.isKinematic = true;
    }

    void EventManager_unpause()
    {
        if (m_anim)
            m_anim.enabled = true;
        if (m_body)
            m_body.isKinematic = false;
    }



}
