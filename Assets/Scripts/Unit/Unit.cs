using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public TileMap.MapController controller;

    public UnitMovement unitMovement;

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
    [SerializeField]
    public int speed;
    [SerializeField]
    int damage;
    [SerializeField]
    int range;
    public UnitType unitType;


}
