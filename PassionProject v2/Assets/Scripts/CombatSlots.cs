using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSlots : MonoBehaviour {

    [Header("Fight")]


    [SerializeField, ReadOnly]
    bool TargetLockOn;
    [SerializeField]
    float combatRadius = 8f;
    static float timeBetweenAttack = 3f;
    [SerializeField, ReadOnly]
    float attackTimer;
    [SerializeField, ReadOnly]
    int attackEnemy = -1, lastAttackEnemy = -1;

    [Header("Slot Info")]
    [SerializeField]               int maxEnemies = 4;
    Slot[] Slots;
                                  
    LinkedList<Enemy>     enemies;

    LinkedListNode<Enemy> theFirst, targetNode;
    FreeCam               m_FreeCam;
    Vector3               targetDir;
    Hitman_BasicMove      m_basicMove;
    Combat                m_combat;
    Enemy                 m_target;
    SphereCollider        m_sphere;



    // Use this for initialization
    void Start()
    {
        TargetLockOn = false;
        attackTimer = timeBetweenAttack;
        enemies = new LinkedList<Enemy>();
        Slots = GetComponentsInChildren<Slot>();
        m_FreeCam = Camera.main.GetComponentInParent<FreeCam>();

        m_basicMove = GetComponentInParent<Hitman_BasicMove>();
        m_combat = GetComponentInParent<Combat>();
        m_sphere = GetComponent<SphereCollider>();

        m_target = null;
        m_sphere.radius = combatRadius;
        switch (GameManager.GameDifficulty)
        {
            case GameManager.GameMode.relaxed:
                CommonEnemiesAttackFreq = 3f;
                break;
            case GameManager.GameMode.expert:
                CommonEnemiesAttackFreq = 2f;
                break;
            case GameManager.GameMode.insane:
                CommonEnemiesAttackFreq = 1f;
                break;
        }
    }

    //Update is called once per frame
    void Update()
    {
        //Debug.Log(enemies.Count);

        #region 1. Given the player's action, adjust the combat slot size
        if (Input.GetButton("Sprint"))
        {
            m_sphere.radius = 3;
            this.transform.localPosition = new Vector3(0, 0, 1.5f);
        }
        else
        {
            this.transform.localPosition = Vector3.zero;
            m_sphere.radius = Mathf.Lerp(m_sphere.radius, combatRadius, 5 * Time.deltaTime);
        }
        // slots will not rotate as player rotate
        transform.rotation = Quaternion.identity;
        #endregion
        #region 2. update default target Direction
        // no enemy
        if (enemies.Count <= 0)
        {
            m_target = null;
            targetNode = null;
            TargetLockOn = false;
            if (theFirst != null)
                theFirst.Value = null;
            targetDir = m_basicMove.MoveDirWorld;
            return;
        }
        #endregion
        UpdateEnemies();
    }

    void UpdateEnemies()
    {

        if (Input.GetButtonDown("Target Lock"))
        {
            if (!TargetLockOn)
                TargetLockOn = true;
            else
                TargetLockOn = false;
        }


        #region 1. which enemy is going to attack enemy attack
        //  which enemy is going to attack enemy attack
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = timeBetweenAttack;
            attackEnemy = Random.Range(0, enemies.Count);
            if (lastAttackEnemy == attackEnemy)
                attackEnemy = (attackEnemy < enemies.Count - 1) ? attackEnemy + 1 : 0;
            lastAttackEnemy = attackEnemy;
            //Debug.Log(lastAttackEnemy);
        }
        else
        {
            attackEnemy = -1;
        }
        #endregion

        // iteration setup for updating Position and Action
        int i = 0;
        float dir_z = 0;
        theFirst = enemies.First;
        #region 2. iteration body to find next target

        while (theFirst != null)
        {
            bool nothingIsRemoved = true;
            if (theFirst.Value.IsAlive && theFirst.Value.MYSTATE != Enemy.BehavivorState.KnockDown)
            {

                theFirst.Value.ShowUI = false;

                if (!TargetLockOn)
                {

                    #region 2. Pick the enemey, when WASD is pressed

                    if (enemies.Count == 1)
                        targetNode = theFirst;
                    else
                    {
                        if (m_basicMove.MoveDirWorld.sqrMagnitude >= .1f)
                        {
                            float dot;
                            Vector3 enemyDir = theFirst.Value.transform.position - this.transform.position;
                            enemyDir.y = 0;
                            dot = m_basicMove.MoveDirWorld.x * enemyDir.normalized.x + m_basicMove.MoveDirWorld.z * enemyDir.normalized.z;

                            if (i == 0)
                            {
                                targetNode = theFirst;
                                dir_z = dot;
                            }
                            else
                            {
                                if (dot > dir_z)
                                {
                                    targetNode = theFirst;
                                    dir_z = dot;
                                }
                            }
                        }
                    }

                    if (targetNode != null && targetNode.Value.BeingLocked)
                        targetNode.Value.BeingLocked = false;
                    #endregion
                }
                else
                {
                    targetNode.Value.BeingLocked = true;
                    //Debug.Log("lock");
                }

                #region 3. Send Attack Order for common enemy
                if (theFirst.Value.MyClass != Enemy.EnemyClass.Mutant)
                {
                    if (i == attackEnemy)
                        theFirst.Value.Attack = true;
                    else
                        theFirst.Value.Attack = false;
                }
                #endregion
            }
            else
            {
                enemies.Remove(theFirst);
                ReleaseSlot(theFirst.Value);
                targetNode = null;
                //Debug.Log("remove");
            }
            if (nothingIsRemoved)
            {
                ++i;
                theFirst = theFirst.Next;
                // Remove a node, the next node will take place, and we still need to update the one that replace the one being removed
            }
        }
        #endregion

        #region pick the enemy using mouse scrollwheel while the using target lock on mode
        if (TargetLockOn)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                targetNode = (targetNode.Next == null) ? enemies.First : targetNode.Next;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                targetNode = (targetNode.Previous == null) ? enemies.Last : targetNode.Previous;
        }
        #endregion 

        if (targetNode != null && targetNode.Value)
        {
            targetNode.Value.ShowUI = true;
            if (m_combat.AttackButtonHold || m_target == null || TargetLockOn)
                m_target = targetNode.Value;
            targetDir = m_target.transform.position - this.transform.position;
            //targetDir = targetNode.Value.transform.position - this.transform.position;
        }
        else
        {
            targetNode = enemies.First;
        }

    }
    #region public interface
    public int GetTheProperSlot(Vector3 _pos)
    {

        Vector3 dir = _pos - transform.position;
        dir.y = 0;
        dir = transform.InverseTransformDirection(dir).normalized;
        int theNearestSlot;


        if (dir.z > 0.71f)
            theNearestSlot = 0; // slot 1
        else if (dir.z < -0.71f)
            theNearestSlot = 2; // slot 3
        else
        {
            if (dir.x > 0.71f)
                theNearestSlot = 1; // slot 2
            else
                theNearestSlot = 3; // slot 4
        }
        // check slot valiability
        if (Slots[theNearestSlot].Owner == null)
            return theNearestSlot;
        else if (Slots[(theNearestSlot + 1) >= maxEnemies ? 0 : (theNearestSlot + 1)].Owner == null)
            return (theNearestSlot + 1) >= maxEnemies ? 0 : (theNearestSlot + 1);
        else if (Slots[(theNearestSlot - 1) < 0 ? maxEnemies - 1 : (theNearestSlot - 1)].Owner == null)
            return (theNearestSlot - 1) < 0 ? maxEnemies - 1 : (theNearestSlot - 1);
        else
            return (theNearestSlot + 2) >= maxEnemies ? theNearestSlot - 2 : theNearestSlot + 2;
    }
    public void RemoveEnemyFromCombatSlot(Enemy _enemy)
    {
        enemies.Remove(_enemy);
        ReleaseSlot(_enemy);
    }
    public Vector3  GetAngularDisFromSlot(Enemy _enemy, int _myslot)
    {
        Vector3 slotDir;
        Vector3 enemyToPlayerDir = _enemy.transform.position - m_combat.transform.position;
        enemyToPlayerDir.y = 0;
        switch (_myslot)
        {
            case 0:
                slotDir = new Vector3(0, 0, 1);
                break;
            case 1:
                slotDir = new Vector3(1, 0, 0);
                break;
            case 2:
                slotDir = new Vector3(0, 0, -1);
                break;
            default:
                slotDir = new Vector3(-1, 0, 0);
                break;          
        }

        //Debug.Log(Quaternion.FromToRotation(enemyToPlayerDir, slotDir));
        //Quaternion.
        Vector3 slotDirEnemyBody = _enemy.transform.InverseTransformDirection(slotDir);
        slotDirEnemyBody.y = 0;
        slotDirEnemyBody = slotDirEnemyBody.normalized;
        return slotDirEnemyBody;
        //return new Vector2( _enemy.transform.InverseTransformDirection(slotDir).x, _enemy.transform.InverseTransformDirection(slotDir).z);
    }
    public void ReleaseSlot(Enemy _enemy)
    {
        if (_enemy.InSlot == -1)
            return;
        Slots[_enemy.InSlot].Owner = null;
        _enemy.InSlot = -1;
    }
    public void PutEnemyInSlot(Enemy _enemy, int _slotIndex)
    {
        Slots[_slotIndex].Owner = _enemy;
        _enemy.InSlot = _slotIndex;
    }
    public void AddEnemyInCombatSystem(Enemy _enemy)
    {
        enemies.AddFirst(_enemy);
        int enterSlot = this.GetTheProperSlot(_enemy.transform.position);
        PutEnemyInSlot(_enemy, enterSlot);
        //_enemy.InSlot = enterSlot;
        //Slots[enterSlot].Owner = _enemy;
    }
    #endregion

    #region Properties
    public bool FULL
    {
        get { return (enemies.Count >= maxEnemies); }
    }
    public Vector3 TargetDir
    {
        get { return targetDir; }
    }
    public int Count
    {
        get { return enemies.Count; }
    }
    public Enemy TheTarget
    {
        get { return m_target; }
    }
    public Enemy TheNextTarget
    {
        get
        {
            if (targetNode != null)
                return targetNode.Value;
            else return null;
        }
    }
    public float Radius
    {
        get { return m_sphere.radius; }
    }
    public bool LockOn
    {
        get { return TargetLockOn; }
    }
    public void ClearSlot()
    {
        enemies.Clear();
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].Owner = null;
        }
        
    }

    public static float CommonEnemiesAttackFreq
    {
        set { timeBetweenAttack = value; }
    }
    #endregion
}
