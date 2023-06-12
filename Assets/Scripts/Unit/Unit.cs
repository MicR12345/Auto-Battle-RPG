using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
public class Unit : MonoBehaviour,Selectable,Placeable
{
    public TileMap.MapController controller;

    public UnitMovement unitMovement;
    public GameObject selectorObject;

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
