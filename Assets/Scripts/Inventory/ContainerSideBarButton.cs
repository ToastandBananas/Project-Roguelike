using UnityEngine;

public class ContainerSideBarButton : MonoBehaviour
{
    public Direction directionFromPlayer;

    ContainerInventoryUI containerInvUI;

    void Start()
    {
        containerInvUI = ContainerInventoryUI.instance;
    }

    public void ShowInventoryItems()
    {
        switch (directionFromPlayer)
        {
            case Direction.Center:
                containerInvUI.PopulateInventoryUI(containerInvUI.playerPositionItems, directionFromPlayer);
                break;
            case Direction.Up:
                containerInvUI.PopulateInventoryUI(containerInvUI.upItems, directionFromPlayer);
                break;
            case Direction.Down:
                containerInvUI.PopulateInventoryUI(containerInvUI.downItems, directionFromPlayer);
                break;
            case Direction.Left:
                containerInvUI.PopulateInventoryUI(containerInvUI.leftItems, directionFromPlayer);
                break;
            case Direction.Right:
                containerInvUI.PopulateInventoryUI(containerInvUI.rightItems, directionFromPlayer);
                break;
            case Direction.UpLeft:
                containerInvUI.PopulateInventoryUI(containerInvUI.upLeftItems, directionFromPlayer);
                break;
            case Direction.UpRight:
                containerInvUI.PopulateInventoryUI(containerInvUI.upRightItems, directionFromPlayer);
                break;
            case Direction.DownLeft:
                containerInvUI.PopulateInventoryUI(containerInvUI.downLeftItems, directionFromPlayer);
                break;
            case Direction.DownRight:
                containerInvUI.PopulateInventoryUI(containerInvUI.downRightItems, directionFromPlayer);
                break;
            default:
                break;
        }
    }
}
