using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateObjectiveFormButton : MonoBehaviour
{
    public CreateObjectiveForm form;
    public TMPro.TextMeshProUGUI componentName;

    public Component component;
    public void OnButtonPress()
    {
        form.DisplayedComponent = component;
    }
}
