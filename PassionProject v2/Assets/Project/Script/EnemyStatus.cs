using UnityEngine.UI;
using UnityEngine;
using System;

public class EnemyStatus : characterStatus{

    [SerializeField, ReadOnly]
    bool UIisOn;
    [SerializeField, ReadOnly]
    float armor;
    Enemy me;
    Transform cam;
    [SerializeField] Image hpBackground, armorBar;
    [SerializeField] Image attackIcon;

    // Use this for initialization
    protected override void Start()
    {
        me = GetComponent<Enemy>();
        cam = Camera.main.transform;
        IniUI();
        base.Start();
    }
    private void Update()
    {      
        hpBackground.transform.LookAt(cam.transform.position);
    }
    protected override void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Hero")
        {
            float damage = me.TakingHitFromHero(collider);
            damage = (inDebug) ? 0 : damage;

            if (armorBar.fillAmount > 0)
            {
                armor -= damage;
            }
            else
            {
                Hp_curr -= damage;
                me.BeatenToARagdoll(collider, Hp_curr);
            }
        }


        UpdateArmorBar();
        UpdateHpBar();

    }

    // UI Stuff
    public void UIUpdate(bool _showUI, bool _beingLock)
    {
        if (_showUI)
        {
            if (!UIisOn)
            {
                UIisOn = true;
                TurnOnFloatingBars();
                TurnAttackIcon(false);
            }
            if (_beingLock)
                BeingLock();
            else
                Unlock();

            //Debug.Log("_beingLock" + _beingLock);
        }
        else
        {
            if (UIisOn)
                IniUI();
        }
    }
    public void TurnAttackIcon(bool _enable)
    {
        attackIcon.enabled = _enable;
    }
    void IniUI()
    {
        Unlock();
        TurnOffFloatingBars();
        TurnAttackIcon(false);
        UIisOn = false;
    }
    void BeingLock()
    {
        hpBar.color = Color.yellow;
    }
    void Unlock()
    {
        hpBar.color = Color.red;
    }
    void TurnOnFloatingBars()
    {
        hpBackground.enabled = true;
        hpBar.enabled = true;
        armorBar.enabled = true;
    }
    void TurnOffFloatingBars()
    {
        hpBackground.enabled = false;
        hpBar.enabled = false;
        armorBar.enabled = false;
    }
    void UpdateArmorBar()
    {
        armor = Mathf.Clamp(armor, 0, 100);
        armorBar.fillAmount = armor * 0.01f;
    }


    public float ArmorValue
    {
        get { return armor; }
        set {
            armor = value;
            UpdateArmorBar();
        }
    }

    public bool ArmorFull
    {
        get { return armorBar.fillAmount == 1; }
    }

}
