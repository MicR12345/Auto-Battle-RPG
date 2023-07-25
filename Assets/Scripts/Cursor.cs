using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Cursor : MonoBehaviour
{
    public enum CursorMode{
        None,
        Selector,
        TilePlacementMode,
        PlacementMode
    }
    public CursorMode mode;
    public List<Selectable> selected = new List<Selectable>();
    public Placeable placeable;
    public TileMap.MapTile tileHandle;

    [SerializeField]
    private TileMap.MapController controller;

    Vector3 startingPoint = Vector3.zero;
    Vector3 endPoint = Vector3.zero;

    public Transform selectorTransform;

    bool startPainting = false;
    [SerializeField]
    SpriteRenderer spriteRenderer;

    public CreateObjectiveForm objectiveForm;

    public TMPro.TextMeshProUGUI selectedNameText;
    public TMPro.TextMeshProUGUI selectedDescriptionText;

    public ProductionList productionList;
    void Update()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 20);
        switch (mode)
        {
            case CursorMode.None:
                if (Input.GetKeyDown(KeyCode.F1) && controller.mapEditorMode)
                {
                    if (selected.Count==1 && selected[0].GetType() == typeof(Objective))
                    {
                        objectiveForm.gameObject.SetActive(true);
                        selected[0].PerformCommand("freeze");
                        objectiveForm.EditForm(selected[0].GetData(), selected[0]);
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    ClearSelected();
                    startingPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    selectorTransform.gameObject.SetActive(true);
                    mode = CursorMode.Selector;
                    RaycastHit2D raycastHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if (raycastHit.collider != null)
                    {
                        Selectable selectable;
                        if(raycastHit.collider.gameObject.TryGetComponent<Selectable>(out selectable))
                        {
                            if (selectable.GetFaction()=="Player" || controller.mapEditorMode)
                            {
                                selectable.OnSelect();
                                selected.Add(selectable);
                            }
                        }
                    }
                }
                else
                {
                    //TODO: Handle changing cursor sprites
                }
                if (Input.GetMouseButtonDown(1))
                {
                    foreach (Selectable item in selected)
                    {
                        item.OnAction(Input.mousePosition);
                    }
                }
                break;
            case CursorMode.Selector:
                if (Input.GetMouseButtonUp(0))
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    selectorTransform.localScale = Vector3.zero;
                    selectorTransform.gameObject.SetActive(false);
                    //Units take priority in selection
                    List<Unit> units = controller.GetUnitsInAreaOfFaction(startingPoint, endPoint,"Player");
                    if (units.Count>0)
                    {
                        ClearSelected();
                        foreach (Selectable item in units)
                        {
                            selected.Add(item);
                            item.OnSelect();
                        }
                    }
                    mode = CursorMode.None;
                    UpdateUI();
                }
                else
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    selectorTransform.position = (endPoint - startingPoint)/2 + new Vector3(startingPoint.x,startingPoint.y);
                    selectorTransform.localScale = endPoint - startingPoint;
                }
                break;
            case CursorMode.TilePlacementMode:
                if (Input.GetMouseButtonDown(0))
                {
                    startPainting = true;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    startPainting = false;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    mode = CursorMode.None;
                    spriteRenderer.color = Color.white;
                    spriteRenderer.sprite = null;
                }
                if (startPainting)
                {
                    PlaceTile();
                }
                break;
            case CursorMode.PlacementMode:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    (int, int) coords = controller.GetMapTileFromWorldPosition(point);
                    Vector3 tilePosition = new Vector3(coords.Item1 + 0.5f, coords.Item2 + 0.5f);
                    placeable.Place(tilePosition);
                    mode = CursorMode.None;
                    spriteRenderer.color = Color.white;
                    spriteRenderer.sprite = null;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    mode = CursorMode.None;
                    spriteRenderer.color = Color.white;
                    spriteRenderer.sprite = null;
                    placeable.Discard();
                }
                break;
        }
    }
    void UpdateUI()
    {
        if (selected.Count==0)
        {
            return;
        }
        if (selected.Count<=1)
        {
            selectedNameText.text = selected[0].GetName();
            selectedDescriptionText.text = selected[0].GetDescription();
            productionList.Selectable = selected[0];
        }
        else
        {
            selectedNameText.text = "Multiple";
            selectedDescriptionText.text = "Selected multiple objects x" + selected.Count;
        }
    }
    void ClearSelected()
    {
        foreach (Selectable item in selected)
        {
            if (!item.IsDeadInside())
            {
                item.Unselect();
            }
            
        }
        selected.Clear();
    }
    public void BeginTilePlacementMode()
    {
        tileHandle = controller.FindTile(controller.tileDropdown.options[controller.tileDropdown.value].text);
        mode = CursorMode.TilePlacementMode;
        spriteRenderer.sprite = tileHandle.sprite[0];
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
    }
    public void BeginObjectivePlacement()
    {
        placeable = controller.objectiveFactory.ReconstructObjectiveFromData(controller.FindObjectivePrefab(controller.objectiveDropdown.options[controller.objectiveDropdown.value].text));
        BeginPlaceableObjectMode();
    }
    public void BeginUnitPlacement()
    {
        placeable = controller.unitFactory.CreatePlaceableUnit(controller.unitDropdown.options[controller.unitDropdown.value].text, controller.factionSelectionDropdown.options[controller.factionSelectionDropdown.value].text);
        BeginPlaceableObjectMode();
    }
    public void BeginTurretPlacement()
    {
        placeable = controller.turretFactory.CreatePlaceableTurret(controller.turretDropdown.options[controller.turretDropdown.value].text, controller.factionSelectionDropdown.options[controller.factionSelectionDropdown.value].text);
        BeginPlaceableObjectMode();
    }
    void BeginPlaceableObjectMode()
    {
        mode = CursorMode.PlacementMode;
    }
    public void PlaceTile()
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        (int, int) coords = controller.GetMapTileFromWorldPosition(point);
        if (coords.Item1>=0 && coords.Item2>=0 && coords.Item1<controller.mapSizeX&& coords.Item2< controller.mapSizeY)
        {
            controller.PlaceTile(tileHandle, coords.Item1, coords.Item2);
        }
    }
}
public interface Selectable
{
    void OnSelect();
    void OnAction(Vector3 screenPostion);
    void Unselect();
    bool IsDeadInside();
    string GetFaction();
    DataStorage GetData();
    void PerformCommand(string command);
    List<ProducesStuff> GetProductionData();
    string GetName();
    string GetDescription();
}
public interface Placeable
{
    void Place(Vector3 position);
    void Discard();
}
