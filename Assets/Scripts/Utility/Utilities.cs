using System.Text;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
    }

    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToRGBAColor(string hex)
    {
        hex = hex.Replace("0x", ""); // in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", ""); // in case the string is formatted #FFFFFF

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255; // assume fully visible unless specified in hex

        // Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public static string FormatStringIntoParagraph(string text, int maxCharactersPerLine)
    {
        string[] words = text.Split(" "[0]); // Split the string into seperate words
        string result = "";
        int charactersUnaccountedFor = 0;

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i].Trim();
            if (i == 0)
            {
                result = words[0];
            }
            else
            {
                result += " " + word;
                charactersUnaccountedFor += word.Length + 1;
            }

            if (charactersUnaccountedFor > maxCharactersPerLine)
            {
                result = result.Substring(0, result.Length - word.Length);
                result += "\n" + word;
                charactersUnaccountedFor = 0;
            }
        }

        return result;
    }

    public static string FormatEnumStringWithSpaces(string enumString)
    {
        if (string.IsNullOrWhiteSpace(enumString))
            return "";

        StringBuilder newText = new StringBuilder(enumString.Length * 2);
        newText.Append(enumString[0]);
        for (int i = 1; i < enumString.Length; i++)
        {
            if (char.IsUpper(enumString[i]) && enumString[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(enumString[i]);
        }
        return newText.ToString();
    }

    public static Vector2 ClampedPosition(Vector2 position)
    {
        if (position.x % 1f != 0)
            position = new Vector2(Mathf.RoundToInt(position.x), position.y);

        if (position.y % 1f != 0)
            position = new Vector2(position.x, Mathf.RoundToInt(position.y));

        return position;
    }

    public static string GetIndefiniteArticle(string noun, bool uppercase, bool returnNoun, string textColor = "#FFFFFF")
    {
        if ("aeiouAEIOU".IndexOf(noun.Substring(0, 1)) >= 0)
        {
            if (uppercase)
            {
                if (returnNoun)
                    return "An <b><color=" + textColor + ">" + noun + "</color></b>";
                return "An ";
            }

            if (returnNoun)
                return "an <b><color=" + textColor + ">" + noun + "</color></b>";
            return "an ";
        }
        else
        {
            if (uppercase)
            {
                if (returnNoun)
                    return "A <b><color=" + textColor + ">" + noun + "</color></b>";
                return "A ";
            }

            if (returnNoun)
                return "a <b><color=" + textColor + ">" + noun + "</color></b>";
            return "a ";
        }
    }
}
