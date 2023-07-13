using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureBars : MonoBehaviour
{
    public GameObject barPrefab;
    public GameObject listParent;
    public List<(HealthBar, string)> factionBars = new List<(HealthBar, string)> ();
    public void ClearBars()
    {
        for (int i = 0; i < factionBars.Count; i++)
        {
            factionBars[i].Item1.SetHealthBar(0, 100);
            factionBars[i].Item1.gameObject.SetActive (false);
        }
    }
    public void SetBar(string faction,int value,int maxValue)
    {
        (HealthBar, string)? bar = FindBar(faction);
        if (bar.HasValue)
        {
            if (!bar.Value.Item1.gameObject.activeSelf)
            {
                bar.Value.Item1.gameObject.SetActive(true);
            }
            bar.Value.Item1.SetHealthBar(value, maxValue);
        }
        else
        {
            (HealthBar, string) newBar = CreateBar(faction);
            newBar.Item1.SetHealthBar(value, maxValue);
            if (faction=="Enemy")
            {
                newBar.Item1.SetBarColor(Color.magenta);
            }
            if (faction == "Player")
            {
                newBar.Item1.SetBarColor(Color.green);
            }
        }
    }
    (HealthBar,string)? FindBar(string faction)
    {
        foreach ((HealthBar,string) item in factionBars)
        {
            if (item.Item2==faction)
            {
                return item;
            }
        }
        return null;
    }
    (HealthBar,string) CreateBar(string faction)
    {
        GameObject bar = GameObject.Instantiate(barPrefab);
        HealthBar healthBar = bar.GetComponent<HealthBar>();
        bar.SetActive(true);
        bar.transform.parent = listParent.transform;
        (HealthBar, string) pair = (healthBar, faction);
        factionBars.Add(pair);
        return pair;
    }
}
