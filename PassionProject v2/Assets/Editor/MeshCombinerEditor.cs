using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshCombiner))]
public class MeshCombinerEditor : Editor {
    public override void OnInspectorGUI()
    {
        MeshCombiner Combiner = (MeshCombiner)target;

        if (GUILayout.Button("Combine Mesh"))
        {
            Combiner.CombineMesh();
        }

        if (GUILayout.Button("Reset Mesh"))
        {
            Combiner.ResetMesh();
        }
    }


}
