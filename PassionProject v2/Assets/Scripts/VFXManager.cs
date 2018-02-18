using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class VFXManager
{ 
    [SerializeField]
    static ParticleSystem groundSlamp;


    public static ParticleSystem GroundSlamp
    {
        get { return groundSlamp; }
        
    }


}
