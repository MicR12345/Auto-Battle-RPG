using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatusManager : MonoBehaviour
{
    public class LoseConditionHolder
    {
        public Objective objective;
        public string faction;
        public bool isOnlyPartial;
        public bool isLost = false;
        public LoseConditionHolder(Objective objective,string faction,bool isOnlyPartial)
        {
            this.objective = objective;
            this.faction = faction;
            this.isOnlyPartial = isOnlyPartial;
        }
    }
    public List<LoseConditionHolder> defeatConditionHolders = new List<LoseConditionHolder>();
    public void RegisterLoseCondition(Objective objective)
    {
        defeatConditionHolders.Add(new LoseConditionHolder(objective, objective.faction, objective.partialLoseCondition));
    }
    public void UpdateLoseCondition(Objective objective)
    {
        foreach (LoseConditionHolder holder in defeatConditionHolders)
        {
            if (holder.objective == objective)
            {
                if (holder.faction != objective.faction)
                {
                    holder.isLost = true;
                    if (!holder.isOnlyPartial)
                    {
                        OnDefeat(holder.faction);
                    }
                    else
                    {
                        CheckForPartialDefeat(holder.faction);
                    }
                }
            }
        }
    }
    public void RemoveCondition(Objective objective)
    {
        for (int i = 0; i < defeatConditionHolders.Count; i++)
        {
            if (defeatConditionHolders[i].objective==objective)
            {
                defeatConditionHolders.RemoveAt(i);
                return;
            }
        }
    }
    public void OnDefeat(string faction)
    {
        Debug.Log(faction + " defeated");
    }
    public void CheckForPartialDefeat(string faction)
    {
        foreach (LoseConditionHolder holder in defeatConditionHolders)
        {
            if (holder.faction==faction && !holder.isLost)
            {
                return;
            }
        }
        OnDefeat(faction);
    }
}
