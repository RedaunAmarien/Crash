using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class LevelControl : MonoBehaviour
{
    [Header("Physics")]
    public Vector2 vel;
    public Vector2 minVel, groundDrag, aerialForceUp, aerialForceDown;
    public float minSqrMag, distance, gravity, clickRangeMin, clickRangeMax, angle, hitAngle, maxStartPower, distToBlockSp, topSpeed;

    [Header("Inputs"), Range(0, 1)]
    public float timeScale;
    [Range(0, 90)]
    public float angleMeter;
    [Range(0, 1)]
    public float powerMeter;
    [Tooltip("X = Angle, Y = Power")]
    public Vector2 startStat, custom;
    [Min(0)]
    public int glides, upForceCount;
    [Range(0, 1)]
    public float downForcePercent;
    public float specialTimer;
    [Range(-1, 2)]
    public int difficulty;

    [Header("Inputs: Easy")]
    public float angleSpeedEasy;
    public float powerSpeedEasy, rechargeEasy, specialTimeEasy;
    public int upForceCountEasy;

    [Header("Inputs: Normal")]
    public float angleSpeedNormal;
    public float powerSpeedNormal, rechargeNormal, specialTimeNormal;
    public int upForceCountNormal;

    [Header("Inputs: Hard")]
    public float angleSpeedHard;
    public float powerSpeedHard, rechargeHard, specialTimeHard;
    public int upForceCountHard;

    [Header("Flags & Triggers")]
    public bool started = false;
    public bool gameOver = false, chooseAngle, choosePower, blocking, blockSpecialReady;
    public bool[] specialReady;
    public int lastHitChar, perfectVolleys, cloneCount;
    public bool multiballReady, chanceOn;
    
    // [Header("Fake Save File (Remove when implemented)")] (Deprecated)
    // public float prevRecordDist;
    // public float prevRecordHeight;
    // public float prevRecordSpeed;

    [Header("Main References")]
    public GameObject player;
    // WindZone wind;
    public GameObject[] sliders;
    public List<int> currentNPC = new List<int>();
    public GameObject[] bg1, bg2, bg3, bg4, bg5;
    public float[] bgSpeedMult;
    public Transform spawn, despawn;

    [Header("Audio")]
    public AudioSource[] bgm;
    public AudioSource sfx;
    public AudioClip punchSFX, specialPunchSFX, bounceSFX, missSFX;
    public AudioClip[] tennisSFX;
    public AudioClip mainMenuBGM, altGameBGM, gameOverBGM;
    public AudioClip[] gameBGM;
    double songTime, nextSongTime;
    bool musicPlaying;
    int flip, musicIndex;

    [Header("Play UI")]
    public Camera cam;
    public Vector2 camSizeRange, camRefRange;
    public Vector3 camPosMin, camPosMax;
    public TextMeshProUGUI recordText, currentText, velocityText, upForceText, downForceText, glidesText, heightText, angleText;
    public GameObject arrow, flyingSet, blockImage, blockSpecialImage;
    public Image[] specialPortrait;

    [Header("Start UI")]
    public GameObject startPanel;
    public Image startAngleArrow, startPowerMeter;
    public TextMeshProUGUI startAngleText, startPowerText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public GameObject gameOverGraph, gameOverLine;
    public TextMeshProUGUI goRecordDist, goRecordHeight, goRecordSpeed, gameOverMaxHeight, gameOverMaxDist;
    public float routeTime;
    public List<Vector3> routeHistory = new List<Vector3>();

    [Header("Internal")]
    NPCSlider basicSlider;
    IntRange[] newRange;

    void Start() {
        //Initialize
        specialReady = new bool[4];
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        musicPlaying = false;

        //Load Save
        SaveData.current = (SaveData)FileManager.LoadFile(Application.persistentDataPath + "/saves/dummy.sv");
        if (SaveData.current == null) {
            FileManager.SaveFile("dummy", SaveData.current);
        }

        //Fake Save (Deprecated)
        // SaveData.current.recordDist = prevRecordDist;
        // SaveData.current.recordHeight = prevRecordHeight;
        // SaveData.current.recordSpeed = prevRecordSpeed;

        //Get reference list.
        basicSlider = sliders[0].GetComponent<NPCSlider>();
        //Measure reference list.
        newRange = new IntRange[basicSlider.allNPC.Length];

        //Use reference list to make new probability range.
        for (int j = 0; j < newRange.Length; j++) {
            newRange[j] = new IntRange(j, j+1, basicSlider.allNPC[j].charWeight);
        }

        //Use new probability range to choose NPCs.
        for (int i = 0; i < 5; i++) {
            int nextNum = RandomRange.Range(newRange)-1;
            //Do not repeat NPCs if already on screen.
            while (currentNPC.Contains(nextNum)) {
                nextNum = RandomRange.Range(newRange)-1;
            }
            currentNPC.Add(nextNum);
            sliders[i].GetComponent<NPCSlider>().charIndex = nextNum;
            RespawnNPC(i);
        }

        //===> Insert Main Menu stuff HERE <===
        
        if (difficulty == 0) {
            upForceCount = upForceCountEasy;
            specialTimer = specialTimeEasy;
        }
        else if (difficulty == 1) {
            upForceCount = upForceCountNormal;
            specialTimer = specialTimeNormal;
        }
        else {
            upForceCount = upForceCountHard;
            specialTimer = specialTimeHard;
        }

        //Choose Power & Angle
        powerMeter = 0f;
        angleMeter = 0f;
        chooseAngle = true;
        StartCoroutine(SetAngle());
    }

    void OnClick(InputValue val) {
        if (!started) {
            if (chooseAngle) {
                if (val.Get<float>() >= .5f) {
                    startStat.x = angleMeter;
                    startAngleText.text = System.Math.Round(startStat.x, 1).ToString();
                    chooseAngle = false;
                    choosePower = true;
                    StartCoroutine(SetPower());
                }
            }
            else if (choosePower) {
                if (val.Get<float>() < .5f) {
                    startStat.y = powerMeter;
                    startPowerText.text = System.Math.Round(startStat.y, 1).ToString();
                    choosePower = false;
                    //X value is angle, Y value is power.
                    vel = Vector3.Slerp(Vector2.right, Vector2.up, startStat.x/90) * startStat.y * maxStartPower;
                    hitAngle = Vector2.Angle(Vector2.right, vel);
                    Debug.Log("Vel: " + vel + "\nAngle: " + hitAngle);
                    started = true;
                    startPanel.gameObject.SetActive(false);
                    routeHistory.Add(Vector3.zero);
                    StartCoroutine(RouteTimer());
                    //Start Music
                    musicPlaying = true;
                    songTime = AudioSettings.dspTime;
                    nextSongTime = songTime + gameBGM[0].length;
                }
            }
        }
        else {
            if (chanceOn) {
                SpecialActivate(0);
            }
            if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax && val.Get<float>() >= .5f) {
                if (vel.y < 0 && upForceCount > 0) {
                    //Use aerial.
                    Bounce(aerialForceUp, Vector2.one);
                    upForceCount --;
                }
                else if (vel.y > 0 && downForcePercent == 1f){
                    //Use AED.
                    Bounce(aerialForceDown, new Vector2(1f, -1f));
                    downForcePercent = 0;
                }
            }
        }
    }
    
    public void Contact(NPCSlider info) {
        if (started & !gameOver) {
            int index = info.charIndex;
            
            if (!blocking) {
                
                //Bulk Character Settings
                if (index <= 3 && specialReady[index]) {
                    if (multiballReady) {
                        multiballReady = false;
                        cloneCount = 0;
                        Debug.LogError("Multiball is not set up.");
                    }
                    else {
                        sfx.PlayOneShot(specialPunchSFX);
                        StartCoroutine(ControlTime(info.timeSpeedSpecial, info.timeDuraSpecial));
                        Bounce(info.specialForceAdd, info.specialForceMult);
                    }
                }
                else if (!info.phantom && index != 4) {
                    sfx.PlayOneShot(punchSFX);
                    Bounce(info.forceAdd, info.forceMult);
                    StartCoroutine(ControlTime(info.timeSpeed, info.timeDura));
                }
                else {
                    //Handle Separately
                }

                if (index != 1 && index != 2) {
                    perfectVolleys = 0;
                }

                //Unique Character Settings
                switch (index) {
                    case 0:
                        //Redaun
                        specialReady[0] = true;
                        specialReady[1] = false;
                        specialReady[2] = false;
                        specialReady[3] = true;
                    break;
                    case 1:
                        //Taco
                        specialReady[0] = false;
                        specialReady[1] = false;
                        specialReady[2] = true;
                        specialReady[3] = true;
                        if (lastHitChar == 2) {
                            perfectVolleys += 1;
                            if (perfectVolleys > 1) sfx.PlayOneShot(tennisSFX[Random.Range(0, tennisSFX.Length)]); 
                        }
                    break;
                    case 2:
                        //Lex
                        specialReady[0] = false;
                        specialReady[1] = true;
                        specialReady[2] = false;
                        specialReady[3] = true;
                        if (lastHitChar == 1) {
                            perfectVolleys += 1;
                            if (perfectVolleys > 1) sfx.PlayOneShot(tennisSFX[Random.Range(0, tennisSFX.Length)]); 
                        }
                    break;
                    case 3:
                        //Masquerade
                        specialReady[0] = true;
                        specialReady[1] = true;
                        specialReady[2] = true;
                        specialReady[3] = true;
                    break;
                    case 4:
                        //Max
                        if (glides >= 1) {
                            glides = 0;
                            Bounce(info.specialForceAdd, info.specialForceMult);
                            sfx.PlayOneShot(missSFX);
                        }
                        else {
                            Bounce(info.forceAdd, info.forceMult);
                            sfx.PlayOneShot(punchSFX);
                        }
                    break;
                    case 5:
                        //Llanga
                        vel = new Vector2(vel.y, vel.x);
                        hitAngle = Vector2.Angle(Vector2.right, vel);
                    break;
                    case 6:
                        //Greed
                        float mag = vel.magnitude;
                        vel = Vector3.Slerp(Vector2.right, Vector2.up, .5f) * mag;
                        hitAngle = Vector2.Angle(Vector2.right, vel);
                    break;
                    case 7:
                        //Old Gula
                        sfx.PlayOneShot(missSFX);
                        StartCoroutine(ControlTime(info.timeSpeed, info.timeDura));
                        blocking = true;
                        distToBlockSp = (Random.Range(4,10) * 10) + 1;
                    break;
                    case 8:
                        //Ariston
                        upForceCount += 2;
                        specialReady[3] = true;
                    break;
                    case 9:
                        //Sintel
                        glides += 2;
                        specialReady[3] = true;
                    break;
                    case 10:
                        //Creepo
                        specialReady[0] = false;
                        specialReady[1] = false;
                        specialReady[2] = false;
                        specialReady[3] = false;
                    break;
                    case 11:
                        //Cloner
                        if (!multiballReady) {
                            cloneCount ++;
                            if (cloneCount > 2) multiballReady = true;
                        }
                        else {
                            multiballReady = false;
                            cloneCount = 0;
                            Debug.LogError("Multiball is not set up.");
                        }
                    break;
                    default:
                        Debug.LogError("Character reaction not listed.");
                    break;
                }
            }
            else if (blocking) {
                if (!blockSpecialReady){
                    sfx.PlayOneShot(missSFX);
                    StartCoroutine(ControlTime(info.timeSpeed, info.timeDura));
                    //Todo: Play block anim
                }
                else {
                    if (index == 8) {
                        player.transform.SetPositionAndRotation(new Vector2(player.transform.position.x, 1000), player.transform.rotation);
                        StartCoroutine(ControlTime(info.timeSpeedSpecial, info.timeDuraSpecial));
                        vel = new Vector2(vel.x + 100, 0);
                    }
                    else {
                        sfx.PlayOneShot(specialPunchSFX);
                        StartCoroutine(ControlTime(info.timeSpeed, info.timeDura));
                        vel = vel + new Vector2(50,50);
                    }
                }
                blocking = false;
                blockSpecialReady = false;
            }
            lastHitChar = index;
        }
    }

    void SpecialActivate(int c) {

    }

    void FixedUpdate() {
        //Control Character and Environment
        if (!gameOver && started) {
            //Move Player
            vel.y -= gravity*timeScale;
            player.transform.Translate(new Vector2(0, vel.y/60*timeScale), Space.World);
            player.transform.Rotate(new Vector3(0, 0, -vel.x/6*timeScale));
            if (vel.x > topSpeed) topSpeed = vel.x;

            //Move NPCs
            for (int i = 0; i < 5; i++) {
                sliders[i].transform.Translate(new Vector2(-vel.x/60*timeScale, 0));
                if (sliders[i].transform.position.x < despawn.position.x) {
                    RespawnNPC(i);
                }
            }
            
            //Move Environment
            for (int i = 0; i < 4; i++) {
                bg1[i].transform.Translate(new Vector2(-vel.x/60*timeScale*bgSpeedMult[0], 0));
                if (bg1[i].transform.position.x < -15) {
                    bg1[i].transform.Translate(new Vector2(32, 0));
                }
                bg2[i].transform.Translate(new Vector2(-vel.x/60*timeScale*bgSpeedMult[1], 0));
                if (bg2[i].transform.position.x < -15) {
                    bg2[i].transform.Translate(new Vector2(32, 0));
                }
                bg3[i].transform.Translate(new Vector2(-vel.x/60*timeScale*bgSpeedMult[2], 0));
                if (bg3[i].transform.position.x < -15) {
                    bg3[i].transform.Translate(new Vector2(32, 0));
                }
                bg4[i].transform.Translate(new Vector2(-vel.x/60*timeScale*bgSpeedMult[3], 0));
                if (bg4[i].transform.position.x < -15) {
                    bg4[i].transform.Translate(new Vector2(32, 0));
                }
                bg5[i].transform.Translate(new Vector2(-vel.x/60*timeScale*bgSpeedMult[4], 0));
                if (bg5[i].transform.position.x < -15) {
                    bg5[i].transform.Translate(new Vector2(32, 0));
                }
            }

            //React Player
            if (player.transform.position.y < 0) {
                player.transform.SetPositionAndRotation(spawn.transform.position, player.transform.rotation);
                vel = Vector2.Reflect(vel, Vector2.up);
                if (glides <= 0) {
                    vel = vel * groundDrag;
                    sfx.PlayOneShot(bounceSFX);
                }
                else {
                    glides --;
                }
                routeHistory.Add(new Vector3(distance, 0, 0));
                hitAngle = Vector2.Angle(Vector2.right, vel);
                specialReady[3] = false;
                if (lastHitChar == 3) {
                    specialReady[0] = false;
                    specialReady[1] = false;
                    specialReady[2] = false;
                }
            }
            if (vel.x < 0) vel.x = 0;
            if (vel.sqrMagnitude < minVel.sqrMagnitude && player.transform.position.y < gravity) {
                GameOver();
            }
            distance += vel.x/60*timeScale;
            if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax) {
                if (difficulty == 0) downForcePercent += rechargeEasy*timeScale;
                else if (difficulty == 1) downForcePercent += rechargeNormal*timeScale;
                else downForcePercent += rechargeHard*timeScale;
                if (downForcePercent >= 1) downForcePercent = 1;
            }
            if (blocking) {
                distToBlockSp -= vel.x/60*timeScale;
                if (distToBlockSp < 0) {
                    blockSpecialReady = false;
                    distToBlockSp += 100;
                }
                else if (distToBlockSp < 10) {
                    blockSpecialReady = true;
                    }
                else blockSpecialReady = false;
            }
        }
    }

    void Update() {
        //Control UI
        if (distance > SaveData.current.recordDist) {
            recordText.text = "New Record! " + distance.ToString("N2") + "m";
        }
        else recordText.text = "Record: " + SaveData.current.recordDist.ToString("N2") + "m";
        currentText.text = "This Run: " + distance.ToString("N2") + "m";
        velocityText.text = vel.x.ToString("N2") + "m/s";
        heightText.text = player.transform.position.y.ToString("N2") + "m";
        upForceText.text = upForceCount.ToString();
        glidesText.text = glides.ToString();
        if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax && vel.y < 0) {
            upForceText.color = Color.green;
        }
        else upForceText.color = Color.white;
        downForceText.text = downForcePercent.ToString("P0");
        if (player.transform.position.y > clickRangeMin && player.transform.position.y < clickRangeMax && vel.y > 0 && downForcePercent == 1) {
            downForceText.color = Color.green;
        }
        else downForceText.color = Color.white;
        if (player.transform.position.y > clickRangeMax) {
            flyingSet.SetActive(true);
        }
        else flyingSet.SetActive(false);
        if (vel.y > 0) {
            angle = Vector2.Angle(Vector2.right, vel);
        }
        else {
            angle = -Vector2.Angle(Vector2.right, vel);
        }
        angleText.text = hitAngle.ToString("N1") + "°";
        arrow.transform.SetPositionAndRotation(arrow.transform.position, Quaternion.Euler(0, 0, angle));
        startPowerMeter.fillAmount = powerMeter;
        startAngleArrow.gameObject.transform.SetPositionAndRotation(startAngleArrow.gameObject.transform.position, Quaternion.Euler(0, 0, angleMeter));
        if (blocking) blockImage.SetActive(true);
        else blockImage.SetActive(false);
        if (blockSpecialReady) blockSpecialImage.SetActive(true);
        else blockSpecialImage.SetActive(false);

        for (int i = 0; i < 4; i++) {
            if (specialReady[i]) {
                specialPortrait[i].color = Color.white;
            }
            else specialPortrait[i].color = new Color(1,1,1,.5f);
        }

        //Control Audio
        if (musicPlaying) {
            //Play Sting
            songTime = AudioSettings.dspTime;
            if (musicIndex == 0) {
                bgm[0].Play();
                musicIndex = 1;
            }
            //Move from Sting to Intro
            else if (musicIndex == 1) {
                if (songTime + 1.0f > nextSongTime) {
                    bgm[1].clip = gameBGM[1];
                    bgm[1].PlayScheduled(nextSongTime);
                    nextSongTime += gameBGM[1].length;
                    flip = 0;
                    musicIndex = 2;
                }
            }
            //Move from Intro to Looping Body
            else if (musicIndex == 2) {
                if (songTime + 1.0f > nextSongTime) {
                    bgm[flip].clip = gameBGM[2];
                    bgm[flip].PlayScheduled(nextSongTime);
                    nextSongTime += gameBGM[2].length;
                    flip = 1 - flip;
                }
            }
            //Move to Game Over Music
            else if (musicIndex == 3) {
                nextSongTime += gameOverBGM.length;
                bgm[0].Pause();
                bgm[1].Pause();
                bgm[0].clip = gameOverBGM;
                bgm[0].Play();
                bgm[0].loop = true;
                musicIndex = 4;
            }
        }
    }

    public void OnPause(bool toggle) {
        if (toggle) {
            timeScale = 0;
            musicPlaying = false;
            bgm[1-flip].Pause();
        }
        else {
            timeScale = 1;
            musicPlaying = true;
            bgm[1-flip].Play();
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
        routeHistory.Add(new Vector3(distance, player.transform.position.y, 1));
    }

    void GameOver() {
        //Stop Game
        musicIndex = 3;
        timeScale = 0;
        gameOver = true;
        gameOverPanel.SetActive(true);

        //Get info for graph
        Vector2 graphSize = new Vector2(gameOverGraph.GetComponent<RectTransform>().rect.width, gameOverGraph.GetComponent<RectTransform>().rect.height);
        LineRenderer line = gameOverLine.GetComponent<LineRenderer>();
        line.positionCount = routeHistory.Count;
        float maxX = routeHistory[routeHistory.Count-1].x;
        gameOverMaxDist.text = maxX.ToString("N2") + "m";
        float maxY = routeHistory[0].y;
        for (int i = 0; i < routeHistory.Count; i++) {
            maxY = Mathf.Max(routeHistory[i].y, maxY);
        }
        gameOverMaxHeight.text = maxY.ToString("N2") + "m";

        if (distance > SaveData.current.recordDist) {
            SaveData.current.recordDist = distance;
            goRecordDist.color = Color.green;
        }
        if (maxY > SaveData.current.recordHeight) {
            SaveData.current.recordHeight = maxY;
            goRecordHeight.color = Color.green;
        }
        if (topSpeed > SaveData.current.recordSpeed) {
            SaveData.current.recordSpeed = topSpeed;
            goRecordSpeed.color = Color.green;
        }
        
        FileManager.SaveFile("dummy", SaveData.current);

        goRecordDist.text = "Record Distance: " + SaveData.current.recordDist.ToString("N2") + "m";
        goRecordHeight.text = "Record Height: " + SaveData.current.recordHeight.ToString("N2") + "m";
        goRecordSpeed.text = "Record Speed: " + SaveData.current.recordSpeed.ToString("N2") + "m";

        //Set graph points
        Vector3[] routeArray = new Vector3[routeHistory.Count];
        for (int i = 0; i < routeHistory.Count; i++) {
            routeArray[i].x = routeHistory[i].x/maxX*graphSize.x;
            routeArray[i].y = routeHistory[i].y/maxY*graphSize.y;
            if (routeHistory[i].z > 0) {

            }
        }
        line.SetPositions(routeArray);
    }

    void RespawnNPC(int slideIndex) {
        sliders[slideIndex].transform.SetPositionAndRotation(new Vector2(sliders[slideIndex].transform.position.x + 50, sliders[slideIndex].transform.position.y), sliders[slideIndex].transform.rotation);
        int newIndex = 0;
        if (blocking) {
            newIndex = RandomRange.Range(newRange)-1;
            while (currentNPC.Contains(newIndex) || newIndex == 7) {
                newIndex = RandomRange.Range(newRange)-1;
            }
        }
        else {
            newIndex = RandomRange.Range(newRange)-1;
            while (currentNPC.Contains(newIndex)) {
                newIndex = RandomRange.Range(newRange)-1;
            }
        }
        currentNPC.Remove(sliders[slideIndex].GetComponent<NPCSlider>().charIndex);
        sliders[slideIndex].GetComponent<NPCSlider>().charIndex = newIndex;
        sliders[slideIndex].GetComponent<NPCSlider>().ResetChar();
        currentNPC.Add(newIndex);
    }

    IEnumerator SpecialChance() {
        chanceOn = true;
        yield return new WaitForSeconds(specialTimer);
        chanceOn = false;
    }

    bool angleFalling;
    IEnumerator SetAngle() {
        if (chooseAngle) {
            if (!angleFalling) {
                if (difficulty == -1) angleMeter = custom.x;
                else if (difficulty == 0) angleMeter += angleSpeedEasy*Time.deltaTime;
                else if (difficulty == 1) angleMeter += angleSpeedNormal*Time.deltaTime;
                else angleMeter += angleSpeedHard*Time.deltaTime;
                if (angleMeter >= 90) {
                    angleMeter = 90;
                    angleFalling = true;
                }
            }
            else {
                if (difficulty == -1) angleMeter = custom.x;
                else if (difficulty == 0) angleMeter -= angleSpeedEasy*Time.deltaTime;
                else if (difficulty == 1) angleMeter -= angleSpeedNormal*Time.deltaTime;
                else angleMeter -= angleSpeedHard*Time.deltaTime;
                if (angleMeter <= 0) {
                    angleMeter = 0;
                    angleFalling = false;
                }
            }
            yield return null;
            StartCoroutine(SetAngle());
        }
    }

    bool powerFalling;
    IEnumerator SetPower() {
        if (choosePower) {
            if (!powerFalling) {
                if (difficulty == -1) powerMeter = custom.y;
                else if (difficulty == 0) powerMeter += powerSpeedEasy*Time.deltaTime;
                else if (difficulty == 1) powerMeter += powerSpeedNormal*Time.deltaTime;
                else powerMeter += powerSpeedHard*Time.deltaTime;
                if (powerMeter >= 1) {
                    powerMeter = 1;
                    powerFalling = true;
                }
            }
            else {
                if (difficulty == -1) powerMeter = custom.y;
                else if (difficulty == 0) powerMeter -= powerSpeedEasy*Time.deltaTime;
                else if (difficulty == 1) powerMeter -= powerSpeedNormal*Time.deltaTime;
                else powerMeter -= powerSpeedHard*Time.deltaTime;
                if (powerMeter <= 0) {
                    powerMeter = 0;
                    powerFalling = false;
                }
            }
            yield return null;
            StartCoroutine(SetPower());
        }
    }

    IEnumerator ControlTime(float speed, float dura) {
        float oldScale = timeScale;
        timeScale = speed;
        yield return new WaitForSeconds(dura);
        timeScale = oldScale;
    }

    IEnumerator RouteTimer() {
        if (!gameOver) {
            yield return new WaitForSeconds(routeTime);
            routeHistory.Add(new Vector3(distance, player.transform.position.y, 0));
            StartCoroutine(RouteTimer());
        }
    }

    public void OnQuit() {
        Application.Quit();
    }
}
