using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu()]
public class CharacterInfo : ScriptableObject
{
    public string charName;
    public Vector2 forceAdd;
    public Vector2 forceMult = Vector2.one;
    public Vector2 specialForceAdd;
    public Vector2 specialForceMult = Vector2.one;
    public float timeSpeed = 0.05f;
    public float timeDura = 0.5f;
    public float timeSpeedSpecial;
    public float timeDuraSpecial = 0.75f;
    public bool phantom;
    public Sprite charSprite;
    public Sprite charSpriteHead;
    public Color charColor;
    public float charWeight = 10;
}
