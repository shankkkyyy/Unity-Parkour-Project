using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour {


    GameManager.Quest quest;
    static GameObject questProgressPrefab;
    string questDescription;
    [SerializeField, ReadOnly]
    bool hasProgressBar;

	// Use this for initialization
	void Start () {

        switch (quest)
        {
            case GameManager.Quest.CollectGems:
                hasProgressBar = true;
                break;
            case GameManager.Quest.DefeatDouglas:
                hasProgressBar = true;
                break;
            case GameManager.Quest.DefeatMutant:
                hasProgressBar = false;
                break;
            case GameManager.Quest.JumptoRelay:
                hasProgressBar = false;
                break;
            default:
                break;
        }
        if (hasProgressBar)
        {
            questProgressPrefab = Resources.Load<GameObject>("Prefabs/QuestProgress");
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
