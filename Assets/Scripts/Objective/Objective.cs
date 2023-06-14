using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
public class Objective : MonoBehaviour,Selectable,Placeable,StoresData
{
    public TileMap.MapController controller;

    public ObjectiveType objectiveType;
    public GameObject selectorObject;
    public (int, int) gatherSpot = (-1, -1);
    private int maxHP;

    public List<StoresData> componentSerializableData = new List<StoresData>();
    public DataStorage reconstructionData = null;
    public bool isReconstructed= false;
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
        GameObject.Destroy(gameObject);
    }

    DataStorage StoresData.GetData()
    {
        return GenerateData();
    }
    DataStorage GenerateData()
    {
        (int, int) postion = controller.GetMapTileFromWorldPosition(transform.position);
        DataStorage dataStorage = new DataStorage("Objective");
        dataStorage.RegisterNewParam("name", objectiveType.type);
        dataStorage.RegisterNewParam("x", postion.Item1.ToString());
        dataStorage.RegisterNewParam("y", postion.Item2.ToString());
        dataStorage.RegisterNewParam("gatherSpotX", gatherSpot.Item1.ToString());
        dataStorage.RegisterNewParam("gatherSpotY", gatherSpot.Item2.ToString());
        dataStorage.RegisterNewParam("hp", HP.ToString());
        dataStorage.RegisterNewParam("faction", faction);
        foreach (StoresData item in componentSerializableData)
        {
            dataStorage.AddSubcomponent(item.GetData());
        }
        return dataStorage;
    }
}