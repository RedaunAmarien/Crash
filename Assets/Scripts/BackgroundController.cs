using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] GameObject[] layerOne;
    [SerializeField] GameObject[] layerTwo;
    [SerializeField] GameObject[] layerThree;
    [SerializeField] GameObject[] layerFour;
    [SerializeField] GameObject[] layerFive;
    public float[] bgSpeedMult;
    LevelControl levelControl;

    private void Start()
    {
        levelControl = GameObject.Find("Level Control").GetComponent<LevelControl>();
    }

    void Update()
    {
        //Move Environment
        for (int i = 0; i < layerOne.Length; i++)
        {
            layerOne[i].transform.Translate(new Vector2(-levelControl.vel.x / 60 * levelControl.timeScale * Time.deltaTime * bgSpeedMult[0], 0));
            if (layerOne[i].transform.position.x < -18.5f)
            {
                layerOne[i].transform.Translate(new Vector2(40, 0));
            }

            layerTwo[i].transform.Translate(new Vector2(-levelControl.vel.x / 60 * levelControl.timeScale * Time.deltaTime * bgSpeedMult[1], 0));
            if (layerTwo[i].transform.position.x < -18.5f)
            {
                layerTwo[i].transform.Translate(new Vector2(40, 0));
            }

            layerThree[i].transform.Translate(new Vector2(-levelControl.vel.x / 60 * levelControl.timeScale * Time.deltaTime * bgSpeedMult[2], 0));
            if (layerThree[i].transform.position.x < -18.5f)
            {
                layerThree[i].transform.Translate(new Vector2(40, 0));
            }

            layerFour[i].transform.Translate(new Vector2(-levelControl.vel.x / 60 * levelControl.timeScale * Time.deltaTime * bgSpeedMult[3], 0));
            if (layerFour[i].transform.position.x < -18.5f)
            {
                layerFour[i].transform.Translate(new Vector2(40, 0));
            }

            layerFive[i].transform.Translate(new Vector2(-levelControl.vel.x / 60 * levelControl.timeScale * Time.deltaTime * bgSpeedMult[4], 0));
            if (layerFive[i].transform.position.x < -18.5f)
            {
                layerFive[i].transform.Translate(new Vector2(40, 0));
            }
        }
    }
}
