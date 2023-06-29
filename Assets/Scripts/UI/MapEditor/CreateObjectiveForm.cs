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

    List<bool> changingComponent = new List<bool>();

    public TMP_InputField objectiveName;
    public TMP_InputField maxHP;
    public TMP_Dropdown faction;
    public Slider hp;
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
                    DataStorage data = currentEditedDataReference.FindSubcomp(value.getName());
                    if (data!=null)
                    {
                        currentEditedDataReference = data;
                    }
                    else
                    {
                        currentEditedDataReference = new DataStorage(value.getName());
                    }
                    
                    PopulateSimpleComponents(value, currentEditedDataReference);
                }
            }
        }
    }
    public Component NextComponent
    {
        set
        {
            changingComponent.Add(false);
            //changingComponent = false;
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
            //changingComponent = true;
            changingComponent.Add(true);
            PopulateSimpleComponents(value,currentEditedDataReference);
        }
    }
    public void PrevComponent(DataStorage data = null,bool remove = false)
    {
        selectedVariable = null;
        //changingComponent = true;
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
        changingComponent.RemoveAt(changingComponent.Count - 1);
        changingComponent.Add(true);
        PopulateSimpleComponents(displayedComponent, currentEditedDataReference);
        changingComponent.RemoveAt(changingComponent.Count - 1);
        //changingComponent = false;
    }
    public void CreateForm()
    {
        //changingComponent = false;
        changingComponent.Add(true);
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
            if (changingComponent[changingComponent.Count - 1])
            {
                Parameter value = data.FindParam(item.Item1);
                if (value!=null)
                {
                    formTemplateInput.text = value.value;
                }
            }
            else
            {
                formTemplateInput.text = "";
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
                if (changingComponent[changingComponent.Count-1] && data.subcomponents!=null)
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
        if (selectedVariable!=null)
        {
            currentEditedDataReference.EditParam(selectedVariable.text, inputField.text);
        }
    }
    public void OnSelectVariable(TextMeshProUGUI textMeshProUGUI)
    {
        selectedVariable = textMeshProUGUI;
    }
    public void OnSave()
    {
        if (componentHeap.Count >0)
        {
            if (!changingComponent[changingComponent.Count - 1])
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
            Component baseComp = displayedComponent;
            //changingComponent = true;
            DataStorage data = createdData.FindSubcomp(displayedComponent.getName());
            if (data==null)
            {
                createdData.AddSubcomponent(currentEditedDataReference);
            }
            currentEditedDataReference = createdData;
            dataHeap.RemoveAt(dataHeap.Count-1);
            DisplayedComponent = baseComp;
            //gameObject.SetActive(false);
        }
    }
    public void OnDestroyData()
    {
        if (componentHeap.Count >0)
        {
            if (changingComponent[changingComponent.Count - 1])
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
            Component baseComp = displayedComponent;
            //changingComponent = true;
            PrevComponent();
            DisplayedComponent = baseComp;
        }
    }
    public void OnCreate()
    {
        createdData.RegisterNewParam("name",objectiveName.text);
        createdData.RegisterNewParam("maxHP", maxHP.text);
        createdData.RegisterNewParam("faction", faction.options[faction.value].text);
        createdData.RegisterNewParam("hp", Mathf.FloorToInt(hp.value * int.Parse(maxHP.text)).ToString());
        createdData.RegisterNewParam("gatherSpotX", "-1");
        createdData.RegisterNewParam("gatherSpotY", "-1");
        SaveManager.SaveObjectiveData(ref createdData, objectiveName.text);
        gameObject.SetActive(false);
    }
    class Counter
    {
        public string name;
        public int value;
    }
}
