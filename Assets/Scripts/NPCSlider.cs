using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSlider : MonoBehaviour
{
    public NPCStats[] allNPC;
    public int charIndex;
    public string charName;
    public Vector2 forceAdd, forceMult, specialForceAdd, specialForceMult;
    public float timeSpeed, timeDura, timeSpeedSpecial, timeDuraSpecial;
    public bool phantom;
    public Sprite charSprite;
    public Sprite charSpriteHead;
    SpriteRenderer sprRend;
    public Color charColor;

    void Start() {
        sprRend = GetComponent<SpriteRenderer>();
        ResetChar();
    }

    public void ResetChar() {
        charName            = allNPC[charIndex].charName;
        forceAdd            = allNPC[charIndex].forceAdd;
        forceMult           = allNPC[charIndex].forceMult;
        specialForceAdd     = allNPC[charIndex].specialForceAdd;
        specialForceMult    = allNPC[charIndex].specialForceMult;
        timeSpeed           = allNPC[charIndex].timeSpeed;
        timeDura            = allNPC[charIndex].timeDura;
        timeSpeedSpecial    = allNPC[charIndex].timeSpeedSpecial;
        timeDuraSpecial     = allNPC[charIndex].timeDuraSpecial;
        phantom             = allNPC[charIndex].phantom;
        charSprite          = allNPC[charIndex].charSprite;
        sprRend.color       = allNPC[charIndex].charColor; //Remove when sprites are finalized.
        charSpriteHead      = allNPC[charIndex].charSpriteHead;
    }
}
