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

    bool startPainting = false;
    [SerializeField]
    SpriteRenderer spriteRenderer;
    void Update()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 20);
        switch (mode)
        {
            case CursorMode.None:
                if (Input.GetMouseButtonDown(0))
                {
                    ClearSelected();
                    startingPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mode = CursorMode.Selector;
                    RaycastHit2D raycastHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if (raycastHit.collider != null)
                    {
                        Selectable selectable;
                        if(raycastHit.collider.gameObject.TryGetComponent<Selectable>(out selectable))
                        {
                            selectable.OnSelect();
                            selected.Add(selectable);
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
                    //Units take priority in selection
                    List<Unit> units = controller.GetUnitsInAreaOfFaction(startingPoint, endPoint,"Player");
                    if (units.Count>0)
                    {
                        ClearSelected();
                        selected.AddRange(units);
                        foreach (Selectable item in units)
                        {
                            selected.Add(item);
                            item.OnSelect();
                        }
                    }
                    mode = CursorMode.None;
                }
                else
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
    void BeginPlaceableObjectMode()
    {
        mode = CursorMode.PlacementMode;
    }
    public void PlaceTile()
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        (int, int) coords = controller.GetMapTileFromWorldPosition(point);
        controller.PlaceTile(tileHandle, coords.Item1, coords.Item2);
    }
}
public interface Selectable
{
    void OnSelect();
    void OnAction(Vector3 screenPostion);
    void Unselect();
    bool IsDeadInside();
}
public interface Placeable
{
    void Place(Vector3 position);
    void Discard();
}
