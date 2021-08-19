using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    public CharacterManager focusedCharacter;
    public GameObject focusedObject;
    public List<ItemData> focusedItems = new List<ItemData>();

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

        GameTiles.itemDatas.TryGetValue(mouseWorldPos, out List<ItemData> list);
        if (list != null)
            focusedItems = new List<ItemData>(list);
        else
            focusedItems.Clear();

        stringBuilder.Clear();

        // Display the tile's type
        if (GameTiles.GetTileFromWorldPosition(mouseWorldPos, GameTiles.shallowWaterTiles) != null)
            stringBuilder.Append("<b>Shallow Water</b>\n\n");
        else if (GameTiles.GetTileFromWorldPosition(mouseWorldPos, GameTiles.groundTiles) != null)
            stringBuilder.Append("<b>Ground</b>\n\n");

        if (Vector2.Distance(mouseWorldPos, gm.playerManager.transform.position) > gm.playerManager.vision.lookRadius)
            stringBuilder.Append("You can't see that far...");
        else if (focusedCharacter == null && focusedObject == null && (list == null || list.Count == 0))
            stringBuilder.Append("You don't see anything here...");
        else
        {
            // If there's an object on this tile, show its name
            if (focusedObject != null)
            {
                stringBuilder.Append("You see " + Utilities.GetIndefiniteArticle(focusedObject.name, false, true) + ".\n\n");
            }

            // If there's a character on this tile, show their name
            if (focusedCharacter != null)
            {
                stringBuilder.Append("You see ");

                if (focusedCharacter.isNPC == false) // If this is the player
                    stringBuilder.Append("yourself.");
                else if (focusedCharacter.isNamed)
                    stringBuilder.Append("<b><color=" + GetNameColor(focusedCharacter) + ">" + focusedCharacter.name + " </color></b>.\n\n");
                else
                    stringBuilder.Append(Utilities.GetIndefiniteArticle(focusedCharacter.name, false, true, GetNameColor(focusedCharacter)) + ".\n\n");
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
                    if ((focusedCharacter != null && i >= 5) || i >= 7)
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
            float npcMaxTorsoHealth = npc.status.GetBodyPart(BodyPartType.Torso).maxHealth.GetValue();
            float playerMaxTorsoHealth = gm.playerManager.status.GetBodyPart(BodyPartType.Torso).maxHealth.GetValue();
            if (npcMaxTorsoHealth / playerMaxTorsoHealth < 0.2f)
                return greenHexColor;
            else if (npcMaxTorsoHealth / playerMaxTorsoHealth < 0.55f)
                return yellowHexColor;
            else if (npcMaxTorsoHealth / playerMaxTorsoHealth < 1f)
                return orangeHexColor;
            else if (npcMaxTorsoHealth / playerMaxTorsoHealth < 1.55f)
                return redHexColor;
            else
                return darkRedHexColor;
        }

        return "#FFFFFF";
    }
}
