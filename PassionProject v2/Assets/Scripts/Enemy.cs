using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public abstract class Enemy : MonoBehaviour {


    public enum EnemyClass
    {
        Douglas = 1,
        Mutant = 2
    }
    public enum BehavivorState
    {
        Navigating = 0,
        Chasing,
        InCombat,
        Strafing,
        KnockDown
    }

    [SerializeField, ReadOnly] BehavivorState myState;
    [SerializeField, ReadOnly] protected EnemyClass myClass;
    //[SerializeField, ReadOnly]
    bool isAlive, showUI, beingLocked, amStandingUp, fightIdle;
    [SerializeField, ReadOnly]
    protected bool timeToAttack, attacking, stunAttack;

    protected Rigidbody   body;
    protected CapsuleCollider m_Collider;
    protected AudioSource[] m_audioSources;

    NavMeshAgent nav;
    RagdollManager  m_rM;
    Spawner         m_spawner;

    protected static Combat hero;
    protected static CombatSlots combatSlots;
    protected Animator m_anim;
    protected  EnemyStatus m_stats;
    protected float m_footTouchGround;


    [SerializeField] protected int belongToSlot;  // Only need in combat
    [SerializeField] protected float rotationSpdInCombat = 5f;
    [SerializeField] float waitingTimeBeforeDelete = 10;


    //[SerializeField, ReadOnly] 
    float movement_speed;
    //[SerializeField, ReadOnly] 
    protected float myImpact;

    // navigation
    //[SerializeField, ReadOnly] 
    protected bool nav_inPosition;

    [SerializeField]
    float nav_TimeGap = 5, nav_speed = 0.5f;// foe is standing on the waypoints, nav speed only needs for nav agent to work

    [SerializeField] Transform [] waypoint;
    int   nav_point;        // tells which the waypoints the foe is going to move to
    float nav_timer;        // the past time on the waypoints, also 


    // combat
    [SerializeField] Collider m_leftKick, m_rightKick, m_leftFist, m_rightFist;
    [SerializeField] GameObject dropOff;

    


    // Use this for initialization
    protected virtual void Start () {

        m_anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        m_rM = GetComponent<RagdollManager>();
        m_stats = GetComponent<EnemyStatus>();
        m_Collider = GetComponent<CapsuleCollider>();
        m_audioSources = GetComponents<AudioSource>();
        if (hero == null)
        {
            hero = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
            combatSlots = hero.GetComponentInChildren<CombatSlots>();
        }

        #region 1. check if this enemy has waypoints 
            if (waypoint.Length == 0)
            {
                myState = BehavivorState.Chasing;
                movement_speed = 2;
            }
            else
            {
                myState = BehavivorState.Navigating;
                for (int i = 0; i < waypoint.Length; i++)
                {
                    waypoint[i].parent = null;
                }
            }
            #endregion

        //combatState = CombatState.Act;

        nav_inPosition = true;   // enemy always starts at waypoint 0
        nav_point = 0;
        nav_timer = nav_TimeGap;
        belongToSlot = -1;
        isAlive = true;

        //m_rightKick.isTrigger = true;
        //m_leftKick.isTrigger = true;
        //m_leftFist.isTrigger = true;
        //m_rightFist.isTrigger = true;
    }

    // Update is called once per frame
    protected virtual void Update () {

        IsPaused();
        m_stats.UIUpdate(showUI, beingLocked);

        if (m_stats.CurrentHp <= 0)
            isAlive = false;

        if (!isAlive)
        {
            showUI = false;
            RemoveThisGuy();
            return;
        }


        amStandingUp = m_anim.GetCurrentAnimatorStateInfo(0).IsTag("standup");

        #region Update Depend on The AI state
        switch (myState)
        {
            case BehavivorState.Navigating:
                OnNavigation();
                break;
            case BehavivorState.Chasing:
                OnChasing();
                break;
            case BehavivorState.InCombat:
                if (!amStandingUp)
                    InCombat();
                break;
            case BehavivorState.Strafing:
                OnStrafing();
                break;
            case BehavivorState.KnockDown:
                OnKnockDown();
                break;
        }
        #endregion
        // for updateAnimation
        UpdateAnimator();
        m_anim.SetFloat("normalized time", m_anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    protected virtual void FixedUpdate()
    {
        if (amStandingUp || m_rM.State != RagdollManager.RagRollState.NotRagDoll)
            return;
        Vector3 newDir = hero.transform.position - transform.position;
        newDir.y = 0;
        switch (myState)
        {
            case BehavivorState.Navigating:
                break;
            case BehavivorState.Chasing:
                newDir = Vector3.RotateTowards(transform.forward, newDir, rotationSpdInCombat * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);
                break;
            case BehavivorState.InCombat:
                {

                    if (m_anim.GetBool("Hit Response"))
                    {
                        transform.position -= transform.forward * Time.deltaTime * m_anim.GetFloat("sliding spd");
                        //newDir = Vector3.RotateTowards(transform.forward, newDir, m_anim.GetFloat("rotating spd") * Time.deltaTime, 0);
                        //transform.rotation = Quaternion.LookRotation(newDir);
                    }
                    else 
                    {
                        float slidingSpd = m_anim.GetFloat("sliding spd");
                        if (newDir.sqrMagnitude >= 1 || slidingSpd < 0)
                            transform.position += transform.forward * Time.deltaTime * slidingSpd;
                        newDir = Vector3.RotateTowards(transform.forward, newDir, m_anim.GetFloat("rotating spd") * Time.deltaTime, 0);
                        transform.rotation = Quaternion.LookRotation(newDir);
                    }
                }
                break;
            case BehavivorState.Strafing:
                newDir = Vector3.RotateTowards(transform.forward, newDir, rotationSpdInCombat * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(newDir);
                break;
        }

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        #region 1. Am I enter the combat slot
        if (other.name == "combatSlots")
        {
            if (myState == BehavivorState.InCombat)
                return;
            // when I try to access the combat slots, if there exsits empty slots and am not in combat state
            // then let I can jump in, if there is not empty slot then I will play background
            if (!combatSlots.FULL)
            {
                this.EnterCombatMode();
            }
            else
               this.EnterStrafingMode();
        }
        #endregion
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        #region 1. Am I getting out the combat slot
        if (other.name == "combatSlots")
        {
            if (myState == BehavivorState.InCombat)
            {
                combatSlots.RemoveEnemyFromCombatSlot(this);
                CanNotDamage();
                EnterChasingMode();
            }
            //if (m_rM.State == RagdollManager.RagRollState.NotRagDoll )
            //{
            //    EnterChasingMode();
            //}
        }
        #endregion
    }

    void IsPaused()
    {
        if (!m_anim.enabled)
        {
            return;
        }

    }
    void RemoveThisGuy()
    {
        // disable damage register
        CanNotDamage();
        nav.enabled = false;
        if (m_rM.State == RagdollManager.RagRollState.NotRagDoll)
            m_rM.BecomeRagDoll();
        if (m_rM.enabled)
        {
            // before this get delete, do once
            if (m_spawner != null)
                m_spawner.OnCloneDead(this);
            nav_timer = 0;
            m_rM.enabled = false;
            m_stats.UIUpdate(false, beingLocked);
            if (dropOff != null)
                Instantiate(dropOff, this.transform.position, this.transform.rotation, null);

        }
        // just reset the timer between become a Ragdoll and disable the ragdoll manager to avoid constantly reset the timer
        nav_timer += Time.deltaTime;
        if (nav_timer > waitingTimeBeforeDelete)
        {
            Destroy(this.gameObject);
            //this.gameObject.SetActive(false);
        }
    }
    void OnNavigation()
    {
        if (waypoint.Length > 1)
        {
            if (nav.remainingDistance < nav.radius + 1 && !nav_inPosition)
            {
                // stop
                nav.Stop();
                nav.speed = 0;
                movement_speed = 0;
                nav_inPosition = true;

            }
            if (nav_inPosition)
            {
                //waiting
                nav_timer -= Time.deltaTime;

            }
            if (nav_timer <= 0)
            {
                // moving
                nav_point++;
                if (nav_point >= waypoint.Length)
                {
                    nav_point = 0;
                }
                nav.Resume();
                nav.speed = nav_speed;
                nav.SetDestination(waypoint[nav_point].position);
                nav_inPosition = false;
                nav_timer = nav_TimeGap;
                movement_speed = 1;
            }
        }

    }
    void OnChasing()
    {
        if (amStandingUp)
            return;
        nav.SetDestination(hero.transform.position);

        if (myClass == EnemyClass.Mutant)
            return;
        if (nav.remainingDistance < combatSlots.Radius - 1)
            this.EnterStrafingMode();
    }
    void OnStrafing()
    {
        bool walkBack = false;
        // Enemy in strafing aways keep distance to the center of combat slot
        float relativeDis = Vector3.Distance(this.transform.position, combatSlots.transform.position);
        if (combatSlots.FULL)
        {
            // If too close the combat cycle Walk back 
            if (relativeDis <= combatSlots.Radius + 1)
                movement_speed = 1;
            else
                movement_speed = 0;
                //walkBack = true;
        }
        else
        {
            if (relativeDis <= combatSlots.Radius)
                this.EnterCombatMode();
            else if (relativeDis > combatSlots.Radius)
                this.EnterChasingMode();            
        }
        m_anim.SetBool("walk back straf", walkBack);
    }
    void OnKnockDown()
    {
        if (m_rM.State == RagdollManager.RagRollState.NotRagDoll)
        {
            if (myClass == EnemyClass.Mutant)
                EnterCombatMode();
            else
                myState = BehavivorState.Strafing;
        }

    }
    void UpdateAnimator()
    {
        m_anim.SetFloat("speed", movement_speed, 0.2f, Time.deltaTime);
        m_anim.SetBool("fightIdle", fightIdle);
    }
    void EnterCombatMode()
    {
        combatSlots.AddEnemyInCombatSystem(this);
        if (myState == BehavivorState.Strafing)
            m_anim.SetBool("walk back straf", false);

        CanNotDamage();
        fightIdle = true;
        // do inPosition check
        nav_inPosition = false;

        // disable Nav Function
        nav.speed = 0;
        nav.enabled = false;


        // update Anim
        myState = BehavivorState.InCombat;

    }
    void EnterStrafingMode()
    {

        // sometime in combat mode, nav is disable
        // just prevent the error
        if (nav.enabled)
            nav.Stop();
        showUI = false;
        movement_speed = 1;
        fightIdle = false;
        myState = BehavivorState.Strafing;
        m_anim.SetTrigger("BecomeBackground");
    }
    void EnterChasingMode()
    {
        if (myClass != EnemyClass.Mutant)
        {
            if (myState == BehavivorState.Strafing)
                m_anim.SetBool("walk back straf", false);
        }
        // re active Nav Func
        nav.enabled = true;
        nav.speed = nav_speed;
        nav_inPosition = false;
        // just not showing hp bar
        showUI = false;

        combatSlots.ReleaseSlot(this);
        // update Anim
        movement_speed = 2;
        fightIdle = false;
        // switch state
        myState = BehavivorState.Chasing;
    }

    protected bool AttackResetWhenGetHit()
    {
        // if enemy is doing hit response, reset his attacking check
        if (m_anim.GetBool("Hit Response"))
        {
            m_anim.ResetTrigger("attack");
            m_stats.TurnAttackIcon(false);
            if (attacking)
                CanNotDamage();
            return true;
        }
        else return false;
    }

    abstract protected void InCombat();

    #region Public Interface
    abstract public float TakingHitFromHero(Collider collider);
    public virtual bool BeatenToARagdoll(Collider collider, float _hp = 100)
    {
        if (hero.MyImpact > 1f || _hp < 0)
        {
            if (m_rM.State == RagdollManager.RagRollState.NotRagDoll)
            {
                myState = BehavivorState.KnockDown;
                m_rM.BecomeRagDoll();
            }
            CanNotDamage();
            showUI = false;
            Vector3 hitDir = transform.position - collider.transform.position;
            hitDir.y = 0;
            Ray ray = new Ray(collider.transform.position, hitDir);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 5, LayerMask.GetMask("E_Rag")))
                hitInfo.rigidbody.AddForce(hitDir.normalized * 50 * hero.MyImpact, ForceMode.VelocityChange);
            return true;
        }
        return false;

    }

    #endregion

    #region Properties


    public Spawner MySpawner
    {
        get { return m_spawner; }
        set { m_spawner = value; }
    }
    public bool StunAttack
    {
        get { return stunAttack; }
    }

    public EnemyClass MyClass
    {
        get { return myClass; }
    }
    public float MyImpact
    {
        get { return myImpact; }
    }
    public BehavivorState MYSTATE
    {
        get { return myState; }
    }
    public bool IsAlive
    {
        get { return isAlive; }
    }
    public int InSlot
    {
        get { return belongToSlot; }
        set { belongToSlot = value; }
    }
    public bool ShowUI
    {
        set { showUI = value; }
    }
    public bool Attack
    {
        get { return timeToAttack; }
        set { timeToAttack = value; }
    }
    public bool BeingLocked
    {
        get { return beingLocked; }
        set { beingLocked = value; }
    }
    public float Width
    {
        get { return m_Collider.radius; }
    }
    #endregion

    #region Animation Event


    void TakeAction(float _impact)
    {
        myImpact = _impact;
        //combatState = CombatState.Act;
    }
    void CanDamage(int _bodyPartIndex)
    {
        switch (_bodyPartIndex)
        {
            case 1:
                m_leftKick.enabled = true;
                break;
            case 2:
                m_rightKick.enabled = true;
                break;
            case 3:
                m_leftFist.enabled = true;
                break;
            case 4:
                m_rightFist.enabled = true;
                break;
        }

        switch (myClass)
        {
            case EnemyClass.Douglas:
                AudioLibrary.PlayBadManHeh(m_audioSources[1], Random.Range(0.1f, 0.2f), 1f);
                break;
            case EnemyClass.Mutant:
                break;
            default:
                break;
        }
        attacking = true;
        m_stats.TurnAttackIcon(false);
    }

    /// <summary>
    /// Initialize the damage register
    /// </summary>
    protected void CanNotDamage()
    {
        m_leftKick .enabled = false;
        m_rightKick.enabled = false;
        m_leftFist.enabled = false;
        m_rightFist.enabled = false;

        attacking = false;
    }

    #endregion


}
