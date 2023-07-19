using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateObjectiveForm : MonoBehaviour
{
    public TileMap.MapController controller;

    public TMP_Dropdown spriteDropdown;

    public ObjectiveFactory objectiveFactory;

    public Image objectiveImagePreview;

    public GameObject componentTemplate;
    public Image componentTemplateImage;
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
    public Toggle loseCondition;
    public Toggle partialLoseCondition;

    bool isEdited = false;
    Selectable editedObjective = null;
    public Component DisplayedComponent
    {
        set
        {
            if (value==null)
            {
                currentEditedDataReference = createdData;
                dataHeap.Clear();
                ClearSimpleComponents();
                RecolorComponentButtons();
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
        ClearSimpleComponents();
        ClearCreationValues();
        isEdited = false;
        editedObjective = null;
        //changingComponent = false;
        changingComponent.Add(true);
        createdData = new DataStorage("Objective");
        currentEditedDataReference = createdData;
        dataHeap = new List<DataStorage>();
        counters = new List<Counter>();
        spriteDropdown.options = objectiveFactory.PopulateObjectiveSpriteDropdown();
        SetObjectivePreview();
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
        RecolorComponentButtons();
    }
    public void EditForm(DataStorage dataStorage,Selectable selectable)
    {
        ClearSimpleComponents();
        ClearCreationValues();
        isEdited = true;
        editedObjective = selectable;
        //changingComponent = false;
        changingComponent.Add(true);
        createdData = dataStorage;
        currentEditedDataReference = createdData;
        FillCreationData();

        dataHeap = new List<DataStorage>();
        counters = new List<Counter>();
        spriteDropdown.options = objectiveFactory.PopulateObjectiveSpriteDropdown();
        SetObjectivePreview();
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
        RecolorComponentButtons();
    }
    public void SetObjectivePreview()
    {
        //TODO
        ObjectiveGraphics objectiveGraphics = objectiveFactory.FindGraphics(spriteDropdown.options[spriteDropdown.value].text);
        if (objectiveGraphics != null)
        {
            objectiveImagePreview.sprite = objectiveGraphics.objectiveSprites[0].stateSprite[0];
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
    public void RecolorComponentButtons()
    {
        foreach (Transform t in componentList)
        {
            Image image;
            bool found = t.TryGetComponent<Image>(out image);
            if (found)
            {
                image.color = Color.white;
            }
        }
        if (createdData.subcomponents is not null)
        {
            foreach (DataStorage subcomp in createdData.subcomponents)
            {
                Transform transform = componentList.Find(subcomp.name);
                if (transform is not null)
                {
                    Image image;
                    bool found = transform.TryGetComponent<Image>(out image);
                    if (found)
                    {
                        image.color = Color.green;
                    }
                }
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
            DisplayedComponent = null;
            //gameObject.SetActive(false);
        }
    }
    public void OnDestroyData()
    {
        //TODO
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
            currentEditedDataReference = createdData;
            dataHeap.RemoveAt(dataHeap.Count - 1);
            DisplayedComponent = null;
        }
    }
    public void ClearCreationValues()
    {
        objectiveName.text = "";
        maxHP.text = "5000";
        faction.value = 0;
        hp.value = 1;
        spriteDropdown.value = 0;
        partialLoseCondition.isOn = false;
        loseCondition.isOn = false;
    }
    public void FillCreationData()
    {
        objectiveName.text = createdData.FindParam("name").value;
        maxHP.text = createdData.FindParam("maxHP").value;
        faction.value = FindFaction(createdData.FindParam("faction").value);
        hp.value = int.Parse(createdData.FindParam("hp").value)/float.Parse(maxHP.text);
        spriteDropdown.value = 0;
        partialLoseCondition.isOn = bool.Parse(createdData.FindParam("partialLoseCondition").value);
        loseCondition.isOn = bool.Parse(createdData.FindParam("loseCondition").value);
    }
    int FindFaction(string factionName)
    {
        for (int i = 0; i < faction.options.Count; i++)
        {
            if (faction.options[i].text== factionName)
            {
                return i;
            }
        }
        return -1;
    }
    public void OnCreate()
    {
        createdData.EditParam("name",objectiveName.text);
        createdData.EditParam("maxHP", maxHP.text);
        createdData.EditParam("faction", faction.options[faction.value].text);
        createdData.EditParam("hp", Mathf.FloorToInt(hp.value * int.Parse(maxHP.text)).ToString());
        createdData.EditParam("gatherSpotX", "-1");
        createdData.EditParam("gatherSpotY", "-1");
        createdData.EditParam("graphicsPackage", spriteDropdown.options[spriteDropdown.value].text);
        createdData.EditParam("partialLoseCondition", partialLoseCondition.isOn.ToString());
        createdData.EditParam("loseCondition", loseCondition.isOn.ToString());
        if (!isEdited)
        {
            SaveManager.SaveObjectiveData(ref createdData, objectiveName.text);
            objectiveFactory.controller.LoadObjectives();
        }
        else
        {
            int x = int.Parse(createdData.FindParam("x").value);
            int y = int.Parse(createdData.FindParam("y").value);
            Placeable objective = objectiveFactory.ReconstructObjectiveFromData(createdData);
            Vector3 tilePosition = new Vector3(x + 0.5f, y + 0.5f);
            editedObjective.PerformCommand("destroyIgnoreConditions");
            objective.Place(tilePosition);
        }
        gameObject.SetActive(false);
    }
    class Counter
    {
        public string name;
        public int value;
    }
}
