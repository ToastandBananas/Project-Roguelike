using UnityEngine;

public class InventoryTooltip : Tooltip
{
    public void BuildTooltip(ItemData itemData)
    {
        if (stringBuilder.ToString() != "")
            stringBuilder.Clear();

        // Item name
        stringBuilder.Append("<b><size=26>" + itemData.name + "</size></b>\n\n");

        // Description
        if (itemData.item.description != "")
            stringBuilder.Append(itemData.item.description + "\n\n");

        // TODO

        ShowTooltip();
    }
}
