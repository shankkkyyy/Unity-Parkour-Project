using UnityEngine.UI;
using UnityEngine;

public abstract class characterStatus : MonoBehaviour {

    [SerializeField][Range (0,1)] protected float Hp_startFriction_temp = 1;

    [SerializeField] protected float Hp_curr;
    [SerializeField] protected float Hp_total = 100;
    [SerializeField] protected Image hpBar;
    [SerializeField] protected bool inDebug = true;


    // Use this for initialization
    protected virtual void Start ()
    {
        Hp_curr = Hp_total * Hp_startFriction_temp;
        hpBar.fillAmount = 1 * Hp_startFriction_temp;
	}
    protected abstract void OnTriggerEnter(Collider collider);

    protected void UpdateHpBar()
    {
        Hp_curr = Mathf.Clamp(Hp_curr, 0, Hp_total);
        hpBar.fillAmount = Hp_curr / Hp_total;
    }

    public float CurrentHp
    {
        get { return Hp_curr; }
    }
    public float StaringHp
    {
        get { return Hp_total; }
    }

}
