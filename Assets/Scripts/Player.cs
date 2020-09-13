using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    NPCSlider info;
    public GameObject levelController;

    void OnTriggerEnter2D(Collider2D other) {
        info = other.GetComponent<NPCSlider>();
        // Debug.Log("Hit " + info.charName + "\n" + info.forceAdd + "\n" + info.forceMult);
        levelController.GetComponent<LevelControl>().Contact(info);
    }
}
