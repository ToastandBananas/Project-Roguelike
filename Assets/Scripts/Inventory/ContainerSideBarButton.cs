using UnityEngine;
using UnityEngine.UI;

public class ContainerSideBarButton : MonoBehaviour
{
    public Direction directionFromPlayer;

    [HideInInspector] public Image icon;

    Image directionIcon;
    ContainerInventoryUI containerInvUI;

    Color highlightColor = new Color(0, 0.4f, 0.62f);

    void Start()
    {
        containerInvUI = ContainerInventoryUI.instance;
        icon = transform.GetChild(0).GetComponent<Image>();
        directionIcon = transform.GetChild(1).GetComponent<Image>();

        if (directionFromPlayer == Direction.Center)
            HighlightDirectionIcon();
    }

    public void ShowInventoryItems()
    {
        containerInvUI.ResetContainerIcons(directionFromPlayer);

        switch (directionFromPlayer)
        {
            case Direction.Center:
                containerInvUI.PopulateInventoryUI(containerInvUI.playerPositionItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Up:
                containerInvUI.PopulateInventoryUI(containerInvUI.upItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Down:
                containerInvUI.PopulateInventoryUI(containerInvUI.downItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Left:
                containerInvUI.PopulateInventoryUI(containerInvUI.leftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.Right:
                containerInvUI.PopulateInventoryUI(containerInvUI.rightItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.UpLeft:
                containerInvUI.PopulateInventoryUI(containerInvUI.upLeftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.UpRight:
                containerInvUI.PopulateInventoryUI(containerInvUI.upRightItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.DownLeft:
                containerInvUI.PopulateInventoryUI(containerInvUI.downLeftItems, directionFromPlayer);
                HighlightDirectionIcon();
                break;
            case Direction.DownRight:
                containerInvUI.PopulateInventoryUI(containerInvUI.downRightItems, directionFromPlayer);
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
}
