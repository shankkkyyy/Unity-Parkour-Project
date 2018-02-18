using UnityEngine;

public class Mutant : Enemy {


    enum RageLevel
    {
        nonRage = 0,
        angry,
        rage
    }

    enum MutantAction
    {
        none = 0,
        jumpEvade,
        backStepCounter,
        block,
        nonRageCombo_near,
        nonRageCombo_far,
        angryCombo_near,
        rageCombo_near,
        jumpAttack,
        blockInRage

    }

    [SerializeField, ReadOnly]
    bool underArmor, immuneToDamage, armorRecoverStart, armorRecovering, runComboBreakerTimer;
    [SerializeField]
    SphereCollider areaDamage;

    [SerializeField, ReadOnly]
    RageLevel  rageLevel;
    [SerializeField]
    float forwardSqDis = 16, backSqDis = 7,
    armorRecoveringRangeDamageRadius = 3f, comboBreakerGap = .5f,
    groundSlampRadius = 2.5f, battleStunRadius = 3f;



    static float attackRateInSec = 1f, armorOffTimeGap = 5f, comboBreakerOnAmountOfDamage = 33f;
    static int comboBreakerOnNumOfCombo = 3;
    [SerializeField, ReadOnly]
    float attackTimer, armorOffTimer, distanceSqToPlayer, areaDamageRadius,
        comboBreakerComboNumCounter, comboBreakerDamageCounter, 
        comboBreakerResetTimer;

    [SerializeField]
    ParticleSystem groundSlamp, battleCry;

    protected override void Start()
    {
        attackTimer = attackRateInSec;
        // comboBreaker Ini
        ComboBreakerReset();
        myClass = EnemyClass.Mutant;
        rageLevel = RageLevel.nonRage;
        areaDamage.isTrigger = true;
        switch (GameManager.GameDifficulty)
        {
            case GameManager.GameMode.relaxed:
                SetMutantCombatProp(1.5f, 7f, 50f, 4);
                break;
            case GameManager.GameMode.expert:
                SetMutantCombatProp(1.2f, 6f, 40f, 4);
                break;
            case GameManager.GameMode.insane:
                SetMutantCombatProp(1f, 5f, 33f, 3);
                break;
        }
        base.Start();


        // come out with full armor
        armorOffTimer = armorOffTimeGap;
        underArmor = true;
        m_stats.ArmorValue = 100;
        
    }
    protected override void InCombat()
    {

        #region 1. Determine my Rage Level By Hp amount
        // check rageLevel status;
        if (m_stats.CurrentHp > 0.66f * m_stats.StaringHp)
            rageLevel = RageLevel.nonRage;
        else if (m_stats.CurrentHp > 0.33f * m_stats.StaringHp)
            rageLevel = RageLevel.angry;
        else
        {
            // in rage mode, we dont have sheild anymore, but a more aggressive attacking action instead
            if (rageLevel != RageLevel.rage)
            {
                rageLevel = RageLevel.rage;
                attackRateInSec *= 0.5f;
                comboBreakerComboNumCounter -= 1;
            }

        }
        #endregion

        // comboBreaker timer Reset after certain time the boss does not being attack
        if (runComboBreakerTimer)
        {
            comboBreakerResetTimer -= Time.deltaTime;
            if (comboBreakerResetTimer <= 0)
                ComboBreakerReset(true);
        }

        if (rageLevel != RageLevel.rage)
        {
            #region 2. Armor Recovery Mechanim
            // check if I am under armor protection, so I can have the ability 
            // for combo breaking and not react when getting hit while I am attacking
            if (m_stats.ArmorValue <= 0)
            {

                if (underArmor)
                {
                    underArmor = false;
                    comboBreakerComboNumCounter = comboBreakerOnNumOfCombo;
                    comboBreakerDamageCounter = comboBreakerOnAmountOfDamage;
                }

                // time delay for recovering armor
                if (!underArmor)
                    armorOffTimer -= Time.deltaTime;

                // recovering armor
                if (armorOffTimer <= 0 && (m_anim.GetBool("can attack") || m_anim.GetBool("Hit Response")))
                {
                    underArmor = true;
                    if (!armorRecoverStart)
                    {
                        armorRecoverStart = true;
                        m_stats.ArmorValue = 0.1f;
                        CanNotDamage();
                        m_stats.TurnAttackIcon(false);
                    }
                }
            }

            // During armorRecoving, immune to damage, cast area damage
            if (armorRecoverStart)
            {
                // initialize armor recharging
                if (!immuneToDamage)
                {
                    immuneToDamage = true;
                    m_anim.SetTrigger("armorRecovering");
                    areaDamage.radius = 0;
                    areaDamage.enabled = true;
                }
                // during recharging
                if (armorRecovering)
                {
                    m_stats.ArmorValue += 45 * Time.deltaTime;
                    areaDamage.radius = Mathf.Lerp(areaDamage.radius, armorRecoveringRangeDamageRadius, 10 * Time.deltaTime);
                    if (Mathf.Abs(areaDamage.radius - armorRecoveringRangeDamageRadius) <= 0.1f)
                        areaDamage.radius = 0;
                }

                // recharging finalize
                if (m_stats.ArmorFull)
                {
                    underArmor = true;
                    armorOffTimer = armorOffTimeGap;
                    immuneToDamage = false;
                    armorRecoverStart = false;
                    armorRecovering = false;
                    areaDamage.radius = 0;
                    areaDamage.enabled = false;
                }
                return;
            }
            #endregion
        }


        if (AttackResetWhenGetHit())
            return;

        // Performing Area Attack
        if (stunAttack)
        {
            if (PerformAreaDamage(areaDamageRadius))
            {
                stunAttack = false;
                areaDamage.enabled = false;
            }
        }

        #region 3. Combat Infomation Gathering
        // get attacking freq

        if (m_anim.GetBool("can attack"))
            // one attack at a time ~~
            attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            timeToAttack = true;
            attackTimer = attackRateInSec;
        }

