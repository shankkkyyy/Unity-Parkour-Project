using System.Collections;
using UnityEngine;

public class MeshOption : MonoBehaviour {

    
    public float size = 0.25f;
    [SerializeField]
    int currentOption = -1;
    [SerializeField]
    GameObject[] options;
    [SerializeField]
    GameObject instance;

    private void OnDrawGizmos()
    {
        // draw effected item;
        Gizmos.color = new Color(2, 2, 0.75f, 1);
        Gizmos.DrawSphere(transform.position, size);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, size * 1.1f);
    }

    public void NextOption()
    {
        if (instance != null)
        {
            DestroyImmediate(instance);
            instance = null;
        }

        currentOption++;
        if (currentOption >= options.Length)
        {
            currentOption = -1;
        }
        else
        {
            instance = Instantiate( options[currentOption]);
            instance.transform.SetParent(transform, false);
            instance.transform.position = transform.position;
        }
    }
}
