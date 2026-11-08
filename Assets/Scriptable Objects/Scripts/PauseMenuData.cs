using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "PauseMenuData", menuName = "Scriptable Objects/UI/PauseMenuData")]
public class PauseMenuData : ScriptableObject
{
    public Visibility visibility = Visibility.Hidden;
}
