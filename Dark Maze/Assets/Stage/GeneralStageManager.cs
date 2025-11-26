using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GeneralStageManager : MonoBehaviour
{
    [SerializeField] public int stageNum;
    [SerializeField] public GameObject fadeManagerObject;
    public FadeManager fadeManager;
    public bool fadeFlag;
    public enum GameStatus
    {
        start,
        play,
        stop,
        menu,
        over,
        clear,
    }
    public GameStatus status = GameStatus.start;
    [SerializeField] public PlayerController playerController;
    [SerializeField] public GameObject player;
    [SerializeField] public NavMeshSurface stageNav;
    [Header("ステージ情報")]
    [SerializeField] public GameObject[] areas;
    [SerializeField] public GameObject startObject;
    [SerializeField] public GameObject goalObject;
    [SerializeField] public GameObject[] buttonObjects;
    [SerializeField] public GameObject[] gateObjects;
    [SerializeField] public GameObject[] activeObject;
    [SerializeField] public GameObject[] areaLightObjects;
    [SerializeField] public GameObject[] lightObjects;
    [SerializeField] public GameObject mainCamera;
    [SerializeField] public GameObject[] cameraPointObjects;
    [SerializeField] public GameObject[] enemys;
    [Header("ステージ詳細情報")]
    [SerializeField] public float startTime = 0.1f;
    float startTimer;

    // cameraWorkの変数
    float[] cameraTimer = new float[10];
    Vector3 cameraPosi;
    Vector3 cameraRota;
    bool[] cameraWorkStartFlag = new bool[10];
    bool[] cameraWorkEndFlag = new bool[10];
    // ギミック変数
    public float originDegree;
    [SerializeField] float[] rotationTimer;
    [SerializeField] bool rotationFlag;

    [SerializeField] float[] openTimer;
    float nowHeight;
    bool oldOpenFlag;
    [SerializeField] float limitActiveObTime;
    bool endFadeInFlag;
    [SerializeField] float[] limitActiveObTimer;
    [SerializeField] float[] activeObTimer;
    [SerializeField] float[] activeLightTimer;
    [SerializeField] bool[] activeFlag;
    [SerializeField] public EnterArea[] enterArea;
    [SerializeField] public bool[] defeatGateFlag;

    [Header("UI情報")]
    [SerializeField] public GameObject playUI;
    [SerializeField] public GameObject menuUI;
    [SerializeField] public GameObject overUI;
    [SerializeField] public GameObject clearUI;
    public bool fadeMenuFlag;
    [SerializeField] public float fadeMenuTime = 0.2f;
    float fadeMenuTimer;    
    public bool fadeOverFlag;
    [SerializeField] public float fadeOverTime = 0.2f;
    float fadeOverTimer;    
    public bool fadeClearFlag;
    [SerializeField] public float fadeClearTime = 0.2f;
    float fadeClearTimer;
    [SerializeField] public GameObject[] menuTexts;
    [SerializeField] public GameObject[] overTexts;
    [SerializeField] public GameObject[] clearTexts;
    [SerializeField] public bool menuFlag;
    [SerializeField] public bool overFlag;
    [SerializeField] public bool clearFlag;
    [SerializeField] public bool enterFlag;
    [SerializeField] public int menuSelectNum;
    bool clearAnimeFlag;
    [SerializeField] public float clearAnimeTime = 2.5f;
    float clearAnimeTimer;

    void Start()
    {
        rotationTimer = new float[areas.Length];
        openTimer = new float[gateObjects.Length];
        activeObTimer = new float[activeObject.Length];
        activeLightTimer = new float[lightObjects.Length];
        activeFlag = new bool[activeObject.Length];
        defeatGateFlag = new bool[enemys.Length];
        menuTexts = new GameObject[3];
    }

    public void StartData()
    {
        GetUIandPlayer();
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
            fadeManager.AfterFade();
        }
        else if (fade != null) fadeManager = fade.GetComponent<FadeManager>();
        fadeManager.fadeOutFlag = true;
        fadeFlag = true;

        for (int i = 0; i < defeatGateFlag.Length; i++)
        {
            defeatGateFlag[i] = true;
        }
        if (GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if (GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = stageNum - 1;
    }

    public void StartAnime()
    {
        if (fadeFlag)
        {
            if (fadeManager.fadeOutFlag && fadeManager.endFlag)
            {
                fadeManager.fadeOutFlag = false;
                fadeManager.endFlag = false;
                fadeFlag = false;
            }
            fadeManager.FadeControl();
        }
        else
        {
            player.transform.position = new Vector3(startObject.transform.position.x, startObject.transform.position.y + 1, startObject.transform.position.z);
            if (startTimer > startTime)
            {
                startTimer = 0;
                status = GameStatus.play;
            }
            else if (startTimer < startTime) startTimer += Time.deltaTime;
        }
    }
    public void EndAnime()
    {
        if (fadeFlag)
        {
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
            fadeManager.FadeControl();
        }
        else if (!fadeFlag && clearAnimeFlag)
        {
            if (GameObject.Find("DataManager") != null)
            {
                DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                int dataNum = dataManager.useDataNum;
                if (dataManager.data[dataNum].clearStageNum == stageNum - 1) dataManager.data[dataNum].clearStageNum = stageNum;
                dataManager.SaveData(dataNum, dataManager.data[dataNum].playerName, dataManager.data[dataNum].clearStageNum, stageNum-1);
                if (stageNum % 5 == 0) dataManager.nextFieldFlag = true;
            }
            SceneManager.LoadScene("StageSelect");
        }
        else if (!fadeFlag && !clearAnimeFlag)
        {
            if (clearAnimeTimer > clearAnimeTime)
            {
                clearAnimeTimer = 0f;
                clearAnimeFlag = true;
                fadeFlag = true;
                fadeManager.fadeInFlag = true;
            }
            else if (clearAnimeTimer < clearAnimeTime)
            {
                clearAnimeTimer += Time.deltaTime;
            }
        }
    }

    public void Goal()
    {
        if (goalObject.GetComponent<GoalManager>().isGoalFlag)
        {
            status = GameStatus.clear;
        }
    }

    // 回転ギミック(回転するエリア、ライト、回転方向、回転度、回転にかかる時間)
    public void AreaRotation(GameObject area, int direction, int degree, float time, int i, bool end, ref bool flag)
    {
        if (rotationTimer[i] == 0)
        {
            originDegree = area.transform.localEulerAngles.y;
        }
        if (rotationTimer[i] > time)
        {
            status = GameStatus.play;
            rotationTimer[i] = 0;
            area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
            if(end)flag = false;
            stageNav.RemoveData();
            stageNav.BuildNavMesh();
        }
        else if (rotationTimer[i] < time)
        {
            status = GameStatus.stop;
            rotationTimer[i] += Time.deltaTime;
            float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
            area.transform.rotation = Quaternion.Euler(0, y, 0);
        }
    }
    public void Pre2AreaRotation(GameObject area, GameObject light, GameObject cameraPoint, int direction, int degree, float time, int i, bool end, ref bool flag)
    {
        if (rotationTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            status = GameStatus.stop;
            if (light) light.SetActive(true);
            originDegree = area.transform.localEulerAngles.y;
            cameraPosi = mainCamera.transform.position;
            cameraRota = mainCamera.transform.eulerAngles;
            cameraWorkStartFlag[i] = true;
        }
        if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
        {
            cameraTimer[i] = 0f;
            cameraWorkStartFlag[i] = false;
            mainCamera.transform.position = cameraPoint.transform.position;
            mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
        }
        else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
            Vector3 caPoRota = cameraPoint.transform.eulerAngles;
            caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
            Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
            Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
            mainCamera.transform.position = posi;
            mainCamera.transform.rotation = Quaternion.Euler(rota);
        }
        if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            rotationTimer[i] = 0;
            area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
            stageNav.RemoveData();
            stageNav.BuildNavMesh();
        }
        else if (rotationTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            rotationTimer[i] += Time.deltaTime;
            float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
            area.transform.rotation = Quaternion.Euler(0, y, 0);
        }
        if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
        {
            status = GameStatus.play;
            if (light != null) light.SetActive(false);
            mainCamera.transform.position = cameraPosi;
            mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
            cameraWorkEndFlag[i] = false;
            cameraTimer[i] = 0f;
            if (end) flag = false;
            Debug.Log(flag);
        }
        else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
            Vector3 caPoRota = cameraPoint.transform.eulerAngles;
            caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
            Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
            Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
            mainCamera.transform.position = posi;
            mainCamera.transform.rotation = Quaternion.Euler(rota);
        }       
    }
    public void PreAreaRotation(GameObject area, GameObject light, GameObject cameraPoint, int direction, int degree, float time, int i, bool end, ref bool flag)
    {
        if (flag)
        {
            if (cameraPoint)
            {
                // スタート
                if (rotationTimer[i] == 0f && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    originDegree = area.transform.localEulerAngles.y;
                    if (light != null) light.SetActive(true);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // エリア回転
                if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] = 0;
                    area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
                    cameraWorkEndFlag[i] = true;
                }
                else if (rotationTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
                    area.transform.rotation = Quaternion.Euler(0, y, 0);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraPosi = Vector3.zero;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) 
                    {
                        flag = false;
                        stageNav.RemoveData();
                        stageNav.BuildNavMesh();
                    }
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else
            {
                // スタート
                if (rotationTimer[i] == 0f && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    originDegree = area.transform.localEulerAngles.y;
                    if (light != null) light.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                // エリア回転
                if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] = 0;
                    area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
                    cameraWorkEndFlag[i] = true;
                }
                else if (rotationTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(originDegree, originDegree + direction * degree, rotationTimer[i] / time);
                    area.transform.rotation = Quaternion.Euler(0, y, 0);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end)
                    {
                        flag = false;
                        stageNav.RemoveData();
                        stageNav.BuildNavMesh();
                    }
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
    }
    //  ゲートオープンギミック(開閉ゲート、ライト、open=true,close=false、開閉にかかる時間、同じギミック同時の際最後のフラグ、複数同時の際フラグ、終了フラグ)
    public void SenceGate(GameObject gate, bool open, float time, int i)
    {
        if (open != oldOpenFlag)
        {
            float a;
            nowHeight = gate.transform.position.y;
            if (open)
            {
                a = Mathf.InverseLerp(0f, -2.1f, nowHeight);
                openTimer[i] = a * time;
            }
            else if (!open)
            {
                a = Mathf.InverseLerp(-2.1f, 0f, nowHeight);
                openTimer[i] = a * time;
            }
        }
        // 閉じる
        if (!open)
        {
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                openTimer[i] = 2;
                gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(nowHeight, 0f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
        // 開ける
        else
        {
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                openTimer[i] = 2;
                gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(nowHeight, -2.1f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
        oldOpenFlag = open;
    }
    public void PreSenceGate(GameObject gate, GameObject light, GameObject cameraPoint, bool open, bool complete, float time, int i)
    {
        if (!complete)
        {
            if (open != oldOpenFlag)
            {
                float a;
                nowHeight = gate.transform.position.y;
                if (open)
                {
                    a = Mathf.InverseLerp(0f, -2.1f, nowHeight);
                    openTimer[i] = a * time;
                }
                else if (!open)
                {
                    a = Mathf.InverseLerp(-2.1f, 0f, nowHeight);
                    openTimer[i] = a * time;
                }
            }
            if (cameraPoint != null)
            {
                // 閉じる
                if (!open)
                {
                    if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        if (light != null) light.SetActive(true);
                        status = GameStatus.stop;
                        cameraWorkStartFlag[i] = true;
                        cameraPosi = mainCamera.transform.position;
                        cameraRota = mainCamera.transform.eulerAngles;
                    }
                    // 最初のカメラ移動
                    if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        cameraWorkStartFlag[i] = false;
                        cameraTimer[i] = 0f;
                        if (cameraPoint.transform.eulerAngles.y >= 180f) mainCamera.transform.position = cameraPoint.transform.position;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                    }
                    else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                    if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                        cameraWorkEndFlag[i] = true;
                    }
                    else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, 0f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                    // 最後のカメラ移動
                    if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f && cameraPoint != null)
                    {
                        status = GameStatus.play;
                        if (light != null) light.SetActive(false);
                        mainCamera.transform.position = cameraPosi;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                        cameraWorkEndFlag[i] = false;
                        cameraTimer[i] = 0f;
                    }
                    else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                }
                // 開ける
                else
                {
                    if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        if (light != null) light.SetActive(true);
                        status = GameStatus.stop;
                        cameraWorkStartFlag[i] = true;
                        cameraPosi = mainCamera.transform.position;
                        cameraRota = mainCamera.transform.eulerAngles;
                    }
                    // 最初のカメラ移動
                    if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        cameraWorkStartFlag[i] = false;
                        cameraTimer[i] = 0f;
                        if (cameraPoint.transform.eulerAngles.y >= 180f) mainCamera.transform.position = cameraPoint.transform.position;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                    }
                    else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                    if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                        cameraWorkEndFlag[i] = true;
                    }
                    else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                    {
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, -2.1f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                    // 最後のカメラ移動
                    if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                    {
                        status = GameStatus.play;
                        if (light != null) light.SetActive(false);
                        mainCamera.transform.position = cameraPosi;
                        mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                        cameraWorkEndFlag[i] = false;
                        cameraTimer[i] = 0f;
                    }
                    else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                    {
                        cameraTimer[i] += Time.deltaTime;
                        Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                        caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                        Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                        Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                        mainCamera.transform.position = posi;
                        mainCamera.transform.rotation = Quaternion.Euler(rota);
                    }
                }
                oldOpenFlag = open;
            }
            else if (cameraPoint == null)
            {
                // 閉じる
                if (!open)
                {
                    if (openTimer[i] > time)
                    {
                        status = GameStatus.play;
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    }
                    else if (openTimer[i] < time)
                    {
                        status = GameStatus.stop;
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, 0f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                }
                // 開ける
                else
                {
                    if (openTimer[i] > time)
                    {
                        status = GameStatus.play;
                        openTimer[i] = 2;
                        gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    }
                    else if (openTimer[i] < time)
                    {
                        status = GameStatus.stop;
                        openTimer[i] += Time.deltaTime;
                        float y = Mathf.Lerp(nowHeight, -2.1f, openTimer[i] / time);
                        gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                    }
                }
                oldOpenFlag = open;
            }
        }
    }
    public void Gate(GameObject gate, bool open, float time, int i, bool end, ref bool flag)
    {
        if (open)
        {
            if (openTimer[i] == 0) gate.SetActive(true);
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                gate.SetActive(false);
                openTimer[i] = 0f;
                if (end) flag = false;
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
        else if (!open)
        {
            if (openTimer[i] == 0) gate.SetActive(true);
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                gate.SetActive(true);
                openTimer[i] = 0f;
                if (end) flag = false;
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
    }    
    public void PreGate(GameObject gate, GameObject light, GameObject cameraPoint, bool open, float time, int i, bool end, ref bool flag)
    {
        if (open)
        {
            if (cameraPoint != null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                     status = GameStatus.stop;
                     gate.SetActive(true);
                     if (light != null) light.SetActive(true);
                     cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                     cameraRota = mainCamera.transform.eulerAngles;
                     cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                     cameraWorkStartFlag[i] = false;
                     cameraTimer[i] = 0f;
                     mainCamera.transform.position = cameraPoint.transform.position;
                     Vector3 caPoRota = cameraPoint.transform.eulerAngles; 
                     caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                     cameraPoint.transform.eulerAngles = caPoRota;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                     cameraTimer[i] += Time.deltaTime;
                     Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                     caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                     Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                     Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                     mainCamera.transform.position = posi;
                     mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateOpen
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(false);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    if (light != null) light.SetActive(false);
                    mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if(end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if (cameraPoint == null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(true);
                    openTimer[i] = 0f;
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    if (light != null) light.SetActive(true);
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
        else if (!open)
        {
            if (cameraPoint != null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    gate.SetActive(true);
                    if (light != null) light.SetActive(true);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateClose
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    mainCamera.transform.position = cameraPosi;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if (cameraPoint == null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    gate.SetActive(true);
                    openTimer[i] = 0f;
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    if (light != null) light.SetActive(true);
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
        }
    }
    public void Pre2Gate(GameObject gate, GameObject light, GameObject cameraPoint, bool open, float time, int i, bool end, ref bool flag)
    {
        if (open)
        {
            if (cameraPoint != null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                     status = GameStatus.stop;
                     gate.SetActive(true);
                     if (light != null) light.SetActive(true);
                     cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                     cameraRota = mainCamera.transform.eulerAngles;
                     cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                     cameraWorkStartFlag[i] = false;
                     cameraTimer[i] = 0f;
                     mainCamera.transform.position = cameraPoint.transform.position;
                     Vector3 caPoRota = cameraPoint.transform.eulerAngles; 
                     caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                     cameraPoint.transform.eulerAngles = caPoRota;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                     cameraTimer[i] += Time.deltaTime;
                     Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                     caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                     Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                     Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                     mainCamera.transform.position = posi;
                     mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateOpen
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(false);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    if (light != null) light.SetActive(false);
                    mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if(end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if(!end && cameraPoint == null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    if (light != null) light.SetActive(true);
                    cameraWorkStartFlag[i] = true;
                }
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraTimer[i] = 0;
                    cameraWorkStartFlag[i] = false;
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    openTimer[i] = 0;
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    if(light) light.SetActive(false);
                    cameraTimer[i] = 0;
                    cameraWorkEndFlag[i] = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                }
            }
            else if (end && cameraPoint == null)
            {
                if (openTimer[i] == 0) gate.SetActive(true);
                if (openTimer[i] > time)
                {
                    status = GameStatus.play;
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(false);
                    openTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (openTimer[i] < time)
                {
                    status = GameStatus.stop;
                    if (light != null) light.SetActive(true);
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(0f, -2.1f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
            }
        }
        else if (!open)
        {
            if (cameraPoint != null)
            {
                if (openTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    gate.SetActive(true);
                    if (light != null) light.SetActive(true);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    mainCamera.transform.position = cameraPoint.transform.position;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
                }
                else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateClose
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                // 最後のカメラ移動
                if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
                {
                    status = GameStatus.play;
                    if (light != null) light.SetActive(false);
                    mainCamera.transform.position = cameraPosi;
                    mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                    cameraWorkEndFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if (light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
                {
                    cameraTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
            }
            else if(!end && cameraPoint == null)
            {
                if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    float t = Mathf.InverseLerp(0, time, openTimer[i]);
                    float y = Mathf.Lerp(-2.1f, 0f, t);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
                else if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i]) gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
            }
            else if(end && cameraPoint == null)
            {
                if (openTimer[i] == 0) gate.SetActive(true);
                if (openTimer[i] > time)
                {
                    status = GameStatus.play;
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    gate.SetActive(true);
                    openTimer[i] = 0f; 
                    if(light != null) light.SetActive(false);
                    if (end) flag = false;
                }
                else if (openTimer[i] < time)
                {
                    status = GameStatus.stop;
                    if (light != null) light.SetActive(true);
                    openTimer[i] += Time.deltaTime;
                    float y = Mathf.Lerp(-2.1f, 0f, openTimer[i] / time);
                    gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
                }
            }
        }
    }
    // 時間内オブジェクトを出現ギミック
    public void LimitActiveObject(GameObject activeOb, GameObject light, int i, bool end, ref bool flag)
    {
        int activeObParentCount = activeOb.transform.childCount;
        Material[] activeObMaterials = new Material[activeObParentCount];

        if (limitActiveObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            for (int n = 0; n < activeObParentCount; n++)
            {
                activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                color.a = 0f;
                activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
            }
            activeOb.SetActive(true);
            if (light != null) light.SetActive(true);
            cameraWorkStartFlag[i] = true;
        }
        if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
        {
            cameraTimer[i] = 0f;
            cameraWorkStartFlag[i] = false;
        }
        else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
        }
        if (limitActiveObTimer[i] > limitActiveObTime && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            for (int n = 0; n < activeObParentCount; n++)
            {
                activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                color.a = 0f;
                activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
            }
            activeOb.SetActive(false);
            limitActiveObTimer[i] = 0;
            cameraWorkEndFlag[i] = true;
        }
        else if (limitActiveObTimer[i] < limitActiveObTime && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
        {
            // FadeIn
            if (limitActiveObTimer[i] < 0.2f && !endFadeInFlag)
            {
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    float a = Mathf.Lerp(0f, 1f, limitActiveObTimer[i] / limitActiveObTime);
                    color.a = a;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
            }
            else if (limitActiveObTimer[i] > 0.2f && !endFadeInFlag)
            {
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    color.a = 1f;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
                endFadeInFlag = true;
            }
            // FadeOut
            if (limitActiveObTimer[i] > limitActiveObTime - 0.2f)
            {
                for (int n = 0; n < activeObParentCount; n++)
                {
                    activeObMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0];
                    Color color = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color;
                    float a = Mathf.Lerp(1f, 0f, limitActiveObTimer[i] / limitActiveObTime);
                    color.a = a;
                    activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().materials[0].color = color;
                }
            }
            limitActiveObTimer[i] += Time.deltaTime;
        }
        if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
        {
            cameraTimer[i] = 0f;
            if (light != null) light.SetActive(false);
            if (end) flag = false;
            endFadeInFlag = false;
            cameraWorkEndFlag[i] = false;
        }
        else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
        {
            cameraTimer[i] += Time.deltaTime;
        }       
    }
    public void PreLimitActiveObject(GameObject activeOb, GameObject light, int i, bool end, ref bool flag)
    {
        if (limitActiveObTimer[i] == 0)
        {
            Color color = activeOb.GetComponent<MeshRenderer>().material.color;
            color.a = 0f;
            activeOb.GetComponent<MeshRenderer>().material.color = color;
            activeOb.SetActive(true);
            if (light != null) light.SetActive(true);
        }
        if (limitActiveObTimer[i] > limitActiveObTime)
        {
            activeOb.SetActive(false);
            limitActiveObTimer[i] = 0;
            if (end) flag = false;
            endFadeInFlag = false;
            if (light != null) light.SetActive(false);
        }
        else if (limitActiveObTimer[i] < limitActiveObTime)
        {
            // FadeIn
            if (limitActiveObTimer[i] < 0.2f && !endFadeInFlag)
            {
                Color color = activeOb.GetComponent<MeshRenderer>().material.color;
                float a = Mathf.Lerp(0f, 1f, limitActiveObTimer[i] / limitActiveObTime);
                color.a = a;
                activeOb.GetComponent<MeshRenderer>().material.color = color;
            }
            else if (limitActiveObTimer[i] > 0.2f && !endFadeInFlag)
            {
                Color color = activeOb.GetComponent<MeshRenderer>().material.color;
                color.a = 1f;
                activeOb.GetComponent<MeshRenderer>().material.color = color;
                endFadeInFlag = true;
            }
            // FadeOut
            if (limitActiveObTimer[i] > limitActiveObTime - 0.2f)
            {
                Color color = activeOb.GetComponent<MeshRenderer>().material.color;
                float a = Mathf.Lerp(1f, 0f, limitActiveObTimer[i] / limitActiveObTime);
                color.a = a;
                activeOb.GetComponent<MeshRenderer>().material.color = color;
            }
            limitActiveObTimer[i] += Time.deltaTime;
        }
    }
    // 透明化オブジェクトを可視化ギミック
    public void ActiveObject(GameObject activeOb, float time, int i, bool end, ref bool flag)
    {
        if (activeObTimer[i] == 0) activeOb.SetActive(true);
        if (activeObTimer[i] > time)
        {
            activeObTimer[i] = 0f;
            if (end) flag = false;
        }
        else if (activeObTimer[i] < time) activeObTimer[i] += Time.deltaTime;
    }
    public void PreActiveObject(GameObject activeOb, GameObject light, GameObject cameraPoint, float time, int i, bool end, ref bool flag)
    {
        if(cameraPoint == null)
        {
            if (activeObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                status = GameStatus.stop;
                activeOb.SetActive(true);
                if(activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];                        
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                if (light != null) light.SetActive(true);
                cameraWorkStartFlag[i] = true;
            }
            // 最初のカメラ移動
            if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
            {
                cameraWorkStartFlag[i] = false;
                cameraTimer[i] = 0f;
            }
            else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
            }
            if (activeObTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else if (!activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                activeObTimer[i] = time;
            }
            else if (activeObTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                activeObTimer[i] += Time.deltaTime;
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
            }
            // 最後のカメラ移動
            if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
            {
                status = GameStatus.play;
                cameraTimer[i] = 0f;
                cameraWorkEndFlag[i] = true;
                if (end) flag = false;
                if (light != null) light.SetActive(false);
            }
            else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
            }
        }
        else if(cameraPoint != null)
        {
            if (activeObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                status = GameStatus.stop;
                activeOb.SetActive(true);
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = 0f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                if (light != null) light.SetActive(true);
                cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                cameraRota = mainCamera.transform.eulerAngles;
                cameraWorkStartFlag[i] = true;
            }
            // 最初のカメラ移動
            if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
            {
                cameraWorkStartFlag[i] = false;
                cameraTimer[i] = 0f;
                mainCamera.transform.position = cameraPoint.transform.position;
                mainCamera.transform.rotation = Quaternion.Euler(cameraPoint.transform.eulerAngles);
            }
            else if (cameraWorkStartFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
                Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                Vector3 rota = Vector3.Lerp(cameraRota, caPoRota, cameraTimer[i] / 0.5f);
                mainCamera.transform.position = posi;
                mainCamera.transform.rotation = Quaternion.Euler(rota);
            }
            if (activeObTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = 1f;
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
                activeObTimer[i] = 0f;
                cameraWorkEndFlag[i] = true;
            }
            else if (activeObTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                activeObTimer[i] += Time.deltaTime;
                if (activeOb.GetComponent<MeshRenderer>())
                {
                    Material[] activeMaterials = new Material[activeOb.GetComponent<MeshRenderer>().materials.Length];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.GetComponent<MeshRenderer>().materials[n];
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.GetComponent<MeshRenderer>().materials[n].color = a;
                    }
                }
                else
                {
                    Material[] activeMaterials = new Material[activeOb.transform.childCount];
                    for (int n = 0; n < activeMaterials.Length; n++)
                    {
                        activeMaterials[n] = activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material;
                        Color a = activeMaterials[n].color;
                        a.a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                        activeOb.transform.GetChild(n).GetComponent<MeshRenderer>().material.color = a;
                    }
                }
            }
            // 最後のカメラ移動
            if (cameraWorkEndFlag[i] && cameraTimer[i] > 0.5f)
            {
                status = GameStatus.play;
                if (light != null) light.SetActive(false);
                mainCamera.transform.position = cameraPosi;
                mainCamera.transform.rotation = Quaternion.Euler(cameraRota);
                cameraWorkEndFlag[i] = false;
                cameraTimer[i] = 0f;
                if(end) flag = false;
            }
            else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f)
            {
                cameraTimer[i] += Time.deltaTime;
                Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
                Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                Vector3 rota = Vector3.Lerp(caPoRota, cameraRota, cameraTimer[i] / 0.5f);
                mainCamera.transform.position = posi;
                mainCamera.transform.rotation = Quaternion.Euler(rota);
            }
        }
    }
    public void ActiveLight(GameObject lightOb, float time, int i, bool end, ref bool flag)
    {
        if (activeLightTimer[i] == 0) lightOb.SetActive(true);
        if (activeLightTimer[i] > time)
        {
            activeLightTimer[i] = 0;
            lightOb.GetComponent<Light>().spotAngle = 180f;
            if (end) flag = false;
        }
        else if (activeLightTimer[i] < time)
        {
            activeLightTimer[i] += Time.deltaTime;
            float range = Mathf.Lerp(0f, 180f, activeLightTimer[i] / time);
            lightOb.GetComponent<Light>().spotAngle = range;
        }
    }
    public void PreActiveLight(GameObject lightOb, float time, int i, bool end, ref bool flag)
    {
        if (activeLightTimer[i] == 0) lightOb.SetActive(true);
        if (activeLightTimer[i] > time)
        {
            activeLightTimer[i] = time;
            lightOb.GetComponent<Light>().spotAngle = 180f;
            if (end) flag = false;
        }
        else if (activeLightTimer[i] < time)
        {
            activeLightTimer[i] += Time.deltaTime;
            float range = Mathf.Lerp(0f, 180f, activeLightTimer[i] / time);
            lightOb.GetComponent<Light>().spotAngle = range;
        }
    }

    public void GetUIandPlayer()
    {
        GameObject playerSet = GameObject.Find("Player (Set)");
        player = playerSet.transform.GetChild(1).gameObject;
        playerController = playerSet.transform.GetChild(1).GetComponent<PlayerController>();

        playUI = playerSet.transform.GetChild(0).transform.GetChild(0).gameObject;
        menuUI = playerSet.transform.GetChild(0).transform.GetChild(1).gameObject;
        overUI = playerSet.transform.GetChild(0).transform.GetChild(2).gameObject;
        clearUI = playerSet.transform.GetChild(0).transform.GetChild(3).gameObject;

        menuTexts = new GameObject[3];
        menuTexts[0] = menuUI.transform.GetChild(2).gameObject;
        menuTexts[1] = menuUI.transform.GetChild(3).gameObject;
        menuTexts[2] = menuUI.transform.GetChild(4).gameObject;
        overTexts = new GameObject[2];
        overTexts[0] = overUI.transform.GetChild(2).gameObject;
        overTexts[1] = overUI.transform.GetChild(3).gameObject;
        clearTexts = new GameObject[2];
        clearTexts[0] = clearUI.transform.GetChild(2).gameObject;
        clearTexts[1] = clearUI.transform.GetChild(3).gameObject;
    }
    // メニュー関数
    public void MenuUIControl()
    {
        if (fadeMenuFlag && menuSelectNum == 0)
        {
            if (fadeMenuTimer > fadeMenuTime)
            {
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                fadeMenuTimer = 0;
                fadeMenuFlag = false;
            }
            else if (fadeMenuTimer < fadeMenuTime)
            {
                fadeMenuTimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, fadeMenuTimer / fadeMenuTime);
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1f);
            }
        }
        else if (fadeMenuFlag && menuSelectNum == 2)
        {
            if (fadeMenuTimer > fadeMenuTime)
            {
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
                fadeMenuTimer = 0;
                fadeMenuFlag = false;

                menuSelectNum = 0;
                playUI.SetActive(true);
                menuUI.SetActive(false);
                menuSelectNum = 0;
                for (int i = 0; i < menuTexts.Length; i++) TextAnime(menuTexts[i], false);
                menuFlag = false;
            }
            else if (fadeMenuTimer < fadeMenuTime)
            {
                fadeMenuTimer += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0f, fadeMenuTimer / fadeMenuTime);
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            for (int i = 0; i < menuTexts.Length; i++)
            {
                if (menuSelectNum == i) TextAnime(menuTexts[i], true);
                else if (menuSelectNum != i) TextAnime(menuTexts[i], false);
            }
            if (enterFlag)
            {
                if (fadeFlag)
                {
                    if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
                    fadeManager.FadeControl();
                }
                else
                {
                    if (menuSelectNum == 0)
                    {
                        int fieldNum = stageNum / 5;
                        int number = stageNum % 5;
                        SceneManager.LoadScene($"{fieldNum + 1}-{number}");
                    }
                    else if (menuSelectNum == 1)
                    {
                        if (GameObject.Find("DataManager") != null)
                        {
                            DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                            int dataNum = dataManager.useDataNum;
                            dataManager.data[dataNum].selectStageNum = 0;
                        }
                        SceneManager.LoadScene("StageSelect");
                    }
                    else if (menuSelectNum == 2)
                    {
                        fadeMenuFlag = true;
                    }
                    enterFlag = false;
                }
            }
        }
    }
    // ゲームオーバー関数
    public void OverUIControl()
    {

    }
    public void TextAnime(GameObject textOb, bool flag)
    {
        TextMeshProUGUI text = textOb.GetComponent<TextMeshProUGUI>();
        // 元のサイズ
        if (!flag) text.fontSize = 100f;
        // 拡大
        else text.fontSize = 120f;
    }
    // Input関数
    public void InputMenuButton(InputAction.CallbackContext context)
    {
        if (context.started && !menuFlag && status == GameStatus.play)
        {
            menuFlag = true;
            fadeMenuFlag = true;
            playUI.SetActive(false);
            menuUI.SetActive(true);
            menuUI.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
        }
    }
    //Enter
    public void InputEnterButton(InputAction.CallbackContext context)
    {
        if (menuFlag && context.started && !enterFlag)
        {
            enterFlag = true;
            if (menuSelectNum != 2)
            {
                fadeManager.fadeInFlag = true;
                fadeFlag = true;
            }
        }
    }
    // Select
    public void InputSelectControl(InputAction.CallbackContext context)
    {
        if (menuFlag)
        {
            if (context.started && context.ReadValue<Vector2>().y > 0)
            {
                menuSelectNum++;
                if (menuSelectNum > 2)
                {
                    menuSelectNum = 0;
                }
            }
            else if (context.started && context.ReadValue<Vector2>().y < 0)
            {
                menuSelectNum--;
                if (menuSelectNum < 0)
                {
                    menuSelectNum = 2;
                }
            }
        }
    }
}