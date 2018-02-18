using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour {


    Enemy owner;
	// Use this for initialization
	void Start () {
        owner = null;
	}

    public Enemy Owner
    {
        get { return owner; }
        set { owner = value; }
    }
	
}
