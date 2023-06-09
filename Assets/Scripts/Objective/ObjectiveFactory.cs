using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveFactory : MonoBehaviour
{
    public TileMap.MapController controller;

    public GameObject ObjectivePrefab;

    public List<ObjectiveType> objectiveTypes = new List<ObjectiveType>();
    public Objective CreateObjective(string type, Vector3 position,string faction)
    {
        GameObject objectiveObject = GameObject.Instantiate(ObjectivePrefab);
        objectiveObject.transform.position = position;
        objectiveObject.name = type;
        Objective objective = objectiveObject.GetComponent<Objective>();
        ObjectiveType objectiveType = FindObjectiveData(type);
        objective.objectiveType = objectiveType;
        if (objectiveType==null)
        {
            GameObject.Destroy(objectiveObject);
            Debug.LogError("Objective type not found");
            return null;
        }
        objective.MaxHP = objectiveType.maxHP;
        objective.faction = faction;
        foreach (ComponentWithParams comp in objectiveType.components)
        {
            objectiveObject.transform.Find(comp.name).gameObject.SetActive(true);
        }
        objective.controller = controller;
        return objective;
    }
    public ObjectiveType FindObjectiveData(string type)
    {
        for (int i = 0; i < objectiveTypes.Count; i++)
        {
            if (type == objectiveTypes[i].type)
            {
                return objectiveTypes[i];
            }
        }
        return null;
    }

}
[System.Serializable]
public class ObjectiveType
{
    public string type;
    public int maxHP;
    public List<ComponentWithParams> components = new List<ComponentWithParams>();
    public List<ObjectiveSprites> sprites = new List<ObjectiveSprites>();
    public ComponentWithParams FindParam(string name)
    {
        foreach (ComponentWithParams component in components)
        {
            if (name == component.name)
            {
                return component;
            }
        }
        return null;
    }
}
[System.Serializable]
public class ObjectiveSprites
{
    public string name;
    public List<Sprite> stateSprite = new List<Sprite>();
    public float animSpeed = 3;
}
[System.Serializable]
public class ComponentWithParams
{
    public string name;
    public List<ComponentWithParams> componentsWithParams = new List<ComponentWithParams>();
    public ComponentWithParams FindParam(string name)
    {
        foreach (ComponentWithParams component in componentsWithParams)
        {
            if (name == component.name)
            {
                return component;
            }
        }
        return null;
    }
}