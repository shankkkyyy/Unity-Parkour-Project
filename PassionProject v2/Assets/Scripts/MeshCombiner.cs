using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour {
    MeshFilter[] filters;
    public void CombineMesh()
    {
        Vector3 position = this.transform.position;
        Quaternion rotation = this.transform.rotation;

        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        filters = this.GetComponentsInChildren<MeshFilter>();
        Debug.Log("Combining " + (filters.Length - 1) + " Meshes");

        Mesh finalMesh = new Mesh();

        CombineInstance[] combineInstances = new CombineInstance[filters.Length - 1];

        for (int i = 1; i < filters.Length; i++)
        {
            int a = i - 1;
            combineInstances[a].subMeshIndex = 0;
            combineInstances[a].mesh = filters[i].sharedMesh;
            combineInstances[a].transform = filters[i].transform.localToWorldMatrix;
            filters[i].transform.gameObject.SetActive(false);
        }

        finalMesh.CombineMeshes(combineInstances);
        this.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        this.transform.position = position;
        this.transform.rotation = rotation;
    }
    public void ResetMesh()
    {
        if (filters == null)
        {
            Debug.Log("Nothing has been combined yet");
            return;
        }
        this.GetComponent<MeshFilter>().sharedMesh = null;
        for (int i = 1; i < filters.Length; i++)
        {
            filters[i].gameObject.SetActive(true);
        }

    }
}
