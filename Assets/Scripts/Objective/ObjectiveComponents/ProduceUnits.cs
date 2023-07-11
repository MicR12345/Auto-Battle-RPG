using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            List<DataStorage> productionSlots = productionData.FindAllSubcomps("ProductionSlots");
            foreach (DataStorage slot in productionSlots)
            {
                ProductionSlot productionSlot = new ProductionSlot(slot);
                slots.Add(productionSlot);
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
        foreach (ProductionSlot slot in slots)
        {
            dataStorage.AddSubcomponent(slot.convertToDataStorage());
        }
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

    SimpleComponent Component.getRequiredFields()
    {
        SimpleComponent produceUnits = new SimpleComponent();
        produceUnits.name = transform.name;
        List<(string, TMP_InputField.ContentType)> fields = new List<(string, TMP_InputField.ContentType)> ();
        produceUnits.fields = fields;
        produceUnits.subComponents = new List<SimpleComponent> ();


        SimpleComponent productionSlot = new SimpleComponent();
        productionSlot.name = "ProductionSlots";
        List<(string, TMP_InputField.ContentType)> productionFields = new List<(string, TMP_InputField.ContentType)>();
        productionFields.Add(("type", TMP_InputField.ContentType.Alphanumeric));
        productionFields.Add(("stage", TMP_InputField.ContentType.IntegerNumber));
        productionFields.Add(("time", TMP_InputField.ContentType.DecimalNumber));
        productionFields.Add(("timer", TMP_InputField.ContentType.DecimalNumber));
        productionSlot.count = -1;
        productionSlot.fields = productionFields;
        produceUnits.subComponents.Add(productionSlot);

        productionSlot.subComponents = new List<SimpleComponent> ();
        SimpleComponent upgrade = new SimpleComponent();
        upgrade.name = "Upgrades";
        List<(string, TMP_InputField.ContentType)> upgradeFields = new List<(string, TMP_InputField.ContentType)>();
        upgradeFields.Add(("upgradeName", TMP_InputField.ContentType.Alphanumeric));
        upgrade.fields = upgradeFields;
        productionSlot.subComponents.Add(upgrade);

        return produceUnits;
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
            DataStorage dataStorage = new DataStorage("ProductionSlots");
            dataStorage.RegisterNewParam("type", currentType);
            dataStorage.RegisterNewParam("stage",currentStage.ToString());
            dataStorage.RegisterNewParam("time", time.ToString(CultureInfo.InvariantCulture.NumberFormat));
            dataStorage.RegisterNewParam("timer", timer.ToString(CultureInfo.InvariantCulture.NumberFormat));
            for (int i = 0; i < upgrades.Count; i++)
            {
                DataStorage upgrade = new DataStorage("Upgrades");
                upgrade.RegisterNewParam("upgradeName", upgrades[i]);
                dataStorage.AddSubcomponent(upgrade);
            }
            return dataStorage;
        }
        public ProductionSlot(DataStorage dataStorage)
        {
            currentType = dataStorage.FindParam("type").value;
            currentStage = int.Parse(dataStorage.FindParam("stage").value);
            time = float.Parse(dataStorage.FindParam("time").value,CultureInfo.InvariantCulture.NumberFormat);
            timer = float.Parse(dataStorage.FindParam("timer").value, CultureInfo.InvariantCulture.NumberFormat);
            List<DataStorage> upgradesData = dataStorage.FindAllSubcomps("Upgrades");
            for (int i = 0; i < upgradesData.Count; i++)
            {
                upgrades.Add(upgradesData[i].FindParam("upgradeName").value);
            }
        }
        public ProductionSlot()
        {

        }
    }
}
