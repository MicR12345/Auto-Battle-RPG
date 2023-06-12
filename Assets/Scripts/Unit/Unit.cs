using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Globalization;
public class Unit : MonoBehaviour,Selectable,Placeable,StoresData
{
    public TileMap.MapController controller;

    public UnitMovement unitMovement;
    public GameObject selectorObject;

    public List<StoresData> componentSerializableData = new List<StoresData>();

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
        private set { hp = value; }
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
    int damage;
    [SerializeField]
    int range;
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
        controller.map.Occupy(tile);
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
        dataStorage.RegisterNewParam("positionX", transform.position.x.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("positionY", transform.position.y.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("hp", HP.ToString());
        dataStorage.RegisterNewParam("freezeLogic", freezeLogic.ToString());
        foreach (StoresData item in componentSerializableData)
        {
            dataStorage.AddSubcomponent(item.GetData());
        }
        return dataStorage;
    }

    [XmlRoot("Unit")]
    public class UnitSaveData
    {
        public string type;
        public string faction;
        public float positionX;
        public float positionY;
        public int hp;
        public bool freezeLogic;
    }
}
