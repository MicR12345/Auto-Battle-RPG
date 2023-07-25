using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProduceGold : MonoBehaviour,StoresData,Component,ProducesStuff,Upgradeable
{
    public string resourceName = "gold";

    Objective objective;

    int stage;
    float timer;

    [SerializeField]
    List<GoldProductionStage> goldProductionStages = new List<GoldProductionStage>();
    ProductionState productionState;
    [System.Serializable]
    public class GoldProductionStage
    {
        public int value;
        public float time;
        public GoldProductionStage(DataStorage dataStorage)
        {
            this.value = int.Parse(dataStorage.FindParam("value").value);
            this.time = float.Parse(dataStorage.FindParam("time").value);

        }
        public DataStorage convertToDataStorage(string resourceName)
        {
            DataStorage dataStorage = new DataStorage(resourceName + "ProductionStage");
            dataStorage.EditParam("value", this.value.ToString());
            dataStorage.EditParam("time", this.time.ToString());
            return dataStorage;
        }
    }
    private void Start()
    {
        objective = transform.parent.GetComponent<Objective>();
        objective.componentSerializableData.Add(this);
        objective.productionComponents.Add(this);
        if (objective.isReconstructed)
        {
            DataStorage goldData = objective.reconstructionData.FindSubcomp(transform.name);
            stage = int.Parse(goldData.FindParam("stage").value);
            timer = float.Parse(goldData.FindParam("stage").value);
            List<DataStorage> productionStages = goldData.FindAllSubcomps(resourceName + "ProductionStage");
            foreach (DataStorage stage in productionStages)
            {
                goldProductionStages.Add(new GoldProductionStage(stage));
            }
            GenerateProductionStates();
        }
    }
    private void FixedUpdate()
    {
        if (objective.freezeLogic || objective.controller.freezeMap) return;
        if (timer<=0)
        {
            objective.controller.factionResourceManager.AddResource(objective.faction, resourceName, goldProductionStages[stage].value);
            timer += goldProductionStages[stage].time;
        }
        else
        {
            timer -= Time.deltaTime;
        }
        productionState.Progress = Mathf.CeilToInt((timer * 100) / (goldProductionStages[stage].time));
    }
    void GenerateProductionStates()
    {
        bool isUpgradeable = CheckIfUpgradeable();
        Upgradeable upgradeable = null;
        if (isUpgradeable)
        {
            upgradeable = this;
        }
        this.productionState = new ProductionState(resourceName, null,
            Mathf.CeilToInt((timer * 100) / (goldProductionStages[stage].time)), 100, isUpgradeable, upgradeable);
    }
    void UpdateProductionState()
    {
        productionState.progress = Mathf.CeilToInt((timer * 100) / (goldProductionStages[stage].time));
        bool isUpgradeable = CheckIfUpgradeable();
        Upgradeable upgradeable = null;
        if (isUpgradeable)
        {
            upgradeable = this;
        }
        productionState.Upgradeable = isUpgradeable;
        productionState.upgradeableRef = upgradeable;
    }
    bool CheckIfUpgradeable()
    {
        if (goldProductionStages.Count>stage+1)
        {
            return true;
        }
        return false;
    }

    public DataStorage GetData()
    {
        DataStorage data = new DataStorage(transform.name);
        data.EditParam("stage",stage.ToString());
        data.EditParam("timer",timer.ToString());
        foreach (GoldProductionStage stage in goldProductionStages)
        {
            data.AddSubcomponent(stage.convertToDataStorage(resourceName));
        }
        return data;
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
        SimpleComponent goldProduction = new SimpleComponent();
        goldProduction.name = transform.name;
        List<(string, TMP_InputField.ContentType)> fields = new List<(string, TMP_InputField.ContentType)>();
        fields.Add(("stage", TMP_InputField.ContentType.IntegerNumber));
        fields.Add(("timer", TMP_InputField.ContentType.DecimalNumber));
        goldProduction.fields = fields;
        goldProduction.subComponents = new List<SimpleComponent>();

        SimpleComponent productionStage = new SimpleComponent();
        productionStage.name = resourceName + "ProductionStage";
        List<(string, TMP_InputField.ContentType)> stageFields = new List<(string, TMP_InputField.ContentType)>();
        stageFields.Add(("value", TMP_InputField.ContentType.IntegerNumber));
        stageFields.Add(("time", TMP_InputField.ContentType.DecimalNumber));
        productionStage.fields = stageFields;
        goldProduction.subComponents.Add(productionStage);
        return goldProduction;
    }

    List<ProductionState> ProducesStuff.GetProductionStates()
    {
        List<ProductionState> states = new List<ProductionState>();
        states.Add(productionState);
        return states;
    }

    public void Upgrade()
    {
        stage++;
        timer = goldProductionStages[stage].time;
        UpdateProductionState();
    }
}
