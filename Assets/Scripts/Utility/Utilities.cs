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

    public static string FormatStringIntoParagraphs(string text, int maxCharactersPerLine)
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
}
