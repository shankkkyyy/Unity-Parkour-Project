
using UnityEngine;
using UnityEngine.UI;


public class HeroStatus : characterStatus
{
    Combat me;
    [SerializeField, ReadOnly]float stamina_curr;
    [SerializeField]float stamina_total = 1;
    [SerializeField]Image staminaBar;

    // Use this for initialization
    protected override void Start()
    {
        me = GetComponent<Combat>();
        stamina_curr = stamina_total;
        staminaBar.fillAmount = 1;
        base.Start();
    }

    void Update()
    {
        StaminaRecover();
    }

    protected override void OnTriggerEnter(Collider collider)
    {

        if (collider.tag == "E_Hit" && !me.DamageImmuen)
        {
            float damage = me.TakingDamageFromEnemies(collider);
            Hp_curr -= (inDebug) ? 0 : damage;
            me.BeatenToARagdoll(collider, Hp_curr);
            float changeToPlayVocal = (Hp_curr <= 0) ? 1 : damage * 0.03f;
            AudioLibrary.PlayGoodManStruggle(Hitman_BasicMove.Audio[0], Random.Range(0.1f, 0.2f), changeToPlayVocal);
            AudioLibrary.PlayFistFightImpact(Hitman_BasicMove.Audio[1]);

        }
        UpdateHpBar();
    }

    void StaminaRecover()
    {
        stamina_curr += 0.1f * Time.deltaTime;
        stamina_curr = Mathf.Clamp(stamina_curr, 0, 1);
        staminaBar.fillAmount = stamina_curr;
    }
    public void Recover()
    {
        Hp_curr = Hp_total;
        UpdateHpBar();
    }

    public void StaminaChangedBy(float _value)
    {
        stamina_curr += _value;
        staminaBar.fillAmount = stamina_curr;
    }
    public float MyStamina
    {
        get { return stamina_curr; }
    }
    public float MyHealth
    {
        get { return Hp_curr; }
    }
}
