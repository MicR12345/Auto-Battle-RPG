using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateObjectiveForm : MonoBehaviour
{
    public TMP_Dropdown spriteDropdown;

    public ObjectiveFactory objectiveFactory;
    public GameObject componentTemplate;
    public TextMeshProUGUI componentTemplateText;

    public Transform componentList;

    List<Component> components;

    public void CreateForm()
    {
        spriteDropdown.options = objectiveFactory.PopulateObjectiveSpriteDropdown();
        components = objectiveFactory.FetchComponents();
        foreach (Component comp in components)
        {
            if (componentList.Find(comp.getName())==null)
            {
                componentTemplateText.text = comp.getName();
                GameObject componentPanel = GameObject.Instantiate(componentTemplate, componentList);
                componentPanel.name = comp.getName();
                componentPanel.SetActive(true);
            }
        }
    }
}
