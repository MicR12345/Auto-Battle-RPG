using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using PathfindMap;

public class Turret : MonoBehaviour,Targetable,Placeable,Damageable,StoresData
{
    public TileMap.MapController controller;

    public Animator animator;

    public List<StoresData> componentSerializableData = new List<StoresData>();
    public DataStorage reconstructionData = null;
    public bool isReconstructed = false;

    public Targetable currentTarget;

    (int, int) tile;

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
        set
        {
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
    public int range = 20;
    [SerializeField]
    public List<BulletPhase> bulletPhases = new List<BulletPhase>();
    public TurretType turretType;

    public bool freezeLogic = false;

    void Placeable.Place(Vector3 position)
    {

        this.tile = controller.GetMapTileFromWorldPosition(position);
        transform.position = position;
        controller.RegisterTurret(this);
        controller.map.OccupyStatic(tile);
        freezeLogic = false;
    }

    void Placeable.Discard()
    {
        GameObject.Destroy(gameObject);
    }

    public DataStorage GetData()
    {
        return GenerateData();
    }

    DataStorage GenerateData()
    {
        DataStorage dataStorage = new DataStorage("Turret");
        dataStorage.RegisterNewParam("type", turretType.type);
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

    public string GetFaction()
    {
        return faction;
    }

    bool Targetable.IsTargedDeadInside()
    {
        return isDead;
    }
    public void DestoryThis()
    {
        controller.map.UnoccupyStatic(tile);
        isDead = true;
        controller.UnregisterTurret(this);
        GameObject.Destroy(this.gameObject);
    }

    int Targetable.GetTargetPriority()
    {
        return 15;
    }
}
