using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
public class ProduceUnits : MonoBehaviour,StoresData,Component
{
    Objective objective;
    [SerializeField]
    List<ProductionSlot> slots = new List<ProductionSlot>();

    Vector3 productionOffset = new Vector3(3,3);
    void Start()
    {
        objective = transform.parent.gameObject.GetComponent<Objective>();
        objective.componentSerializableData.Add(this);
        if (objective.isReconstructed)
        {
            DataStorage productionData = objective.reconstructionData.FindSubcomp(transform.name);
            DataStorage productionSlots = productionData.FindSubcomp("ProductionSlots");
            foreach (DataStorage slot in productionSlots.subcomponents)
            {
                ProductionSlot productionSlot = new ProductionSlot(slot);
                slots.Add(productionSlot);
            }
        }
        else
        {
            ComponentWithParams unitProductionComponent = objective.objectiveType.FindParam("ProduceUnits");
            ComponentWithParams productionSlots = unitProductionComponent.FindParam("ProductionSlots");
            foreach (ComponentWithParams slot in productionSlots.componentsWithParams)
            {
                ProductionSlot productionSlot = new ProductionSlot();
                float time = float.Parse(slot.FindParam("Time").componentsWithParams[0].name,
                          CultureInfo.InvariantCulture.NumberFormat);
                productionSlot.time = time;
                foreach (ComponentWithParams stage in slot.FindParam("Stages").componentsWithParams)
                {
                    productionSlot.upgrades.Add(stage.name);
                }
                productionSlot.currentType = productionSlot.upgrades[0];
                slots.Add(productionSlot);

                productionSlot.timer = time;
            }
        } 
    }

    void FixedUpdate()
    {
        if (objective.freezeLogic || objective.controller.freezeMap)
        {
            return;
        }
        TickProduction();
    }
    void TickProduction()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].timer <= 0)
            {
                bool canProduce = !objective.controller.map.CheckIfReservedOrOccupied(
                    objective.controller.GetMapTileFromWorldPosition(transform.position + productionOffset)
                    );
                if (canProduce)
                {
                    Produce(i);
                    slots[i].timer += slots[i].time;
                }
            }
            else
            {
                slots[i].timer -= Time.deltaTime;
            }
        }
    }
    void Produce(int i)
    {
        if (objective.gatherSpot!=(-1,-1))
        {
            Unit unit = objective.controller.unitFactory.CreatePlaceableUnit(
                slots[i].currentType,
                objective.faction
                );
            Placeable placeableUnit = unit;
            placeableUnit.Place(transform.position + productionOffset);
            unit.unitMovement.BeginPathfind(objective.gatherSpot);
        }
        else
        {
            Unit unit = objective.controller.unitFactory.CreatePlaceableUnit(
                slots[i].currentType,
                objective.faction
                );
            Placeable placeableUnit = unit;
            placeableUnit.Place(transform.position + productionOffset);
        }

    }

    DataStorage StoresData.GetData()
    {
        return GenerateData();
    }

    public DataStorage GenerateData()
    {
        DataStorage dataStorage = new DataStorage(transform.name);
        DataStorage productionSlots = new DataStorage("ProductionSlots");
        foreach (ProductionSlot slot in slots)
        {
            productionSlots.AddSubcomponent(slot.convertToDataStorage());
        }
        dataStorage.AddSubcomponent(productionSlots);
        return dataStorage;
    }

    bool Component.isStatic()
    {
        return false;
    }

    string Component.getName()
    {
        return transform.name;
    }

    [System.Serializable]
    class ProductionSlot
    {
        public string currentType;
        public int currentStage = 0;
        public float time;
        public List<string> upgrades = new List<string>();
        public float timer;
        public DataStorage convertToDataStorage()
        {
            DataStorage dataStorage = new DataStorage("ProductionSlot");
            dataStorage.RegisterNewParam("type", currentType);
            dataStorage.RegisterNewParam("stage",currentStage.ToString());
            dataStorage.RegisterNewParam("time", time.ToString(CultureInfo.InvariantCulture.NumberFormat));
            dataStorage.RegisterNewParam("timer", timer.ToString(CultureInfo.InvariantCulture.NumberFormat));
            DataStorage upgradeStorage = new DataStorage("Upgrades");
            for (int i = 0; i < upgrades.Count; i++)
            {
                upgradeStorage.RegisterNewParam(i.ToString(), upgrades[i]);
            }
            dataStorage.AddSubcomponent(upgradeStorage);
            return dataStorage;
        }
        public ProductionSlot(DataStorage dataStorage)
        {
            currentType = dataStorage.FindParam("type").value;
            currentStage = int.Parse(dataStorage.FindParam("stage").value);
            time = float.Parse(dataStorage.FindParam("time").value,CultureInfo.InvariantCulture.NumberFormat);
            timer = float.Parse(dataStorage.FindParam("timer").value, CultureInfo.InvariantCulture.NumberFormat);
            DataStorage upgradeStorage = dataStorage.FindSubcomp("Upgrades");
            for (int i = 0; i < upgradeStorage.parameters.Count; i++)
            {
                upgrades.Add(upgradeStorage.parameters[i].value);
            }
        }
        public ProductionSlot()
        {

        }
    }
}
