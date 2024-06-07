using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ArrayHolder : ScriptableObject
{
    public bool isFilled;
    public List<txtReader.message> constantMessagesList = new List<txtReader.message>();
}