        // get distance to player info
        Vector3 vectorToPlayer = hero.transform.position - this.transform.position;
        vectorToPlayer.y = 0;
        distanceSqToPlayer = vectorToPlayer.sqrMagnitude;
        // get player direction 
        vectorToPlayer = transform.InverseTransformDirection(vectorToPlayer).normalized;
        #endregion

        #region 3. combat behaviour
        bool turning = m_anim.GetCurrentAnimatorStateInfo(0).IsTag("turning");
        float forwardInCombat = 0;

        // normal attack:
        //if (timeToAttack && m_anim.GetBool("can attack"))
        if (timeToAttack)
        {
            // if player is behind the boss
            if (vectorToPlayer.z < .5f)
            {
                if (!turning)
                    if (vectorToPlayer.x > 0)
                        m_anim.SetTrigger("turn right");
                    else
                        m_anim.SetTrigger("turn left");
                else
                {
                    m_anim.ResetTrigger("turn right");
                    m_anim.ResetTrigger("turn left");
                }
            }
            else
            {
                m_anim.SetTrigger("attack");
                switch (rageLevel)
                {
                    case RageLevel.nonRage:
                        if (distanceSqToPlayer < 8)
                            m_anim.SetInteger("action", (int)MutantAction.nonRageCombo_near);
                        else if (distanceSqToPlayer < 36f)
                            m_anim.SetInteger("action", (int)MutantAction.nonRageCombo_far);
                        else
                            m_anim.ResetTrigger("attack");

                        ComboBreakerReset();

                        break;

                    case RageLevel.angry:
                        if (distanceSqToPlayer < 25)
                            m_anim.SetInteger("action", (int)MutantAction.angryCombo_near);
                        else if (distanceSqToPlayer < 64f)
                            m_anim.SetInteger("action", (int)MutantAction.jumpAttack);
                        else
                            m_anim.ResetTrigger("attack");

                        ComboBreakerReset();

                        break;
                    case RageLevel.rage:
                        if (distanceSqToPlayer < 25)
                            m_anim.SetInteger("action", (int)MutantAction.rageCombo_near);
                        else if (distanceSqToPlayer < 64f)
                            m_anim.SetInteger("action", (int)MutantAction.jumpAttack);
                        else
                            m_anim.ResetTrigger("attack");

                        break;
                }
                m_stats.TurnAttackIcon(true);
                // the combo breaker counter will reset after a normal attack
            }
        }


