using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class GeneralStageManager : MonoBehaviour
{
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
    float[] cameraTimer = new float[5];
    Vector3 cameraPosi;
    Vector3 cameraRota;
    bool[] cameraWorkStartFlag = new bool[5];
    bool[] cameraWorkEndFlag = new bool[5];
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

    [Header("Input情報")]
    [SerializeField] public GameObject playUI;
    [SerializeField] public GameObject menuUI;
    public bool startMenuFlag;
    [SerializeField] public float startMenuTime = 0.2f;
    float startMenuTimer;
    [SerializeField] public GameObject[] menuTexts;
    [SerializeField] public bool menuFlag;
    [SerializeField] public bool enterFlag;
    [SerializeField] public int menuSelectNum;

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
    public void Goal()
    {
        if (goalObject.GetComponent<GoalManager>().isGoalFlag)
        {
            fadeFlag = true;
            fadeManager.fadeInFlag = true;
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
                    cameraPosi = mainCamera.transform.position;
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                    Debug.Log(light.activeSelf);
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
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, cameraPoint.transform.eulerAngles, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // エリア回転
                if (rotationTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    rotationTimer[i] = 0;
                    area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
                    cameraWorkEndFlag[i] = true;
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
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
                    mainCamera.transform.position = cameraPosi;
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
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraPoint.transform.eulerAngles, cameraRota, cameraTimer[i] / 0.5f);
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
                Debug.Log(light.activeSelf);

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
                    cameraPosi = mainCamera.transform.position;
                    cameraRota = mainCamera.transform.eulerAngles;
                    cameraWorkStartFlag[i] = true;
                }
                // 最初のカメラ移動
                if (cameraWorkStartFlag[i] && cameraTimer[i] > 0.5f)
                {
                    cameraWorkStartFlag[i] = false;
                    cameraTimer[i] = 0f;
                    if(cameraPoint.transform.eulerAngles.y >= 180f) mainCamera.transform.position = cameraPoint.transform.position;
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
                // GateOpen
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, -2.1f, gate.transform.position.z);
                    gate.SetActive(false);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
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
                    cameraPosi = mainCamera.transform.position;
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
                    Vector3 posi = Vector3.Lerp(cameraPosi, cameraPoint.transform.position, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraRota, cameraPoint.transform.eulerAngles, cameraTimer[i] / 0.5f);
                    mainCamera.transform.position = posi;
                    mainCamera.transform.rotation = Quaternion.Euler(rota);
                }
                // GateClose
                if (openTimer[i] > time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    openTimer[i] = 0f;
                    gate.transform.position = new Vector3(gate.transform.position.x, 0f, gate.transform.position.z);
                    gate.SetActive(true);
                    if (light != null) light.SetActive(false);
                    cameraPosi = new Vector3(player.transform.position.x, player.transform.position.y + 10f, player.transform.position.z - 2f);
                    cameraRota = new Vector3(80f, 0f, 0f);
                    cameraWorkEndFlag[i] = true;
                }
                else if (openTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
                {
                    status = GameStatus.stop;
                    openTimer[i] += Time.deltaTime;
                    Vector3 caPoRota = cameraPoint.transform.eulerAngles;
                    caPoRota.y = (caPoRota.y <= 180) ? cameraPoint.transform.eulerAngles.y : (caPoRota.y - 180) * -1f;
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
                    Vector3 posi = Vector3.Lerp(cameraPoint.transform.position, cameraPosi, cameraTimer[i] / 0.5f);
                    Vector3 rota = Vector3.Lerp(cameraPoint.transform.eulerAngles, cameraRota, cameraTimer[i] / 0.5f);
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
    public void LimitActiveObject(GameObject activeOb, int i, bool end, ref bool flag)
    {
        if (limitActiveObTimer[i] == 0)
        {
            Color color = activeOb.GetComponent<MeshRenderer>().material.color;
            color.a = 0f;
            activeOb.GetComponent<MeshRenderer>().material.color = color;
            activeOb.SetActive(true);
        }
        if (limitActiveObTimer[i] > limitActiveObTime)
        {
            activeOb.SetActive(false);
            limitActiveObTimer[i] = 0;
            if (end) flag = false;
            endFadeInFlag = false;
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
            if (activeObTimer[i] == 0)
            {
                status = GameStatus.stop;
                activeOb.SetActive(true);
                if (light != null) light.SetActive(true);
            }
            if (activeObTimer[i] > time)
            {
                status = GameStatus.play;
                Color color = activeOb.GetComponent<MeshRenderer>().material.color;
                color.a = 1f;
                if(activeOb.GetComponent<MeshRenderer>()) activeOb.GetComponent<MeshRenderer>().material.color = color;
                activeObTimer[i] = 0f;
                if (end) flag = false;
                if (light != null) light.SetActive(false);
            }
            else if (activeObTimer[i] < time)
            {
                activeObTimer[i] += Time.deltaTime;
                Color color = activeOb.GetComponent<MeshRenderer>().material.color;
                float a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                color.a = a;
                activeOb.GetComponent<MeshRenderer>().material.color = color;
            }
        }
        else if(cameraPoint != null)
        {
            if (activeObTimer[i] == 0 && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                status = GameStatus.stop;
                activeOb.SetActive(true);
                if (light != null) light.SetActive(true);
                cameraPosi = mainCamera.transform.position;
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
                Color color = Color.white;
                if (activeOb.GetComponent<MeshRenderer>()) color = activeOb.GetComponent<MeshRenderer>().material.color;
                color.a = 1f;
                if (activeOb.GetComponent<MeshRenderer>()) activeOb.GetComponent<MeshRenderer>().material.color = color;
                activeObTimer[i] = 0f;
                cameraWorkEndFlag[i] = true;
            }
            else if (activeObTimer[i] < time && !cameraWorkStartFlag[i] && !cameraWorkEndFlag[i])
            {
                activeObTimer[i] += Time.deltaTime;
                Color color = Color.white;
                if (activeOb.GetComponent<MeshRenderer>()) color = activeOb.GetComponent<MeshRenderer>().material.color;
                float a = Mathf.Lerp(0f, 1f, activeObTimer[i] / time);
                color.a = a;
                if (activeOb.GetComponent<MeshRenderer>()) activeOb.GetComponent<MeshRenderer>().material.color = color;
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
                if(end) flag = false;
            }
            else if (cameraWorkEndFlag[i] && cameraTimer[i] < 0.5f && cameraPoint != null)
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

    // メニュー関数
    public void MenuControl()
    {
        if (startMenuFlag)
        {
            if (startMenuTimer > startMenuTime)
            {
                menuUI.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                startMenuTimer = 0;
                startMenuFlag = false;
            }
            else if (startMenuTimer < startMenuTime)
            {
                startMenuTimer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, startMenuTimer / startMenuTime);
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
                    if (menuSelectNum == 0) SceneManager.LoadScene("1-1");
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
                        playUI.SetActive(true);
                        menuUI.SetActive(false);
                        menuSelectNum = 0;
                        for (int i = 0; i < menuTexts.Length; i++) TextAnime(menuTexts[i], false);
                        menuFlag = false;
                    }
                    enterFlag = false;
                }
            }
        }
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
            startMenuFlag = true;
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