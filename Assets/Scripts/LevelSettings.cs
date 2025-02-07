using UnityEngine;

[CreateAssetMenu(fileName = "LevelSetting", menuName = "Scriptable Objects/LevelSetting")]
public class LevelSetting : ScriptableObject
{
    public int rows;
    public int columns;
    public int numberOfColors;
    public int conditionForGroupA;
    public int conditionForGroupB;
    public int conditionForGroupC;
    public float blockSizeHorizontal;
    public float verticalSpacing;

}
