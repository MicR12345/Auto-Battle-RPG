using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public class ProduceUnits : MonoBehaviour
{
    Objective objective;
    [SerializeField]
    List<ProductionSlot> slots = new List<ProductionSlot>();
    List<float> timers = new List<float>();

    Vector3 productionOffset = Vector3.one * 3;
    void OnEnable()
    {
        objective = transform.parent.gameObject.GetComponent<Objective>();
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
            timers.Add(time);
        } 
    }

    void FixedUpdate()
    {
        for (int i = 0; i < timers.Count; i++)
        {
            if (timers[i]<=0)
            {
                bool canProduce = !objective.controller.map.CheckIfReservedOrOccupied(
                    objective.controller.GetMapTileFromWorldPosition(transform.position + productionOffset)
                    );
                if (canProduce)
                {
                    Produce(i);
                    timers[i] += slots[i].time;
                }
            }
            else
            {
                timers[i] -= Time.deltaTime;
            }
        }
    }
    void Produce(int i)
    {
        if (objective.gatherSpot!=(-1,-1))
        {
            objective.controller.RegisterUnit(objective.controller.unitFactory.CreateUnit(
                slots[i].currentType,
                transform.position + productionOffset,
                objective.faction,
                objective.gatherSpot
             ));
        }
        else
        {
            objective.controller.RegisterUnit(objective.controller.unitFactory.CreateUnit(
                slots[i].currentType,
                transform.position + productionOffset,
                objective.faction,
                objective.controller.GetMapTileFromWorldPosition(transform.position + productionOffset)
                ));
        }

    }
    [System.Serializable]
    class ProductionSlot
    {
        public string currentType;
        public int currentStage = 0;
        public float time;
        public List<string> upgrades = new List<string>();
    }
}
