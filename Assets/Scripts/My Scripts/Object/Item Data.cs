
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Store Item", menuName = "Game/Store Item")]
public class ItemData : ScriptableObject
{
    public string Name;

    public int price;

    public Sprite Icon;

    [TextArea]

    public String description;
}
