using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSlider : MonoBehaviour
{
    public CharacterInfo[] allNPCs;
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
        charName            = allNPCs[charIndex].charName;
        forceAdd            = allNPCs[charIndex].forceAdd;
        forceMult           = allNPCs[charIndex].forceMult;
        specialForceAdd     = allNPCs[charIndex].specialForceAdd;
        specialForceMult    = allNPCs[charIndex].specialForceMult;
        timeSpeed           = allNPCs[charIndex].timeSpeed;
        timeDura            = allNPCs[charIndex].timeDura;
        timeSpeedSpecial    = allNPCs[charIndex].timeSpeedSpecial;
        timeDuraSpecial     = allNPCs[charIndex].timeDuraSpecial;
        phantom             = allNPCs[charIndex].phantom;
        charSprite          = allNPCs[charIndex].charSprite;
        sprRend.color       = allNPCs[charIndex].charColor; //Remove when sprites are finalized.
        charSpriteHead      = allNPCs[charIndex].charSpriteHead;
    }
}
