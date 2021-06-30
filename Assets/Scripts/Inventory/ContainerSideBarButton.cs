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
            case Direction.Up:
                return gm.containerInvUI.upInventory;
            case Direction.Down:
                return gm.containerInvUI.downInventory;
            case Direction.Left:
                return gm.containerInvUI.leftInventory;
            case Direction.Right:
                return gm.containerInvUI.rightInventory;
            case Direction.UpLeft:
                return gm.containerInvUI.upLeftInventory;
            case Direction.UpRight:
                return gm.containerInvUI.upRightInventory;
            case Direction.DownLeft:
                return gm.containerInvUI.downLeftInventory;
            case Direction.DownRight:
                return gm.containerInvUI.downRightInventory;
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
            case Direction.Up:
                return gm.containerInvUI.upItems;
            case Direction.Down:
                return gm.containerInvUI.downItems;
            case Direction.Left:
                return gm.containerInvUI.leftItems;
            case Direction.Right:
                return gm.containerInvUI.rightItems;
            case Direction.UpLeft:
                return gm.containerInvUI.upLeftItems;
            case Direction.UpRight:
                return gm.containerInvUI.upRightItems;
            case Direction.DownLeft:
                return gm.containerInvUI.downLeftItems;
            case Direction.DownRight:
                return gm.containerInvUI.downRightItems;
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
            case Direction.Up:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.upItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Down:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.downItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Left:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.leftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Right:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.rightItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.UpLeft:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.upLeftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.UpRight:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.upRightItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.DownLeft:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.downLeftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.DownRight:
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.downRightItems, directionFromPlayer);
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
