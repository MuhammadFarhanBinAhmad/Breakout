using UnityEngine;

public class GroupUnderAttribute : PropertyAttribute
{
    public string BoolField;

    public GroupUnderAttribute(string boolField)
    {
        BoolField = boolField;
    }
}
