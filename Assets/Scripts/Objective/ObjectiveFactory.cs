using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
public class ObjectiveFactory : MonoBehaviour
{
    public TileMap.MapController controller;

    public GameObject ObjectivePrefab;

    public List<ObjectiveType> objectiveTypes = new List<ObjectiveType>();
    public List<ObjectiveGraphics> objectiveGraphics = new List<ObjectiveGraphics>();
    public List<Component> attachableObjectiveComponents= new List<Component>();
    public void Start()
    {
        foreach (Transform child in ObjectivePrefab.transform)
        {
            Component comp;
            if (child.TryGetComponent<Component>(out comp))
            {
                if (!comp.isStatic())
                {
                    attachableObjectiveComponents.Add(comp);
                }
            }
        }
    }
    public List<TMPro.TMP_Dropdown.OptionData> PopulateObjectiveSpriteDropdown()
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new List<TMPro.TMP_Dropdown.OptionData>();
        foreach (ObjectiveGraphics graphics in objectiveGraphics)
        {
            TMPro.TMP_Dropdown.OptionData optionData = new TMPro.TMP_Dropdown.OptionData();
            optionData.text = graphics.objectiveName;
            optionData.image = graphics.objectiveSprites[0].stateSprite[0];
            options.Add(optionData);
        }
        return options;
    }
    public List<Component> FetchComponents()
    {
        return attachableObjectiveComponents;
    }
    public Objective DebugCreateObjective(string type, Vector3 position,string faction)
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
        OccupySpaceUnderObjective(controller.GetMapTileFromWorldPosition(position));
        objective.controller = controller;
        objective.freezeLogic = false;
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
    public void OccupySpaceUnderObjective((int,int) tile)
    {
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                controller.map.OccupyStatic((tile.Item1 + i, tile.Item2 + j));
            }
        }
    }
    public Objective CreatePlaceableObjective(string type, string faction)
    {
        GameObject objectiveObject = GameObject.Instantiate(ObjectivePrefab);
        objectiveObject.transform.position = new Vector3(-10,-10);
        objectiveObject.name = type;
        Objective objective = objectiveObject.GetComponent<Objective>();
        ObjectiveType objectiveType = FindObjectiveData(type);
        if (objectiveType == null)
        {
            GameObject.Destroy(objectiveObject);
            Debug.LogError("Objective type not found");
            return null;
        }
        objective.objectiveType = objectiveType;
        objective.MaxHP = objectiveType.maxHP;
        objective.faction = faction;
        foreach (ComponentWithParams comp in objectiveType.components)
        {
            objectiveObject.transform.Find(comp.name).gameObject.SetActive(true);
        }
        objective.controller = controller;
        objective.freezeLogic = true;
        return objective;
    }
    public Objective ReconstructObjectiveFromData(DataStorage objectiveData)
    {
        GameObject objectiveObject = GameObject.Instantiate(ObjectivePrefab);
        objectiveObject.transform.position = new Vector3(-10, -10);
        string objectiveName = objectiveData.FindParam("name").value;
        objectiveObject.name = objectiveName;
        Objective objective = objectiveObject.GetComponent<Objective>();
        objective.isReconstructed = true;
        objective.reconstructionData = objectiveData;
        objective.freezeLogic = true;
        //ObjectiveType objectiveType = FindObjectiveData(objectiveName);
        //if (objectiveType == null)
        //{
        //    GameObject.Destroy(objectiveObject);
        //    Debug.LogError("Objective type not found");
        //    return null;
        //}
        //objective.objectiveType = objectiveType;
        objective.MaxHP = int.Parse(objectiveData.FindParam("maxHP").value);
        objective.HP = int.Parse(objectiveData.FindParam("hp").value);
        objective.gatherSpot =
            (
            int.Parse(objectiveData.FindParam("gatherSpotX").value),
            int.Parse(objectiveData.FindParam("gatherSpotY").value)
            );
        objective.faction = objectiveData.FindParam("faction").value;
        foreach (DataStorage comp in objectiveData.subcomponents)
        {
            objectiveObject.transform.Find(comp.name).gameObject.SetActive(true);
        }
        objective.controller = controller;
        return objective;
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
public class ObjectiveGraphics
{
    public string objectiveName;
    public List<ObjectiveSprites> objectiveSprites = new List<ObjectiveSprites>();
}
[System.Serializable,XmlRoot("Component")]
public class ComponentWithParams
{
    public string name;
    [XmlArray("Subcomponents"),XmlArrayItem("Component")]
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
    public ComponentWithParams(string name)
    {
        this.name = name;
        componentsWithParams = null;
    }
    public ComponentWithParams()
    {
        this.name = "";
        componentsWithParams = null;
    }
}