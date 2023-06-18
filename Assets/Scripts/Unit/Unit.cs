using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Globalization;
using PathfindMap;

public class Unit : MonoBehaviour,Selectable,Placeable,StoresData,Damageable,Targetable
{
    public TileMap.MapController controller;

    public UnitMovement unitMovement;
    public GameObject selectorObject;

    public List<StoresData> componentSerializableData = new List<StoresData>();
    public DataStorage reconstructionData = null;
    public bool isReconstructed = false;

    public bool isDead = false;
    [SerializeField]
    private int maxHP;
    public int MaxHP
    {
        get { return maxHP; }
        set
        {
            maxHP = value;
            HP = value;
        }
    }
    int hp;
    public int HP
    {
        get { return hp; }
        set { 
            if (hp + value < 0)
            {
                DestoryThis();
            }
            hp = value; 
        }
    }
    string faction;
    public string Faction
    {
        get { return faction; }
        set { faction = value; }
    }
    [SerializeField]
    public int speed;
    [SerializeField]
    public int damage;
    [SerializeField]
    public int range = 20;
    public UnitType unitType;

    public bool freezeLogic = false;

    void Selectable.OnSelect()
    {
        selectorObject.SetActive(true);
    }

    void Selectable.OnAction(Vector3 screenPosition)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPosition);
        unitMovement.BeginPathfind(controller.GetMapTileFromWorldPosition(pos));
    }
    void Selectable.Unselect()
    {
        selectorObject.SetActive(false);
    }

    void Placeable.Place(Vector3 position)
    {

        (int,int) tile = controller.GetMapTileFromWorldPosition(position);
        unitMovement.currentTile = tile;
        transform.position = position;
        controller.RegisterUnit(this);
        controller.map.Occupy(tile,unitMovement);
        freezeLogic = false;
    }

    void Placeable.Discard()
    {
        GameObject.Destroy(gameObject);
    }

    DataStorage StoresData.GetData()
    {
        return GenerateData();
    }

    DataStorage GenerateData()
    {
        DataStorage dataStorage = new DataStorage("Unit");
        dataStorage.RegisterNewParam("type", unitType.type);
        dataStorage.RegisterNewParam("faction", faction);
        dataStorage.RegisterNewParam("x", transform.position.x.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("y", transform.position.y.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("hp", HP.ToString());
        foreach (StoresData item in componentSerializableData)
        {
            dataStorage.AddSubcomponent(item.GetData());
        }
        return dataStorage;
    }

    void Damageable.ApplyDamage(int damage)
    {
        HP = HP - damage;
    }

    Vector3 Targetable.GetShootPosition()
    {
        return transform.position;
    }

    string Targetable.GetFaction()
    {
        return faction;
    }

    string Damageable.GetFaction()
    {
        return faction;
    }

    bool Targetable.IsTargedDeadInside()
    {
        return isDead;
    }
    public void DestoryThis()
    {
        controller.UnregisterUnit(this);
        isDead = true;
        GameObject.Destroy(this.gameObject);
    }

    bool Selectable.IsDeadInside()
    {
        return isDead;
    }
}
