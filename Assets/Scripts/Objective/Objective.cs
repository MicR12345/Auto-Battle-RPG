using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Objective : MonoBehaviour,Selectable,Placeable
{
    public TileMap.MapController controller;

    public ObjectiveType objectiveType;
    public GameObject selectorObject;
    public List<Saveable> saveableComps = new List<Saveable>();
    public (int, int) gatherSpot = (-1, -1);
    private int maxHP;

    public bool freezeLogic = false;
    public int MaxHP
    {
        get { return maxHP; }
        set 
        {
            maxHP = value;
            HP = value; 
        }
    }
    private int hp;
    public int HP
    {
        get { return hp; }
        set { hp = value; }
    }
    public string faction;

    void Selectable.OnSelect()
    {
        selectorObject.SetActive(true);
    }

    void Selectable.OnAction(Vector3 screenPosition)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPosition);
        gatherSpot = controller.GetMapTileFromWorldPosition(pos);
    }

    void Selectable.Unselect()
    {
        selectorObject.SetActive(false);
    }

    void Placeable.Place(Vector3 position)
    {
        transform.position = position;
        controller.objectiveFactory.OccupySpaceUnderObjective(controller.GetMapTileFromWorldPosition(position));
        controller.RegisterObjective(this);
        freezeLogic = false;
    }

    void Placeable.Discard()
    {
        GameObject.Destroy(this);
    }
}