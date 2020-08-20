using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class LevelControl : MonoBehaviour
{

    [Header("Inputs"),Range(0,2)]
    public int difficulty;
    [Range(0,1)]
    public float timeScale;
    public Vector2 vel;
    public Vector2 minVel;
    public float distance, gravity, clickRangeMin, clickRangeMax, angle, hitAngle, maxStartPower;
    [Tooltip("X = Angle, Y = Power")]
    public Vector2 startStat;
    bool chooseAngle, choosePower;
    [Range(0,90)]
    public float angleMeter;
    [Range(0,1)]
    public float powerMeter;
    public float angleSpeedEasy, angleSpeedNormal, angleSpeedHard, powerSpeedEasy, powerSpeedNormal, powerSpeedHard;
    public Vector2 groundDrag, aerialForceUp, aerialForceDown;
    [Min(0)]
    public int upForceCount;
    public int upForceCountEasy, upForceCountNormal, upForceCountHard;
    [Range(0,1)]
    public float downForcePercent, rechargeEasy, rechargeNormal, rechargeHard;
    bool gameOver, started;
    public bool[] specialReady;

    [Header("References")]
    public GameObject player;
    // WindZone wind;
    public GameObject[] npcSlider;
    int[] currentNPC;
    public Transform spawn, despawn;
    public AudioSource bgm, sfx;
    public AudioClip punchSFX, bounceSFX, missSFX;

    [Header("UI")]
    public Camera cam;
    public Vector2 camSizeRange, camRefRange;
    public Vector3 camPosMin, camPosMax;
    public TextMeshProUGUI recordText, currentText, velocityText, upForceText, downForceText, heightText, angleText;
    public GameObject arrow, startPanel;
    public Image startAngleArrow, startPowerMeter;
    public Image[] specialPortrait;

    void Start() {
        //Initialize
        // wind = player.GetComponentInChildren<WindZone>();
        specialReady = new bool[4];

        //Choose NPCs
        currentNPC = new int[5];
        for (int i = 0; i < 5; i++) {
            RespawnNPC(i);
        }
        
        if (difficulty == 0) upForceCount = upForceCountEasy;
        else if (difficulty == 1) upForceCount = upForceCountNormal;
        else upForceCount = upForceCountHard;

        //Choose Power & Angle
        powerMeter = 0f;
        angleMeter = 0f;
        chooseAngle = true;
        StartCoroutine(SetAngle());
    }

        void OnClick(InputValue val) {
        bool down = false;
        if (val.Get<float>() >= .5f) {
            down = true;
        }
        if (!started) {
            if (chooseAngle) {
                if (down) {
                    startStat.x = angleMeter;
                    StopCoroutine(SetAngle());
                    chooseAngle = false;
                    choosePower = true;
                    StartCoroutine(SetPower());
                }
            }
            else if (choosePower) {
                if (!down) {
                    startStat.y = powerMeter;
                    StopCoroutine(SetPower());
                    choosePower = false;
                    //X value is angle, Y value is power.
                    vel = Vector3.Slerp(Vector2.right, Vector2.up, startStat.x/90) * startStat.y * maxStartPower;
                    hitAngle = Vector2.Angle(Vector2.right, vel);
                    Debug.Log("Vel: " + vel + "\nAngle: " + hitAngle);
                    started = true;
                    startPanel.gameObject.SetActive(false);
                }
            }
        }
        else {
            if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax && down) {
                if (vel.y < 0 && upForceCount > 0) {
                    //Use aerial.
                    Bounce(aerialForceUp, Vector2.one);
                    upForceCount --;
                }
                else if (vel.y > 0 && downForcePercent == 1f){
                    //Use AED.
                    Bounce(aerialForceDown, new Vector2(1f, -1f));
                    downForcePercent = 0f;
                }
            }
        }
    }
    
    public void Contact(NPCInfo info) {
        StartCoroutine(ControlTime(.1f, .5f));
        //Deal with Reaction
        int index = info.charIndex;
        if (index <= 3 && specialReady[index]) {
            Bounce(info.specialForceAdd, info.specialForceMult);
        }
        else if (!info.phantom) {
            sfx.PlayOneShot(punchSFX);
            Bounce(info.forceAdd, info.forceMult);
        }
        if (info.charIndex == 5) {
            // vel = Vector3.InverseSlerp(vel, Vector2.one);
            vel = new Vector2(vel.y, vel.x);
            hitAngle = Vector2.Angle(Vector2.right, vel);
        }
        if (info.charIndex == 6) {
            float mag = vel.magnitude;
            vel = Vector3.Slerp(Vector2.right, Vector2.up, .5f) * mag;
            hitAngle = Vector2.Angle(Vector2.right, vel);
            // vel = new Vector2((vel.x + vel.y)/2f, (vel.x + vel.y)/2f);
        }
        Debug.Log("New Velocity: " + vel);

        //Control Specials
        if (info.charIndex == 0) {
            specialReady[0] = true;
            specialReady[1] = false;
            specialReady[2] = false;
            specialReady[3] = true;
        }
        if (info.charIndex == 1) {
            specialReady[0] = false;
            specialReady[1] = false;
            specialReady[2] = true;
            specialReady[3] = true;
        }
        if (info.charIndex == 2) {
            specialReady[0] = false;
            specialReady[1] = true;
            specialReady[2] = false;
            specialReady[3] = true;
        }
        if (info.charIndex == 3) {
            specialReady[0] = true;
            specialReady[1] = true;
            specialReady[2] = true;
            specialReady[3] = true;
        }
    }

    void Update() {
        if (!gameOver && started) {
            vel.y -= gravity*timeScale;
            player.transform.Translate(new Vector2(0, vel.y/60*timeScale));
            for (int i = 0; i < 5; i++)
            {
                npcSlider[i].transform.Translate(new Vector2(-vel.x/60*timeScale, 0));
                if (npcSlider[i].transform.position.x < despawn.position.x) {
                    RespawnNPC(i);
                }
            }
            if (player.transform.position.y <= 0) {
                player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
                vel = Vector2.Reflect(vel, Vector2.up);
                vel = vel * groundDrag;
                sfx.PlayOneShot(bounceSFX);
                specialReady[3] = false;
                if (vel.x < 0) vel.x = 0;
                if (vel.x < minVel.x && Mathf.Abs(vel.y) < minVel.y) {
                    GameOver();
                }
            }
            distance += vel.x/60*timeScale;
            if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax) {
                if (difficulty == 0) downForcePercent += rechargeEasy*timeScale;
                else if (difficulty == 1) downForcePercent += rechargeNormal*timeScale;
                else downForcePercent += rechargeHard*timeScale;
                if (downForcePercent >= 1) downForcePercent = 1;
            }
            // wind.windMain = vel.x;
        }
        currentText.text = "Current: " + System.Math.Round(distance,2) + "m";
        velocityText.text = System.Math.Round(vel.x,2) + "m/s";
        heightText.text = System.Math.Round(player.transform.position.y,2) + "m";
        upForceText.text = upForceCount.ToString();
        downForceText.text = System.Math.Round(downForcePercent*100) + "%";
        if (vel.y > 0) {
            angle = Vector2.Angle(Vector2.right, vel);
        }
        else {
            angle = -Vector2.Angle(Vector2.right, vel);
        }
        angleText.text = System.Math.Round(hitAngle,2) + "°";
        arrow.transform.SetPositionAndRotation(arrow.transform.position, Quaternion.Euler(0, 0, angle));
        startPowerMeter.fillAmount = powerMeter;
        startAngleArrow.gameObject.transform.SetPositionAndRotation(startAngleArrow.gameObject.transform.position, Quaternion.Euler(0, 0, angleMeter));

        for (int i = 0; i < 4; i++) {
            if (specialReady[i]) {
                specialPortrait[i].color = Color.white;
            }
            else specialPortrait[i].color = new Color(1,1,1,.5f);
        }
    }

    void LateUpdate() {
        if (player.transform.position.y >= camRefRange.y) {
            cam.orthographicSize = camSizeRange.y;
            cam.transform.SetPositionAndRotation(camPosMax, cam.transform.rotation);
        }
        else if (player.transform.position.y <= camRefRange.x) {
            cam.orthographicSize = camSizeRange.x;
            cam.transform.SetPositionAndRotation(camPosMin, cam.transform.rotation);
        }
        else {
            float interp = (player.transform.position.y-camRefRange.x)/(camRefRange.y-camRefRange.x);
            cam.orthographicSize = Mathf.Lerp(camSizeRange.x, camSizeRange.y, interp);
            cam.transform.SetPositionAndRotation(new Vector3(Mathf.Lerp(camPosMin.x, camPosMax.x, interp),Mathf.Lerp(camPosMin.y, camPosMax.y, interp), camPosMax.z), cam.transform.rotation);
        }
    }

    void Bounce(Vector2 forceToAdd, Vector2 forceToMult) {
        vel.x = (vel.x + forceToAdd.x) * forceToMult.x;
        if (vel.y < 0) {
            vel.y = (-vel.y + forceToAdd.y) * forceToMult.y;
        }
        else {
            vel.y = (vel.y + forceToAdd.y) * forceToMult.y;
        }
        hitAngle = Vector2.Angle(Vector2.right, vel);
    }

    void GameOver() {
        Debug.Log("Game Over");
        gameOver = true;
    }

    void RespawnNPC(int index) {
        npcSlider[index].transform.SetPositionAndRotation(new Vector2(npcSlider[index].transform.position.x + 50, 0), npcSlider[index].transform.rotation);
        int newIndex = 0;
        bool on = true;
        while (on) {
            newIndex = Random.Range(0,7);
            if (newIndex != currentNPC[0]) {
                if (newIndex != currentNPC[1]) {
                    if (newIndex != currentNPC[2]) {
                        if (newIndex != currentNPC[3]) {
                            if (newIndex != currentNPC[4]) {
                                on = false;
                            }
                        }
                    }
                }
            }
        }
        npcSlider[index].GetComponent<NPCInfo>().charIndex = newIndex;
        currentNPC[index] = newIndex;
        npcSlider[index].GetComponent<NPCInfo>().ResetChar();
    }



    IEnumerator SetAngle() {
        bool falling = false;
        while (chooseAngle) {
            while (!falling) {
                if (difficulty == 0) angleMeter += angleSpeedEasy;
                else if (difficulty == 1) angleMeter += angleSpeedNormal;
                else angleMeter += angleSpeedHard;
                if (angleMeter >= 90) {
                    angleMeter = 90;
                    falling = true;
                }
                yield return null;
            }
            while (falling) {
                if (difficulty == 0) angleMeter -= angleSpeedEasy;
                else if (difficulty == 1) angleMeter -= angleSpeedNormal;
                else angleMeter -= angleSpeedHard;
                if (angleMeter <= 0) {
                    angleMeter = 0;
                    falling = false;
                }
                yield return null;
            }
        }
    }

    IEnumerator ControlTime(float speed, float dura) {
        float oldScale = timeScale;
        timeScale = speed;
        yield return new WaitForSeconds(dura);
        timeScale = oldScale;
    }

    IEnumerator SetPower() {
        bool falling = false;
        while (choosePower) {
            while (!falling) {
                if (difficulty == 0) powerMeter += powerSpeedEasy;
                else if (difficulty == 1) powerMeter += powerSpeedNormal;
                else powerMeter += powerSpeedHard;
                if (powerMeter >= 1) {
                    powerMeter = 1;
                    falling = true;
                }
                yield return null;
            }
            while (falling) {
                if (difficulty == 0) powerMeter -= powerSpeedEasy;
                else if (difficulty == 1) powerMeter -= powerSpeedNormal;
                else powerMeter -= powerSpeedHard;
                if (powerMeter <= 0) {
                    powerMeter = 0;
                    falling = false;
                }
                yield return null;
            }
        }
    }
}
