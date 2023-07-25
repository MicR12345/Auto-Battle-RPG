using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionList : MonoBehaviour
{
    public TileMap.MapController controller;

    public GameObject productionSlotTemplate;

    public Transform listReference;
    List<ProductionState> productionStates;
    public Selectable Selectable
    {
        set
        {
            ClearList();
            productionStates = new List<ProductionState>();
            List<ProducesStuff> producesStuffs = value.GetProductionData();
            if (producesStuffs is not null)
            {
                foreach (ProducesStuff item in producesStuffs)
                {
                    productionStates.AddRange(item.GetProductionStates());
                }
                foreach (ProductionState state in productionStates)
                {
                    GameObject gameObject = GameObject.Instantiate(productionSlotTemplate);
                    gameObject.transform.SetParent(listReference);
                    UIProductionSlotHandle uIProductionSlotHandle = gameObject.GetComponent<UIProductionSlotHandle>();
                    uIProductionSlotHandle.state = state;
                    uIProductionSlotHandle.nameText.text = state.name;
                    uIProductionSlotHandle.image.sprite = state.image;
                    uIProductionSlotHandle.upgradeButton.SetActive(state.upgradeable);
                    state.productionSlotHandle = uIProductionSlotHandle;
                    gameObject.SetActive(true);
                }
            }
        }
    }
    void ClearList()
    {
        if (productionStates!=null)
        {
            foreach (ProductionState state in productionStates)
            {
                state.productionSlotHandle = null;
            }
            productionStates.Clear();
        }
        foreach (Transform transform in listReference)
        {
            GameObject.Destroy(transform.gameObject);
        }
    }
}
public interface ProducesStuff
{
    public List<ProductionState> GetProductionStates();
}
public interface Upgradeable
{
    public void Upgrade();
}
public class ProductionState
{
    public string name;
    public Sprite image;
    public int Progress
    {
        set
        {
            progress = value;
            UpdateBar();
        }
    }
    public int progress;
    public int maxProgress;
    public bool upgradeable;
    public bool Upgradeable
    {
        set
        {
            upgradeable = value;
            UpdateGameObject();
        }
    }
    public Upgradeable upgradeableRef;

    public void UpdateGameObject()
    {
        if (productionSlotHandle is not null)
        {
            productionSlotHandle.nameText.text = name;
            productionSlotHandle.image.sprite = image;
            productionSlotHandle.upgradeButton.SetActive(upgradeable);
        }
    }
    public void UpdateBar()
    {
        if (productionSlotHandle is not null)
        {
            productionSlotHandle.healthBar.SetHealthBar(maxProgress - progress, maxProgress);
        }
    }
    public UIProductionSlotHandle productionSlotHandle;
    public ProductionState(string name, Sprite image,int progress,int maxProgress,bool upgradeable = false,Upgradeable refer = null)
    {
        this.name = name;
        this.image = image;
        this.progress = progress;
        this.maxProgress = maxProgress;
        this.upgradeable = upgradeable;
        this.upgradeableRef = refer;
    }
}