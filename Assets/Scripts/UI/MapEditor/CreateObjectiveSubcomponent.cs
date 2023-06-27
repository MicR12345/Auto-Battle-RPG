using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjectiveSubcomponent : MonoBehaviour
{
    public CreateObjectiveForm form;
    public TMPro.TextMeshProUGUI componentName;

    public Component component;
    public DataStorage dataReference;
    public void OnCreatePress()
    {
        form.NextComponent = component;
    }
    public void OnModifyPress()
    {
        form.SetDataToModify(dataReference);
        form.ModifyComponent = component;
    }
}
