using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    NPCSlider info;
    [SerializeField] LevelControl levelController;

    void OnTriggerEnter2D(Collider2D other)
    {
        info = other.GetComponentInParent<NPCSlider>();
        // Debug.Log("Hit " + info.charName + "\n" + info.forceAdd + "\n" + info.forceMult);
        levelController.Contact(info);
    }
}
