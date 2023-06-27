using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateObjectiveForm : MonoBehaviour
{
    public TMP_Dropdown spriteDropdown;

    public ObjectiveFactory objectiveFactory;
    public GameObject componentTemplate;
    public TextMeshProUGUI componentTemplateText;
    public CreateObjectiveFormButton componentTemplateFormButton;
    public Transform componentList;
    public Transform formList;
    public GameObject formTemplate;
    public TextMeshProUGUI formTemplateText;
    public TMP_InputField formTemplateInput;

    public GameObject formAddSubcompTemplate;
    public TextMeshProUGUI formAddSubcompTemplateText;

    public GameObject formModifySubcompTemplate;
    public TextMeshProUGUI formModifySubcompTemplateText;
    List<Component> components;
    List<Component> componentHeap;

    Component displayedComponent;

    DataStorage createdData = new DataStorage("Objective");
    DataStorage currentEditedDataReference;
    [HideInInspector]
    List<DataStorage> dataHeap;

    List<Counter> counters = new List<Counter>();

    TextMeshProUGUI selectedVariable;

    bool changingComponent = false;
    public Component DisplayedComponent
    {
        set
        {
            if (value==null)
            {
                currentEditedDataReference = createdData;
                dataHeap.Clear();
                ClearSimpleComponents();
            }
            foreach (Component comp in components)
            {
                if (comp == value)
                {
                    displayedComponent = comp;
                    componentHeap = new List<Component>();
                    dataHeap.Add(currentEditedDataReference);
                    currentEditedDataReference = new DataStorage(value.getName());
                    PopulateSimpleComponents(value);
                }
            }
        }
    }
    public Component NextComponent
    {
        set
        {
            changingComponent = false;
            dataHeap.Add(currentEditedDataReference);
            currentEditedDataReference = new DataStorage(value.getName());
            componentHeap.Add(displayedComponent);
            displayedComponent = value;
            PopulateSimpleComponents(value);
        }
    }
    public Component ModifyComponent
    {
        set
        {
            componentHeap.Add(displayedComponent);
            displayedComponent = value;
            changingComponent = true;
            PopulateSimpleComponents(value,currentEditedDataReference);
        }
    }
    public void PrevComponent(DataStorage data = null,bool remove = false)
    {
        changingComponent = true;
        currentEditedDataReference = dataHeap[dataHeap.Count - 1];
        dataHeap.RemoveAt(dataHeap.Count - 1);
        if (data != null && !remove)
        {
            currentEditedDataReference.AddSubcomponent(data);
        }
        if (data != null && remove)
        {
            currentEditedDataReference.subcomponents.Remove(data);
        }
        displayedComponent = componentHeap[componentHeap.Count - 1];
        componentHeap.RemoveAt(componentHeap.Count - 1);
        PopulateSimpleComponents(displayedComponent, currentEditedDataReference);
    }
    public void CreateForm()
    {
        changingComponent = false;
        createdData = new DataStorage("Objective");
        currentEditedDataReference = createdData;
        dataHeap = new List<DataStorage>();
        counters = new List<Counter>();
        spriteDropdown.options = objectiveFactory.PopulateObjectiveSpriteDropdown();
        components = objectiveFactory.FetchComponents();
        foreach (Component comp in components)
        {
            if (componentList.Find(comp.getName()) == null)
            {
                componentTemplateText.text = comp.getName();
                GameObject componentPanel = GameObject.Instantiate(componentTemplate, componentList);
                CreateObjectiveFormButton formButton = componentPanel.GetComponent<CreateObjectiveFormButton>();
                formButton.component = comp;
                componentPanel.name = comp.getName();
                componentPanel.SetActive(true);
            }
        }
    }
    void PopulateSimpleComponents(Component comp,DataStorage data = null)
    {
        ClearSimpleComponents();
        SimpleComponent simpleComponent = comp.getRequiredFields();
        foreach ((string, TMP_InputField.ContentType) item in simpleComponent.fields)
        {
            formTemplateInput.contentType = item.Item2;
            if (changingComponent)
            {
                formTemplateInput.text = data.FindParam(item.Item1).value;
            }
            formTemplateText.text = item.Item1;
            GameObject inputField = GameObject.Instantiate(formTemplate, formList);
            inputField.name = item.Item1;
            inputField.SetActive(true);
        }
        if (simpleComponent.subComponents != null && simpleComponent.subComponents.Count > 0)
        {
            foreach (SimpleComponent subSC in simpleComponent.subComponents)
            {
                if (changingComponent && data.subcomponents!=null)
                {
                    foreach (DataStorage dataStorage in data.subcomponents)
                    {
                        if (dataStorage.name == subSC.name)
                        {
                            formModifySubcompTemplateText.text = subSC.name;
                            GameObject modifySubcompButton = GameObject.Instantiate(formModifySubcompTemplate, formList);
                            CreateObjectiveSubcomponent buttonn = modifySubcompButton.GetComponent<CreateObjectiveSubcomponent>();
                            buttonn.component = subSC;
                            buttonn.dataReference = dataStorage;
                            modifySubcompButton.SetActive(true);
                        }
                    }
                }
                formAddSubcompTemplateText.text = subSC.name;
                GameObject addSubcompButton = GameObject.Instantiate(formAddSubcompTemplate, formList);
                CreateObjectiveSubcomponent button = addSubcompButton.GetComponent<CreateObjectiveSubcomponent>();
                button.component = subSC;
                addSubcompButton.SetActive(true);
            }
        }
    }
    public void ClearSimpleComponents()
    {
        foreach (Transform child in formList)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public void SetDataToModify(DataStorage data)
    {
        dataHeap.Add(currentEditedDataReference);
        currentEditedDataReference = data;
    }
    void IncrementCounter(string name)
    {
        foreach (Counter counter in counters)
        {
            if (counter.name == name)
            {
                counter.value++;
                return;
            }
        }
        Counter c = new Counter();
        c.name = name;
        c.value = 1;
        counters.Add(c);
    }
    void DecreaseCounter(string name)
    {
        foreach (Counter counter in counters)
        {
            if (counter.name == name)
            {
                counter.value--;
                return;
            }
        }
    }
    int GetCounterValue(string name)
    {
        foreach (Counter counter in counters)
        {
            if (counter.name == name)
            {
                return counter.value;
            }
        }
        Counter c = new Counter();
        c.name = name;
        c.value = 0;
        counters.Add(c);
        return c.value;
    }
    public void OnChangeData(TMP_InputField inputField)
    {
        currentEditedDataReference.EditParam(selectedVariable.text, inputField.text);
    }
    public void OnSelectVariable(TextMeshProUGUI textMeshProUGUI)
    {
        selectedVariable = textMeshProUGUI;
    }
    public void OnSave()
    {
        if (componentHeap.Count >1)
        {
            if (!changingComponent)
            {
                DataStorage temp = currentEditedDataReference;
                PrevComponent(temp);
            }
            else
            {
                PrevComponent();
            }
        }
        else
        {
            Debug.Log("TODO");
            gameObject.SetActive(false);
        }
    }
    public void OnDestroyData()
    {
        if (componentHeap.Count >1)
        {
            if (changingComponent)
            {
                DataStorage temp = currentEditedDataReference;
                PrevComponent(temp,true);
            }
            else
            {
                PrevComponent();
            }
        }
        else
        {
            DisplayedComponent = null;
        }
    }
    class Counter
    {
        public string name;
        public int value;
    }
}
