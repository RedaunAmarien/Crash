using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCRoulette : MonoBehaviour {

    public GameObject[] npcSlider;
    public int[] npcOrder;
    public GameObject[] rouletteImage;
    public Sprite[] charSprite;
    public Transform despawn;
    [Range(0,1)]
    public float tailLength;

    void Start() {
        npcOrder = new int[5];
    }

    void Update() {
        for (int i = 0; i < 5; i++) {
            float pos = npcSlider[i].transform.position.x;
            if (pos < despawn.position.x + 10) {
                npcOrder[3] = i;
            }
            else if (pos < despawn.position.x + 20) {
                npcOrder[4] = i;
            }
            else if (pos < despawn.position.x + 30) {
                npcOrder[0] = i;
            }
            else if (pos < despawn.position.x + 40) {
                npcOrder[1] = i;
            }
            else if (pos < despawn.position.x + 50) {
                npcOrder[2] = i;
            }
            else npcOrder[3] = i;
        }
        for (int i = 0; i < 3; i++) {
            rouletteImage[i].GetComponent<Image>().sprite = npcSlider[npcOrder[i]].GetComponent<NPCInfo>().charSpriteHead;
            rouletteImage[i].GetComponent<LineRenderer>().SetPosition(0, rouletteImage[i].transform.position);
            rouletteImage[i].GetComponent<LineRenderer>().SetPosition(1, Vector3.Lerp(rouletteImage[i].transform.position, npcSlider[npcOrder[i]].transform.position, tailLength));
        }
    }
}
