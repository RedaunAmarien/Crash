using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInfo : MonoBehaviour
{
    public Vector2 forceAdd, forceMult, specialForceAdd, specialForceMult;
    public bool phantom;
    public string charName;
    public Sprite charSprite;
    public Sprite[] charSpriteList;
    public Sprite charSpriteHead;
    public Sprite[] charSpriteHeadList;
    SpriteRenderer sprRend;
    public int charIndex;
    public Color charColor;

    void Start() {
        sprRend = GetComponent<SpriteRenderer>();
    }

    public void ResetChar() {
        switch (charIndex) {
            case 0:
                charName = "Brittany";
                forceAdd = new Vector2(10, 5);
                forceMult = Vector2.one;
                specialForceAdd = new Vector2(5, 2);
                specialForceMult = new Vector2(2, 1.2f);
                charColor = Color.red;
                phantom = false;
            break;
            case 1:
                charName = "Susan";
                forceAdd = new Vector2(5, 10);
                forceMult = Vector2.one;
                specialForceAdd = new Vector2(2, 5);
                specialForceMult = new Vector2(1.2f, 2);
                charColor = Color.yellow;
                phantom = false;
            break;
            case 2:
                charName = "Sarah";
                forceAdd = new Vector2(10, 10);
                forceMult = Vector2.one;
                specialForceAdd = new Vector2(3, 3);
                specialForceMult = new Vector2(1.5f, 1.5f);
                charColor = Color.cyan;
                phantom = false;
            break;
            case 3:
                charName = "Patricia";
                forceAdd = Vector2.zero;
                forceMult = new Vector2(.2f, .2f);
                specialForceAdd = new Vector2(10, 10);
                specialForceMult = new Vector2(2, 2);
                charColor = Color.green;
                phantom = false;
            break;
            case 4:
                charName = "Max";
                forceAdd = Vector2.zero;
                forceMult = new Vector2(.6f, .6f);
                charColor = Color.blue;
                phantom = false;
            break;
            case 5:
                charName = "Jim";
                forceAdd = Vector2.zero;
                forceMult = Vector2.one;
                charColor = new Color(0.5f, 0.5f, 0);
                phantom = false;
            break;
            case 6:
                charName = "Nat";
                forceAdd = Vector2.zero;
                forceMult = Vector2.one;
                charColor = Color.gray;
                phantom = false;
            break;
            case 7:
                charName = "Melanie";
                phantom = true;
                charColor = Color.magenta;
            break;
            default:
                Debug.LogError("Character index outside of defined character reactions.");
            break;
        }
        sprRend.color = charColor;
        charSpriteHead = charSpriteHeadList[charIndex];
    }
}
