using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public string controlledFaction = "Enemy";
    public TileMap.MapController controller;
    public List<Objective> controlledObjectives = new List<Objective>();
    public List<Unit> controlledUnits = new List<Unit>();
    List<(Targetable,float)> priorityTargetQueue = new List<(Targetable,float)>();
    private void Start()
    {
        StartCoroutine(AITick());
    }
    public void RegisterUnit(Unit unit)
    {
        controlledUnits.Add(unit);
    }
    public void RegisterObjective(Objective objective)
    {
        controlledObjectives.Add(objective);
    }
    public void UnregisterUnit(Unit unit)
    {
        controlledUnits.Remove(unit);
    }
    public void UnregisterObjective(Objective objective)
    {
        controlledObjectives.Remove(objective);
    }
    void ObjectiveAI(Objective objective)
    {
        AIControllable aiControllable = objective as AIControllable;
        if (priorityTargetQueue.Count > 0)
        {
            aiControllable.ReciveOrder("target", controller.GetMapTileFromWorldPosition(priorityTargetQueue[0].Item1.GetShootPosition()));
        }
    }
    void UnitAI(Unit unit)
    {
        AIControllable aiControllable = unit as AIControllable;
        (string,Targetable) state = aiControllable.CurrentState();
        if (state.Item1 == "idle")
        {
            if (priorityTargetQueue.Count>0)
            {
                aiControllable.ReciveOrder("move", controller.GetMapTileFromWorldPosition(priorityTargetQueue[0].Item1.GetShootPosition()));
            }
        }
    }
    void PopulateTargetQueue()
    {
        priorityTargetQueue.Clear();
        foreach (Objective objective in controller.objectives)
        {
            if (objective.faction!=controlledFaction)
            {
                priorityTargetQueue.Add((objective, Random.Range(1f,100f)));
            }
        }
        priorityTargetQueue.Sort((x,y) => y.Item2.CompareTo(x.Item2));
    }
    IEnumerator AITick()
    {
        for(; ; )
        {
            PopulateTargetQueue();
            foreach (Unit units in controlledUnits)
            {
                if(!units.freezeLogic)UnitAI(units);
            }
            foreach (Objective objective in controlledObjectives)
            {
                if (!objective.freezeLogic) ObjectiveAI(objective);
            }
            yield return new WaitForSeconds(5f);
        }
        
    }
}
public interface AIControllable
{
    (string,Targetable) CurrentState();
    void ReciveOrder(string order,(int,int)? target);
}