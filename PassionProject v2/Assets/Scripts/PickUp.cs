using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour {

    public enum ItemProp
    {
        relayToMutant,
        relayToDouglas,
        relayToEnergyYard,
        eneryDrop,
    }

    [SerializeField]
    ItemProp itemProp;
    static GameManager GM;
    private void Start()
    {

        if (GM == null)
            GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Renderer render = GetComponent<Renderer>();

        switch (itemProp)
        {
            case ItemProp.relayToMutant:
                render.material.color = Color.red;
                break;
            case ItemProp.relayToDouglas:
                render.material.color = Color.blue;
                break;
            case ItemProp.relayToEnergyYard:
                render.material.color = Color.yellow;
                break;
            case ItemProp.eneryDrop:
                render.material.color = Color.green;
                transform.localScale = new Vector3(.2f, .5f, .5f);
                break;
            default:
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GetPickUp();
    }

    void GetPickUp()
    {
        switch (itemProp)
        {
            case ItemProp.relayToEnergyYard:
                StartCoroutine(GM.TeleportPlayerTo(GameManager.GameLocation.energyYard));
                break;
            case ItemProp.relayToMutant:
                StartCoroutine(GM.TeleportPlayerTo(GameManager.GameLocation.mutantYard));
                break;
            case ItemProp.relayToDouglas:
                StartCoroutine(GM.TeleportPlayerTo(GameManager.GameLocation.douglasYard));
                break;
            case ItemProp.eneryDrop:
                GM.CollectEnergyDrop();
                Destroy(this.gameObject);
                break;
            default:
                break;
        }
    }

    public ItemProp MyProp
    {
        set { itemProp = value; }
        get { return itemProp; }
    }




}
