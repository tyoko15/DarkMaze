using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStage1to2 : MonoBehaviour
{
    [SerializeField] GameObject fadeManagerObject;
    FadeManager fadeManager;
    bool fadeFlag;
    public enum GameStatus
    {
        start,
        play,
        stop,
        over,
        clear,
    }
    public GameStatus status = GameStatus.start;
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject player;
    [SerializeField] NavMeshSurface stageNav;
    [Header("ステージ情報")]
    [SerializeField] GameObject[] areas;
    [SerializeField] GameObject startObject;
    [SerializeField] GameObject goalObject;
    [SerializeField] GameObject[] buttonObjects;
    [SerializeField] GameObject[] gateObjects;
    [SerializeField] GameObject[] lightObjects;
    [SerializeField] GameObject[] enemys;
    [Header("ステージ詳細情報")]
    [SerializeField] float startTime;
    float startTimer;

    // ギミック変数
    float originDegree;
    [SerializeField] float[] rotationTimer;
    [SerializeField] float[] openTimer;
    float nowHeight;
    bool oldOpenFlag;
    [SerializeField] float[] activeObTimer;
    [SerializeField] float[] activeLightTimer;
    [SerializeField] bool[] activeFlag;
    [SerializeField] EnterArea[] enterArea;
    [SerializeField] bool[] defeatGateFlag;
    void Start()
    {
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
        openTimer[0] = 2f;
        if(GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if(GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = 1;
    }

    void Update()
    {
        switch (status)
        {
            case GameStatus.start:
                playerController.status = 0;
                StartAnime();
                break;
            case GameStatus.play:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4(); 
                Goal();
                playerController.status = 1;
                break;
            case GameStatus.stop:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4();
                Goal();
                playerController.status = 2;
                break;
            case GameStatus.over:
                playerController.status = 3;
                break;
            case GameStatus.clear:
                playerController.status = 4;
                EndAnime();
                break;
        }
    }

    void StartAnime()
    {
        if(fadeFlag)
        {
            if (fadeManager.fadeOutFlag && fadeManager.endFlag)
            {
                fadeFlag = false;
                fadeManager.fadeOutFlag = false;
                fadeManager.endFlag = false;
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

    void EndAnime()
    {
        if (fadeFlag)
        {
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
            fadeManager.FadeControl();
        }
        else SceneManager.LoadScene("StageSelect");
    }

    // 右上エリアの箱を感圧版に置くギミック
    public void Gimmick1()
    {
        SenceGate(gateObjects[0], buttonObjects[0].GetComponent<ButtonManager>().buttonFlag, 2, 0);
    }
    // 左下エリアの回転ギミック
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], -1, 90, 2, 0, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアの敵撃破＆離れたボタンギミック
    public void Gimmick3()
    {
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[1], false, 2, 1, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(lightObjects[0], 2 , 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], true, 2, 1, true, ref defeatGateFlag[0]);            
        }
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) Gate(gateObjects[2], true, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右上エリアのゲートオープンギミック
    public void Gimmick4()
    {
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], -1, 180, 2, 0, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
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

        // 回転ギミック(回転するエリア、回転方向、回転度、回転にかかる時間)
        public void AreaRotation(GameObject area, int direction, int degree, float time, int i, ref bool flag)
    {
        if (rotationTimer[i] == 0) originDegree = area.transform.localEulerAngles.y;
        if (rotationTimer[i] > time)
        {
            status = GameStatus.play;
            rotationTimer[i] = 0;
            area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
            flag = false;
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
    //  ゲートオープンギミック(開閉ゲート、open=true,close=false、開閉にかかる時間、同じギミック同時の際最後のフラグ、複数同時の際フラグ、終了フラグ)
    public void SenceGate(GameObject gate, bool open, float time, int i)
    {
        if (open != oldOpenFlag)
        {
            float a;
            nowHeight = gate.transform.position.y;
            if (open)
            {
                a = Mathf.InverseLerp(1f, -1.1f, nowHeight);
                openTimer[i] = a * time;
            }
            else if(!open)
            {
                a = Mathf.InverseLerp(-1.1f, 1f, nowHeight);
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
                gate.transform.position = new Vector3(gate.transform.position.x, 1f, gate.transform.position.z);
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(nowHeight, 1f, openTimer[i] / time);
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
                gate.transform.position = new Vector3(gate.transform.position.x, -1.1f, gate.transform.position.z);
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(nowHeight, -1.1f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
        oldOpenFlag = open;
    }
    public void Gate(GameObject gate, bool open, float time, int i, bool end, ref bool flag)
    {
        if (open)
        {
            if (openTimer[i] == 0) gate.SetActive(true);
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                gate.transform.position = new Vector3(gate.transform.position.x, -1.1f, gate.transform.position.z);
                gate.SetActive(false);
                openTimer[i] = 0f;
                if (end) flag = false;
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(1f, -1.1f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
        else if (!open)
        {
            if (openTimer[i] == 0) gate.SetActive(true);
            if (openTimer[i] > time)
            {
                status = GameStatus.play;
                gate.transform.position = new Vector3(gate.transform.position.x, 1f, gate.transform.position.z);
                gate.SetActive(true);
                if (end)
                {
                    flag = false;
                    openTimer[i] = 0f;
                }
            }
            else if (openTimer[i] < time)
            {
                status = GameStatus.stop;
                openTimer[i] += Time.deltaTime;
                float y = Mathf.Lerp(-1.1f, 1f, openTimer[i] / time);
                gate.transform.position = new Vector3(gate.transform.position.x, y, gate.transform.position.z);
            }
        }
    }
    // 透明化オブジェクトを可視化ギミック
    public void ActiveObject(GameObject activeOb, float time, int i, bool end, ref bool flag)
    {
        if (activeObTimer[i] == 0) activeOb.SetActive(true);
        if (activeObTimer[i] > time)
        {
            Color color = activeOb.GetComponent<MeshRenderer>().material.color;
            color.a = 1f;
            activeOb.GetComponent<MeshRenderer>().material.color = color;
            activeObTimer[i] = 0f;
            if (end) flag = false;
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
}

