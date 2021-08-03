using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    StringBuilder stringBuilder = new StringBuilder();

    readonly string blueHexColor = "#005BEA";
    readonly string greenHexColor = "#00BE06";
    readonly string yellowHexColor = "#FFE109";
    readonly string orangeHexColor = "#FF9800";
    readonly string redHexColor = "#FF5139";
    readonly string darkRedHexColor = "#C30600";

    Vector2 lastPositionChecked, mouseWorldPos;

    GameManager gm;

    #region
    public static TileInfoDisplay instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of TileInfo. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        gm = GameManager.instance;
    }

    void FixedUpdate()
    {
        mouseWorldPos = Utilities.ClampedPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (EventSystem.current.IsPointerOverGameObject() == false && lastPositionChecked != mouseWorldPos)
            DisplayTileInfo();
    }

    public void DisplayTileInfo()
    {
        lastPositionChecked = mouseWorldPos;
        GameTiles.objects.TryGetValue(mouseWorldPos, out GameObject worldObject);
        GameTiles.npcs.TryGetValue(mouseWorldPos, out CharacterManager npc);
        GameTiles.itemDatas.TryGetValue(mouseWorldPos, out List<ItemData> list);

        stringBuilder.Clear();

        // Display the tile's type
        if (GameTiles.GetTileFromWorldPosition(mouseWorldPos, GameTiles.shallowWaterTiles) != null)
            stringBuilder.Append("<b>Shallow Water</b>\n\n");
        else if (GameTiles.GetTileFromWorldPosition(mouseWorldPos, GameTiles.groundTiles) != null)
            stringBuilder.Append("<b>Ground</b>\n\n");

        if (Vector2.Distance(mouseWorldPos, gm.playerManager.transform.position) > gm.playerManager.vision.lookRadius)
            stringBuilder.Append("You can't see that far...");
        else if (npc == null && worldObject == null && (list == null || list.Count == 0))
            stringBuilder.Append("You don't see anything here...");
        else
        {
            // If there's an object on this tile, show its name
            if (worldObject != null)
            {
                stringBuilder.Append("You see " + Utilities.GetIndefiniteArticle(worldObject.name, false, true) + ".\n\n");
            }

            // If there's an NPC on this tile, show its name
            if (npc != null)
            {
                stringBuilder.Append("You see ");

                if (npc.isNamed)
                    stringBuilder.Append("<b><color=" + GetNameColor(npc) + ">" + npc.name + " </color></b>.\n\n");
                else
                    stringBuilder.Append(Utilities.GetIndefiniteArticle(npc.name, false, true, GetNameColor(npc)) + ".\n\n");
            }

            // If there are any items at this position, show their names
            if (list != null)
            {
                if (list.Count > 1)
                    stringBuilder.Append("You see some items on the ground:\n");
                else
                    stringBuilder.Append("You see ");

                for (int i = 0; i < list.Count; i++)
                {
                    if ((npc != null && i >= 5) || i >= 7)
                    {
                        stringBuilder.Append("And more...");
                        break;
                    }

                    if (list.Count > 1)
                        stringBuilder.Append("- ");
                    else
                    {
                        if (list[i].item.IsWearable())
                        {
                            Wearable wearable = (Wearable)list[i].item;
                            if (wearable.equipmentSlot == EquipmentSlot.BodyArmor || wearable.equipmentSlot == EquipmentSlot.LegArmor 
                                || wearable.equipmentSlot == EquipmentSlot.Gloves || wearable.equipmentSlot == EquipmentSlot.Boots)
                                stringBuilder.Append("<b>");
                            else
                                stringBuilder.Append(Utilities.GetIndefiniteArticle(list[i].itemName, false, false) + "<b>");
                        }
                        else if (list[i].currentStackSize == 1)
                            stringBuilder.Append(Utilities.GetIndefiniteArticle(list[i].itemName, false, false) + "<b>");
                        else
                            stringBuilder.Append("<b>");
                    }

                    if (list[i].currentStackSize > 1)
                    {
                        stringBuilder.Append(list[i].currentStackSize + " ");

                        if (list[i].item.pluralName != "")
                            stringBuilder.Append(list[i].item.pluralName);
                        else
                            stringBuilder.Append(list[i].itemName + "s");
                    }
                    else
                        stringBuilder.Append(list[i].itemName);

                    if (list.Count > 1)
                        stringBuilder.Append("\n");
                }

                if (list.Count == 1)
                    stringBuilder.Append("</b> here.");
            }
        }

        displayText.text = stringBuilder.ToString();
    }

    string GetNameColor(CharacterManager npc)
    {
        if (npc.alliances.allies.Contains(Factions.Player))
            return blueHexColor;
        else if (npc.alliances.enemies.Contains(Factions.Player))
        {
            if ((float)npc.characterStats.maxHealth.GetValue() / (float)gm.playerManager.playerStats.maxHealth.GetValue() < 0.2f)
                return greenHexColor;
            else if ((float)npc.characterStats.maxHealth.GetValue() / (float)gm.playerManager.playerStats.maxHealth.GetValue() < 0.55f)
                return yellowHexColor;
            else if ((float)npc.characterStats.maxHealth.GetValue() / (float)gm.playerManager.playerStats.maxHealth.GetValue() < 1f)
                return orangeHexColor;
            else if ((float)npc.characterStats.maxHealth.GetValue() / (float)gm.playerManager.playerStats.maxHealth.GetValue() < 1.55f)
                return redHexColor;
            else
                return darkRedHexColor;
        }

        return "#FFFFFF";
    }
}
