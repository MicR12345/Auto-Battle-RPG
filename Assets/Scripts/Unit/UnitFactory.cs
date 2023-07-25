using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFactory : MonoBehaviour
{
    public TileMap.MapController controller;

    public GameObject UnitPrefab;
    public List<UnitType> unitTypes = new List<UnitType>();
    public UnitType FindUnitData(string type)
    {
        for (int i = 0; i < unitTypes.Count; i++)
        {
            if (type == unitTypes[i].type)
            {
                return unitTypes[i];
            }
        }
        return null;
    }
    public Unit CreatePlaceableUnit(string type, string faction)
    {
        GameObject unitObject = GameObject.Instantiate(UnitPrefab);
        unitObject.name = type;
        unitObject.transform.position = new Vector3(-10, -10);
        Unit unit = unitObject.GetComponent<Unit>();
        UnitType unitType = FindUnitData(type);
        unit.unitType = unitType;
        if (unitType == null)
        {
            GameObject.Destroy(unitObject);
            Debug.LogError("Objective type not found");
            return null;
        }
        unit.MaxHP = unitType.maxHP;
        unit.speed = unitType.speed;
        unit.Faction = faction;
        unit.range = unitType.range;
        unit.capturePower = unitType.capturePower;
        unit.bulletPhases = unitType.bulletPhases;
        foreach (string comp in unitType.components)
        {
            unitObject.transform.Find(comp).gameObject.SetActive(true);
        }
        unit.controller = controller;
        unit.unitMovement.gameObject.SetActive(true);
        unit.freezeLogic = true;
        return unit;
    }
    public Unit ReconstructUnitFromData(DataStorage unitData)
    {
        GameObject unitObject = GameObject.Instantiate(UnitPrefab);
        string unitName = unitData.FindParam("type").value;
        unitObject.name = unitName;
        unitObject.transform.position = new Vector3(-10, -10);
        Unit unit = unitObject.GetComponent<Unit>();
        UnitType unitType = FindUnitData(unitName);
        unit.unitType = unitType;
        if (unitType == null)
        {
            GameObject.Destroy(unitObject);
            Debug.LogError("Objective type not found");
            return null;
        }
        unit.MaxHP = unitType.maxHP;
        unit.speed = unitType.speed;
        unit.range = unitType.range;
        unit.capturePower = unitType.capturePower;
        unit.bulletPhases = unitType.bulletPhases;
        unit.Faction = unitData.FindParam("faction").value;
        unit.HP = int.Parse(unitData.FindParam("hp").value);
        foreach (DataStorage comp in unitData.subcomponents)
        {
            unitObject.transform.Find(comp.name).gameObject.SetActive(true);
        }
        foreach (string comp in unitType.components)
        {
            unitObject.transform.Find(comp).gameObject.SetActive(true);
        }
        unit.reconstructionData = unitData;
        unit.isReconstructed = true;
        unit.controller = controller;
        unit.unitMovement.gameObject.SetActive(true);
        unit.freezeLogic = true;
        return unit;
    }
}
[System.Serializable]
public class UnitType
{
    public string type;
    public string description;
    public int maxHP;
    public int speed;
    public int capturePower;
    public int range;
    public List<string> components = new List<string>();
    public List<BulletPhase> bulletPhases = new List<BulletPhase>();
    public List<UnitSprites> sprites = new List<UnitSprites>();
}
[System.Serializable]
public class UnitSprites
{
    public string name;
    public List<Sprite> stateSprite = new List<Sprite>();
    public float animSpeed = 3;
}
