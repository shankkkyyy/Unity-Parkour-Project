using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    static GameManager GM;
    [SerializeField] Enemy clone;
    [SerializeField] Transform[] spawnPoint;
    [SerializeField] PickUp[] relays;
    List<Enemy> children;


    [SerializeField]
    float spawnTimeGap = 1f;
    [SerializeField, ReadOnly]
    float spawnTimer;

    [SerializeField]
    int size = 4, maxCapacity = 20;
    [SerializeField, ReadOnly]
    int deadedClone, producedClone;



    [SerializeField, ReadOnly]
    bool isSpawning, spawningFinished, spawningOnPause;

	// Use this for initialization
	void Start () {

        if (GM == null)
            GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        children = new List<Enemy>();
        Initialization();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!isSpawning)
            return;

        // before the spawn made enough clone
        if (!spawningFinished && !spawningOnPause)
        {
            if (Time.time - spawnTimer >= spawnTimeGap)
            {
                int bornPoints = Random.Range(0, spawnPoint.Length);
                Enemy child = Instantiate(clone, spawnPoint[bornPoints].position, Quaternion.identity, null);
                children.Add(child);
                child.MySpawner = this;
                producedClone++;
                spawnTimer = Time.time;
                // If the spawner producer enough clone, then stop spawn
                if (producedClone >= size)
                    spawningFinished = true;
            }
        }

        // if there are enough clone alive, set it to on - pause
        if (producedClone - deadedClone >= maxCapacity)
            spawningOnPause = true;
        else
        {
            if (spawningOnPause)
                spawnTimer = Time.time;
            spawningOnPause = false;
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnTimer = Time.time;
        }
    }

    public void OnCloneDead(Enemy _this)
    {
        deadedClone++;
        children.Remove(_this);
        if (deadedClone >= size)
        {
            isSpawning = false;
            GM.UpdateCurrentQuest(GameManager.Quest.JumptoRelay);
            OpenRelay(relays[0]);
            OpenRelay(relays[1]);
        }
    }
    public void Initialization()
    {
        isSpawning = false;
        spawningFinished = false;
        spawningOnPause = false;
        deadedClone = 0;
        producedClone = 0;
        relays[0].gameObject. SetActive(false);
        relays[1].gameObject. SetActive(false);
    }
    public void CleanAllChildren()
    {
        for (int i = 0; i < children.Count; i++)
        {
            Destroy(children[i].gameObject);
        }
        children.Clear();
    }

    void OpenRelay(PickUp _relay)
    {
        switch (_relay.MyProp)
        {
            case PickUp.ItemProp.relayToMutant:
                if (GM.MCPAccessable)
                    _relay.gameObject.SetActive(true);
                break;
            case PickUp.ItemProp.relayToDouglas:
                if (GM.DCPAccessable)
                    _relay.gameObject.SetActive(true);
                break;
            case PickUp.ItemProp.relayToEnergyYard:
                    _relay.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}
