using UnityEngine;

[CreateAssetMenu(fileName = "PopupData", menuName = "Scriptable Objects/UI/PopupData")]
public class PopupData : ScriptableObject
{
    public string popupTitle;
    public Sprite popupImage;
    [TextArea(5, 10)]
    public string popupText;
}