        // movement : keep distance to player
        if (distanceSqToPlayer >= forwardSqDis)
            forwardInCombat = 1f;
        m_anim.SetFloat("forwardInCombat", forwardInCombat, 0.3f, Time.deltaTime);
        #endregion

        // Footstep Audio
        // FootStepAudio();

        // reset state
        timeToAttack = false;
    }
    protected override void OnTriggerExit(Collider other)
    {
        // do nothing for boss
        // because once boss in the combat mode, then always in combat mode
    }


    /// <summary>
    /// The events trigger when this character get hitten by hero damage
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public override float TakingHitFromHero(Collider collider)
    {

        // withour armor
        if (immuneToDamage)
            return 0;

        bool BossAction = m_anim.GetCurrentAnimatorStateInfo(0).IsTag("BossAction");
        #region 1. Get Hit Info

        bool takingHitFromBehind = false;
        bool takingHitFromFront = false;
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
            m_anim.SetFloat("taking hit dir x", hitDir.x);
        }
        else
            takingHitFromBehind = true;

        m_anim.SetBool("taking hit from behind", takingHitFromBehind);
        m_anim.SetBool("taking hit from front", takingHitFromFront);

        #endregion

        #region 2. Determine Damage

        m_anim.SetFloat("taking hit Impact", hero.MyImpact);
        float damage = ((m_stats.ArmorValue > 0) ? 15 : 20) * hero.MyImpact;

        switch (rageLevel)
        {
            case RageLevel.nonRage:
            case RageLevel.angry:
                #region Counter and evade or response to hit during the nonRage and angry mode


                bool hitResponseCondition =
                     comboBreakerComboNumCounter > 0 && comboBreakerDamageCounter > 0 &&
                     (!BossAction || takingHitFromBehind);

                if (m_stats.ArmorValue > 0)
                {
                    runComboBreakerTimer = true;
                    comboBreakerResetTimer = comboBreakerGap;

                    if (hitResponseCondition)
                    {
                        m_anim.SetTrigger("taking hit");
                    }
                    else if (comboBreakerDamageCounter <= 0 || comboBreakerComboNumCounter <= 0)
                    {
                        // counter attack and evade
                        m_anim.SetTrigger("comboBreaker");
                        if (rageLevel == RageLevel.nonRage)
                        {
                            if (takingHitFromFront)
                                m_anim.SetInteger("action", (int)MutantAction.backStepCounter);
                            else if (takingHitFromBehind)
                                m_anim.SetInteger("action", (int)MutantAction.jumpEvade);
                        }
                        else
                        {
                            if (takingHitFromFront)
                                m_anim.SetInteger("action", (int)MutantAction.block);
                            else if (takingHitFromBehind)
                                m_anim.SetInteger("action", (int)MutantAction.jumpEvade);
                        }
                        // reset breaker counter after perform a combo breaker
                        comboBreakerComboNumCounter = comboBreakerOnNumOfCombo;
                        comboBreakerDamageCounter = comboBreakerOnAmountOfDamage;
                        damage = 0;
                    }

                    if (!BossAction)
                    {
                        comboBreakerDamageCounter -= damage;
                        comboBreakerComboNumCounter -= 1;
                    }

                }
                else
                    m_anim.SetTrigger("taking hit");
                #endregion
                break;
            case RageLevel.rage:
                #region Counter and evade or response to hit during the rage mode

                runComboBreakerTimer = true;
                comboBreakerResetTimer = comboBreakerGap;

                if (comboBreakerDamageCounter <= 0 || comboBreakerComboNumCounter <= 0)
                {
                    m_anim.SetTrigger("comboBreaker");
                    if (takingHitFromFront)
                    {
                        if (comboBreakerDamageCounter <= 0)
                            m_anim.SetInteger("action", (int)MutantAction.block);
                        else if (comboBreakerComboNumCounter <= 0)
                            m_anim.SetInteger("action", (int)MutantAction.blockInRage);
                    }
                    else if (takingHitFromBehind)
                            m_anim.SetInteger("action", (int)MutantAction.jumpEvade);
                    comboBreakerComboNumCounter = comboBreakerOnNumOfCombo;
                    comboBreakerDamageCounter = comboBreakerOnAmountOfDamage;
                    damage = 0;
                }
                else
                {
                    m_anim.SetTrigger("taking hit");
                }

                comboBreakerDamageCounter -= damage;
                comboBreakerComboNumCounter -= 1;

                #endregion
                break;
        }
        #endregion

        if (m_anim.GetCurrentAnimatorStateInfo(0).IsName("hit_on_the_back"))
        {
            // animation will not be interrupt
            m_anim.ResetTrigger("taking hit");
        }


        #region 3. Audio Call
        AudioLibrary.PlayFistFightImpact(m_audioSources[0]);
        if (!underArmor)
            AudioLibrary.PlayMonsterInPain(m_audioSources[1], Random.Range(0.1f, 0.2f), hero.MyImpact * 0.4f);
        else if (m_stats.CurrentHp <= damage)
            AudioLibrary.PlayMonsterDead(m_audioSources[1], Random.Range(0.15f, 0.2f));
        #endregion

        return damage;
    }

    public override bool BeatenToARagdoll(Collider collider, float _hp = 100)
    {
        if (immuneToDamage)
            return false;
        if (base.BeatenToARagdoll(collider, _hp))
        {
            ComboBreakerReset();
            return true;
        }
        else
            return false;
    }



    bool PerformAreaDamage(float _area)
    {
        areaDamage.radius = Mathf.Lerp(areaDamage.radius, _area, 20 * Time.deltaTime);
        if (Mathf.Abs(areaDamage.radius - _area) <= 0.1f)
        {
            areaDamage.radius = 0f;
            return true;
        }
        else return false;
    }
    void ComboBreakerReset(bool _onlyForNumOfCombo = false)
    {
        runComboBreakerTimer = false;
        comboBreakerResetTimer = comboBreakerGap;
        comboBreakerComboNumCounter = comboBreakerOnNumOfCombo;
        if (!_onlyForNumOfCombo)
            comboBreakerDamageCounter = comboBreakerOnAmountOfDamage;
    }



    public static void SetMutantCombatProp(float _attackRateInSec, float _armorOffTimeGap, float _comboBreakerOnAmountOfDamage, int _comboBreakerOnNumOfCombo)
    {
        attackRateInSec = _attackRateInSec;
        armorOffTimeGap = _armorOffTimeGap;
        comboBreakerOnAmountOfDamage = _comboBreakerOnAmountOfDamage;
        comboBreakerOnNumOfCombo = _comboBreakerOnNumOfCombo;
    }
    #region Animation Event
    void ComboDistanceCheck(float _disSq)
    {
        if (distanceSqToPlayer < _disSq)
            m_anim.SetTrigger("combo");
    }
    // Area Damage Function
    void GroundSlampAreaDamage()
    {
        AreaDamage(.5f, groundSlampRadius);
        //VFX
        Instantiate(groundSlamp, this.transform.position + Vector3.up * 0.05f, Quaternion.LookRotation(this.transform.up));
    }
    void BattleCryAreaDamage()
    {
        armorRecovering = true;
        AreaDamage(.15f, battleStunRadius);
        Instantiate(battleCry, this.transform.position + Vector3.up * 0.05f, Quaternion.LookRotation(this.transform.up));
    }
    void DefenceBlock()
    {
        AreaDamage(0.2f, 1f);
    }
    void AreaDamage(float _damage, float _radius)
    {
        areaDamage.enabled = true;
        areaDamageRadius = groundSlampRadius;
        myImpact = _damage;
        stunAttack = true;
    }
    void BossJumpOn()
    {
        immuneToDamage = true;
        m_Collider.isTrigger = true;        
    }
    void Evade()
    {
        immuneToDamage = true;
    }
    void BossJumpLanding()
    {
        m_Collider.isTrigger = false;
    }
    void BossImmuneOff()
    {
        immuneToDamage = false;
    }
    void PlayFootStep()
    {

        bool play = (MYSTATE == BehavivorState.InCombat && m_anim.GetFloat("forwardInCombat") >= .3f)
            || (MYSTATE != BehavivorState.InCombat && m_anim.GetFloat("speed") >= 1f);
        if (play)
        {
            AudioLibrary.PlayMonsterFootstep(m_audioSources[0]);
        }
    }

    #endregion



    

}
