using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TileInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    StringBuilder stringBuilder = new StringBuilder();

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
        DisplayTileInfo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    public void DisplayTileInfo(Vector2 position)
    {
        position = Utilities.ClampedPosition(position);
        gm.gameTiles.npcs.TryGetValue(position, out CharacterManager npc);
        gm.gameTiles.itemDatas.TryGetValue(position, out List<ItemData> list);

        stringBuilder.Clear();

        if (Vector2.Distance(position, gm.playerManager.transform.position) > gm.playerManager.vision.lookRadius)
            stringBuilder.Append("You can't see that far...");
        else if (npc == null && (list == null || list.Count == 0))
            stringBuilder.Append("You don't see anything here...");
        else
        {
            // If there's an NPC on this tile, show its name
            if (npc != null)
            {
                stringBuilder.Append("You see ");

                if (npc.isNamed)
                    stringBuilder.Append(npc.name + ".\n\n");
                else
                    stringBuilder.Append(Utilities.GetIndefiniteArticle(npc.name, false) + ".\n\n");
            }

            // If there are any items at this position, show their names
            if (list != null)
            {
                // Header
                stringBuilder.Append("There are some items on the ground:\n");

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].currentStackSize > 1)
                    {
                        stringBuilder.Append("- " + list[i].currentStackSize + " ");

                        if (list[i].item.pluralName != "")
                            stringBuilder.Append(list[i].item.pluralName + "\n");
                        else
                            stringBuilder.Append(list[i].itemName + "s\n");
                    }
                    else
                        stringBuilder.Append("- " + list[i].itemName + "\n");

                }
            }
        }

        displayText.text = stringBuilder.ToString();
    }
}
