using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public string controlledFaction = "Enemy";
    public TileMap.MapController controller;
    public List<(Objective, IEnumerator)> controlledObjectives = new List<(Objective, IEnumerator)>();
    public List<(Unit, IEnumerator)> controlledUnits = new List<(Unit, IEnumerator)>();
    List<(Targetable,float)> priorityTargetQueue = new List<(Targetable,float)>();
    private void Start()
    {
        StartCoroutine(AITick());
    }
    public void RegisterUnit(Unit unit)
    {
        IEnumerator coorutine = AICoorutineUnit(unit);
        controlledUnits.Add((unit,coorutine));
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
            foreach ((Unit,IEnumerator) units in controlledUnits)
            {
                if(!units.Item1.freezeLogic)StartCoroutine(units.Item2);
            }
            yield return new WaitForSeconds(5f);
        }
        
    }
    IEnumerator AICoorutineUnit(Unit unit)
    {
        UnitAI(unit);
        yield break;
    }
}
public interface AIControllable
{
    (string,Targetable) CurrentState();
    void ReciveOrder(string order,(int,int)? target);
}