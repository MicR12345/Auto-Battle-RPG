using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Objective : MonoBehaviour,Selectable,Placeable
{
    public TileMap.MapController controller;

    public ObjectiveType objectiveType;
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
    private int hp;
    public int HP
    {
        get { return hp; }
        set { hp = value; }
    }
    public string faction;

    void Selectable.OnSelect()
    {
        throw new System.NotImplementedException();
    }

    void Selectable.OnAction(Vector3 screenPostion)
    {
        throw new System.NotImplementedException();
    }

    void Selectable.Unselect()
    {
        throw new System.NotImplementedException();
    }

    void Placeable.Place(Vector3 screenPostion)
    {
        throw new System.NotImplementedException();
    }
}