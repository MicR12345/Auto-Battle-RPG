using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProduceGold : MonoBehaviour,StoresData,Component
{
    const string resourceName = "gold";

    Objective objective;

    int stage;
    float timer;

    [SerializeField]
    List<GoldProductionStage> goldProductionStages = new List<GoldProductionStage>();
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
        public DataStorage convertToDataStorage()
        {
            DataStorage dataStorage = new DataStorage("goldProductionStage");
            dataStorage.EditParam("value", this.value.ToString());
            dataStorage.EditParam("time", this.time.ToString());
            return dataStorage;
        }
    }
    private void Start()
    {
        objective = transform.parent.GetComponent<Objective>();
        objective.componentSerializableData.Add(this);
        if (objective.isReconstructed)
        {
            DataStorage goldData = objective.reconstructionData.FindSubcomp(transform.name);
            stage = int.Parse(goldData.FindParam("stage").value);
            timer = float.Parse(goldData.FindParam("stage").value);
            List<DataStorage> productionStages = goldData.FindAllSubcomps("goldProductionStage");
            foreach (DataStorage stage in productionStages)
            {
                goldProductionStages.Add(new GoldProductionStage(stage));
            }
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
    }

    public DataStorage GetData()
    {
        DataStorage data = new DataStorage(transform.name);
        data.EditParam("stage",stage.ToString());
        data.EditParam("timer",timer.ToString());
        foreach (GoldProductionStage stage in goldProductionStages)
        {
            data.AddSubcomponent(stage.convertToDataStorage());
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
        productionStage.name = "goldProductionStage";
        List<(string, TMP_InputField.ContentType)> stageFields = new List<(string, TMP_InputField.ContentType)>();
        stageFields.Add(("value", TMP_InputField.ContentType.IntegerNumber));
        stageFields.Add(("time", TMP_InputField.ContentType.DecimalNumber));
        productionStage.fields = stageFields;
        goldProduction.subComponents.Add(productionStage);
        return goldProduction;
    }
}
