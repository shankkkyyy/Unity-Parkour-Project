using UnityEngine;

public class Douglas : Enemy {


    [SerializeField]
    float forwardSqDis = 16, backSqDis = 4;

    [SerializeField]
    int attackStyle;
    // Use this for initialization
    protected override void Start()
    {
        myClass = EnemyClass.Douglas;
        m_footTouchGround = 0.4f;
        base.Start();
    }

    protected override void InCombat()
    {
        if (AttackResetWhenGetHit())
            return;
        // if the enemy is not nav_inPosition (angle position), move to position, if attack call during movement, disable movement and attack
        // after attack enemy will run GetTheProperSlot and move into the proper position (angle position)
        // when the enemy move into the right angle position, then put it in the right distance, during this movement, nav_inPosition still holds
        // if the enemy is inPosition (on the slot direction), wait for attack call

        // attack
        // dodge
        // dodge + attack 

        Vector3 vectorToPlayer = hero.transform.position - this.transform.position;
        vectorToPlayer.y = 0;
        float distanceSqToPlayer = vectorToPlayer.sqrMagnitude;
        float forwardInCombat = 0;
        float combatStraffingSpd = 0;



        //Debug.Log(hero.AttackButtonOnClick);
        if (m_anim.GetBool("can attack"))
        {
            #region 1. check the dodge condition
            if (combatSlots.TheNextTarget == this && hero.AttackButtonOnClick)
            {
               m_anim.SetTrigger("dodge");
            }
            else
            {
                m_anim.ResetTrigger("dodge");
                #region 2. Check the attack permisiion
                if (timeToAttack)
                {
                    // how we attack
                    // check on distance
                    if (distanceSqToPlayer >= 25)
                        m_anim.SetInteger("action", 0);
                    else if (distanceSqToPlayer > 16)
                        m_anim.SetInteger("action", Random.Range(10, 12));
                    else
                        m_anim.SetInteger("action", Random.Range(2, 10));

                    m_stats.TurnAttackIcon(true);
                    m_anim.SetTrigger("attack");
                    combatSlots.ReleaseSlot(this);
                }
                #endregion

                #region 3. Movement in combat slots system
                if (belongToSlot == -1)
                {
                    // If I dont have a slot, I will find the closet slot
                    combatSlots.PutEnemyInSlot(this, combatSlots.GetTheProperSlot(this.transform.position));
                    nav_inPosition = false;
                }
                else
                {
                    // Check if the distance to player is greater minimum distance
                    // If it does, move backward
                    if (distanceSqToPlayer <= backSqDis)
                        forwardInCombat = -1f;
                    else
                    {
                        #region 3.2 When the distance to player is greater minmum distance
                        if (!nav_inPosition)
                        {
                            #region 3.2.1 move me to the correct regular position
                            Vector3 slotDir = Vector3.zero;
                            slotDir = combatSlots.GetAngularDisFromSlot(this, belongToSlot);
                            if (slotDir.x >= 0)
                            {
                                if (slotDir.z >= 0)
                                    combatStraffingSpd = 1;
                                else
                                {
                                    if (slotDir.x > 0.5f)
                                        combatStraffingSpd = 1;
                                    else if (slotDir.x > 0.25f)
                                        combatStraffingSpd = .5f;
                                    else
                                    {
                                        combatStraffingSpd = 0f;
                                        nav_inPosition = true;
                                    }

                                }
                            }
                            else
                            {
                                if (slotDir.z >= 0)
                                    combatStraffingSpd = -1;
                                else
                                {
                                    if (slotDir.x < -0.5f)
                                        combatStraffingSpd = -1;
                                    else if (slotDir.x < -0.25f)
                                        combatStraffingSpd = -.5f;
                                    else
                                    {
                                        combatStraffingSpd = 0f;
                                        nav_inPosition = true;
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            if (distanceSqToPlayer >= forwardSqDis)
                                forwardInCombat = 1f;
                        }
                        #endregion
                    }
                }
                #endregion
            }
            #endregion
        }

        FootStepAudio();

        // 3. Animation Call
        m_anim.SetFloat("angDis to slot", combatStraffingSpd, 0.2f, Time.deltaTime);
        m_anim.SetFloat("forwardInCombat", forwardInCombat, 0.1f, Time.deltaTime);
        m_anim.SetBool("combat in position", nav_inPosition);
    }

    public override float TakingHitFromHero(Collider collider)
    {
        #region 1. Get the location of being Hit Info
        bool takingHitFromBehind = false;
        bool takingHitFromFront = false;
        float hitLevel = collider.transform.position.y - hero.transform.position.y;
        // calculate the direction where the hit comes from in local space, regardless y axis
        // playerDir tells the player's position, to identify the back attack, and side attack

        // hitDir tells when player is attack from front of the enemy, where the player hit on the enemy
        Vector3 playerDir;

        playerDir = hero.transform.position - transform.position;
        playerDir.y = 0;
        playerDir = transform.InverseTransformDirection(playerDir).normalized;


        if (playerDir.z >= 0f)
        {
            takingHitFromFront = true;
            Vector3 hitDir = collider.transform.position - transform.position;
            hitDir.y = 0;
            // get the hir direction in body space
            hitDir = transform.InverseTransformDirection(hitDir).normalized;
            m_anim.SetFloat("hit direction x", hitDir.x);

        }
        else
            takingHitFromBehind = true;
        #endregion


        #region 2. Animation Call
        m_anim.SetBool("taking hit from behind", takingHitFromBehind);
        m_anim.SetBool("taking hit from front", takingHitFromFront);
        m_anim.SetFloat("taking hit Level", hitLevel);
        m_anim.SetFloat("taking hit Impact", hero.MyImpact);
        m_anim.SetTrigger("taking hit");
        #endregion

        #region 3. Audio Call
        AudioLibrary.PlayFistFightImpact(m_audioSources[0]);
        AudioLibrary.PlayBadManStruggle(m_audioSources[1], Random.Range(0.1f, 0.2f), hero.MyImpact * 0.5f);
        #endregion

        return 15 * hero.MyImpact;
    }


    void FootStepAudio()
    {
        bool inTransistion = m_anim.IsInTransition(0);
        float normalizedTime = m_anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (inTransistion)
        {
            if (normalizedTime > .4f)
                m_footTouchGround = .9f;
            else
                m_footTouchGround = .4f;
        }
        else if (m_anim.GetCurrentAnimatorStateInfo(0).IsName("combatMotion"))
        {
            if (normalizedTime > m_footTouchGround)
            {
                AudioLibrary.PlayHumanFootStep(m_audioSources[0], Mathf.Abs(m_anim.GetFloat("forwardInCombat")));
                m_footTouchGround += 0.5f;
            }
        }


    }

}
