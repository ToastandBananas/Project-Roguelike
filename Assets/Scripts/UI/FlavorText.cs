using System;
using System.Text;
using System.Linq;
using UnityEngine;
using TMPro;

public class FlavorText : MonoBehaviour
{
    public TextMeshProUGUI flavorText;

    StringBuilder stringBuilder = new StringBuilder();

    int index;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            WriteLine("This is a new line of text! I'm making it really long so that it goes onto the next line. Is this long enough?" + index);
            index++;
            //stringBuilder.Append("This is a new line of text!\n");
            //flavorText.text = stringBuilder.ToString();
        }

        /*if (Input.GetKeyDown(KeyCode.P))
        {
            //stringBuilder.Clear();
            //string newString = RemoveFirstLine(stringBuilder.ToString());
            stringBuilder.Clear();
            stringBuilder.Append(newString);
            flavorText.text = stringBuilder.ToString();
        }*/
    }

    public void WriteLine(string input)
    {
        int numLines = Convert.ToString(stringBuilder).Split('\n').Length;
        if (numLines > 20)      //Max Lines
        {
            stringBuilder.Remove(0, Convert.ToString(stringBuilder).Split('\n').FirstOrDefault().Length + 1);
        }
        stringBuilder.Append(input + "\r\n");
        flavorText.text = Convert.ToString(stringBuilder);
    }
}
