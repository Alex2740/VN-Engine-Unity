using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Choice : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textBox;
    public int jump;
    public string text;

    public void UpdateData(string newText, int newJump) {
        text = newText;
        jump = newJump;

        _textBox.text = text;
    }

    public void Jump() {
        VNController.instance.OpenDialogue(jump);
    }
}
