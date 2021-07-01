using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ContainerSideBarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Direction directionFromPlayer;

    [HideInInspector] public Image icon;

    Image directionIcon;
    GameManager gm;

    Color highlightColor = new Color(0, 0.4f, 0.62f);

    void Start()
    {
        gm = GameManager.instance;
        icon = transform.GetChild(0).GetComponent<Image>();
        directionIcon = transform.GetChild(1).GetComponent<Image>();

        if (directionFromPlayer == Direction.Center)
            HighlightDirectionIcon();
    }

    public Inventory GetInventory()
    {
        switch (directionFromPlayer)
        {
            case Direction.North:
                return gm.containerInvUI.northInventory;
            case Direction.South:
                return gm.containerInvUI.southInventory;
            case Direction.West:
                return gm.containerInvUI.westInventory;
            case Direction.East:
                return gm.containerInvUI.eastInventory;
            case Direction.Northwest:
                return gm.containerInvUI.northwestInventory;
            case Direction.Northeast:
                return gm.containerInvUI.northeastInventory;
            case Direction.Southwest:
                return gm.containerInvUI.southwestInventory;
            case Direction.Southeast:
                return gm.containerInvUI.southeastInventory;
            default:
                return null;
        }
    }

    public List<ItemData> GetItemsList()
    {
        switch (directionFromPlayer)
        {
            case Direction.Center:
                return gm.containerInvUI.playerPositionItems;
            case Direction.North:
                return gm.containerInvUI.northItems;
            case Direction.South:
                return gm.containerInvUI.southItems;
            case Direction.West:
                return gm.containerInvUI.westItems;
            case Direction.East:
                return gm.containerInvUI.eastItems;
            case Direction.Northwest:
                return gm.containerInvUI.northwestItems;
            case Direction.Northeast:
                return gm.containerInvUI.northeastItems;
            case Direction.Southwest:
                return gm.containerInvUI.southwestItems;
            case Direction.Southeast:
                return gm.containerInvUI.southeastItems;
            default:
                return null;
        }
    }

    public void ShowInventoryItems()
    {
        gm.containerInvUI.ResetContainerIcons(directionFromPlayer);

        switch (directionFromPlayer)
        {
            case Direction.Center:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.playerPositionItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.North:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.northItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.South:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.southItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.West:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.westItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.East:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.eastItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Northwest:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.northwestItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Northeast:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.northeastItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Southwest:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.southwestItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Southeast:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.southeastItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            default:
                break;
        }
    }

    public void HighlightDirectionIcon()
    {
        directionIcon.color = highlightColor;
    }

    public void ResetDirectionIconColor()
    {
        directionIcon.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gm.uiManager.activeContainerSideBarButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gm.uiManager.activeContainerSideBarButton == this)
            gm.uiManager.activeContainerSideBarButton = null;
    }
}
