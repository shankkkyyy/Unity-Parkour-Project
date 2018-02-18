using UnityEditor;
using UnityEngine;


[CustomEditor (typeof(MeshOption))]


public class MeshOptionEditor : Editor {

    MeshOption mo;
    private void OnSceneGUI()
    {
        mo = target as MeshOption;

        if (Handles.Button(mo.transform.position - mo.transform.forward * mo.size,  mo.transform.rotation, mo.size * 2f, mo.size * 1.7f, Handles.CylinderCap))
        {
            mo.NextOption();
        }
    }

}
