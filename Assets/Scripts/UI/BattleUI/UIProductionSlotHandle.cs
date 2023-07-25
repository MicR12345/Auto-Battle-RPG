using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProductionSlotHandle : MonoBehaviour
{
    public HealthBar healthBar;
    public Image image;
    public TMPro.TextMeshProUGUI nameText;
    public GameObject upgradeButton;
    public ProductionState state;
    [SerializeField]
    ProductionList productionList;
    public void OnUpgradeButton()
    {
        if (state.upgradeable)
        {
            if (productionList.controller.factionResourceManager.RemoveResource("Player","gold",500))
            {
                state.upgradeableRef.Upgrade();
            }
        }
    }
}
