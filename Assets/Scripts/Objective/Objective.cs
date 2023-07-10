using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
public class Objective : MonoBehaviour,Selectable,Placeable,StoresData,Damageable,Targetable,AIControllable
{
    public TileMap.MapController controller;

    public ObjectiveType objectiveType;
    public GameObject selectorObject;
    public Animator animator;

    public ObjectiveGraphics graphics;

    public (int, int) gatherSpot = (-1, -1);
    private int maxHP;

    public List<StoresData> componentSerializableData = new List<StoresData>();
    public DataStorage reconstructionData = null;
    public bool isReconstructed= false;
    public bool freezeLogic = false;

    string graphicsState = "";
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
        set {
            if (hp + value < 0)
            {
                if (faction!="Neutral")
                {
                    
                }
                else
                {
                    faction = "Neutral";
                    freezeLogic = true;
                }
                //TODO : Change to neutral
            }
            hp = value; 
            ResolveGraphics();
        }
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
        dataStorage.RegisterNewParam("maxHP", maxHP.ToString());
        dataStorage.RegisterNewParam("hp", HP.ToString());
        dataStorage.RegisterNewParam("faction", faction);
        dataStorage.RegisterNewParam("graphicsPackage", graphics.objectiveName);
        foreach (StoresData item in componentSerializableData)
        {
            dataStorage.AddSubcomponent(item.GetData());
        }
        return dataStorage;
    }

    bool Selectable.IsDeadInside()
    {
        return false;
    }

    void Damageable.ApplyDamage(int damage)
    {
        HP = HP - damage;
    }

    public string GetFaction()
    {
        return faction;
    }

    Vector3 Targetable.GetShootPosition()
    {
        return transform.position;
    }

    bool Targetable.IsTargedDeadInside()
    {
        return false;
    }
    public void ResolveGraphics()
    {
        string newState = "";
        if (HP > Mathf.FloorToInt(0.75f * MaxHP)) newState = faction;
        //Tu kolejne warunki stanów grafiki

        if (newState!=graphicsState)
        {
            graphicsState = newState;
            ObjectiveSprites sprites = graphics.FindGraphicsState(newState);
            if (sprites != null)
            {
                animator.SetSpriteList(sprites.stateSprite, sprites.animSpeed);
            }
        }
    }

    (string, Targetable) AIControllable.CurrentState()
    {
        throw new System.NotImplementedException();
    }

    void AIControllable.ReciveOrder(string order, (int, int)? target)
    {
        throw new System.NotImplementedException();
    }
}