using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFactory : MonoBehaviour
{
    public TileMap.MapController controller;

    public GameObject TurretPrefab;
    public List<TurretType> turretTypes = new List<TurretType>();
    public TurretType FindTurretData(string type)
    {
        for (int i = 0; i < turretTypes.Count; i++)
        {
            if (type == turretTypes[i].type)
            {
                return turretTypes[i];
            }
        }
        return null;
    }
    public Turret CreatePlaceableTurret(string type, string faction)
    {
        GameObject turretObject = GameObject.Instantiate(TurretPrefab);
        turretObject.name = type;
        turretObject.transform.position = new Vector3(-10, -10);
        Turret turret = turretObject.GetComponent<Turret>();
        TurretType turretType = FindTurretData(type);
        turret.turretType = turretType;
        if (turretType == null)
        {
            GameObject.Destroy(turretObject);
            Debug.LogError("Turret type not found");
            return null;
        }
        turret.MaxHP = turretType.maxHP;
        turret.Faction = faction;
        turret.range = turretType.range;
        turret.bulletPhases = turretType.bulletPhases;
        foreach (string comp in turretType.components)
        {
            turretObject.transform.Find(comp).gameObject.SetActive(true);
        }
        turret.controller = controller;
        turret.freezeLogic = true;
        return turret;
    }
    public Turret ReconstructTurretFromData(DataStorage turretData)
    {
        GameObject turretObject = GameObject.Instantiate(TurretPrefab);
        string turretName = turretData.FindParam("type").value;
        turretObject.name = turretName;
        turretObject.transform.position = new Vector3(-10, -10);
        Turret turret = turretObject.GetComponent<Turret>();
        TurretType turretType = FindTurretData(turretName);
        turret.turretType = turretType;
        if (turretType == null)
        {
            GameObject.Destroy(turretObject);
            Debug.LogError("Turret type not found");
            return null;
        }
        turret.MaxHP = turretType.maxHP;
        turret.range = turretType.range;
        turret.bulletPhases = turretType.bulletPhases;
        turret.Faction = turretData.FindParam("faction").value;
        turret.HP = int.Parse(turretData.FindParam("hp").value);
        foreach (DataStorage comp in turretData.subcomponents)
        {
            turretObject.transform.Find(comp.name).gameObject.SetActive(true);
        }
        foreach (string comp in turretType.components)
        {
            turretObject.transform.Find(comp).gameObject.SetActive(true);
        }
        turret.reconstructionData = turretData;
        turret.isReconstructed = true;
        turret.controller = controller;
        turret.freezeLogic = true;
        return turret;
    }
}
[System.Serializable]
public class TurretType
{
    public string type;
    public int maxHP;
    public int range;
    public List<string> components = new List<string>();
    public List<BulletPhase> bulletPhases = new List<BulletPhase>();
    public List<TurretSprites> sprites = new List<TurretSprites>();
}
[System.Serializable]
public class TurretSprites
{
    public string name;
    public List<Sprite> stateSprite = new List<Sprite>();
    public float animSpeed = 3;
}