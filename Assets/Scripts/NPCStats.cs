using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStats : MonoBehaviour
{
    public string charName;
    public Vector2 forceAdd, forceMult, specialForceAdd, specialForceMult;
    public float timeSpeed, timeDura, timeSpeedSpecial, timeDuraSpecial;
    public bool phantom;
    public Sprite charSprite;
    public Sprite charSpriteHead;
    public Color charColor;
    public float charWeight;
}
