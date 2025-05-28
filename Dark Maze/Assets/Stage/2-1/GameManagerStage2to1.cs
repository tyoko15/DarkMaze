using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManagerStage2to1 : MonoBehaviour
{
    [SerializeField] GameObject fadeManagerObject;
    FadeManager fadeManager;
    bool fadeFlag;
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

    [Header("Input情報")]
    [SerializeField] GameObject playUI;
    [SerializeField] GameObject menuUI;
    bool startMenuFlag;
    [SerializeField] float startMenuTime;
    float startMenuTimer;
    [SerializeField] GameObject[] menuTexts;
    [SerializeField] bool menuFlag;
    [SerializeField] bool enterFlag;
    [SerializeField] int menuSelectNum;
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
        //openTimer[0] = 2f;
        if (GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if (GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = 5;
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
                if (menuFlag) status = GameStatus.menu;
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
            case GameStatus.menu:
                MenuControl();
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;
            case GameStatus.over:
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }

    void StartAnime()
    {
        if (fadeFlag)
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
        else
        {
            if (GameObject.Find("DataManager") != null)
            {
                DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                int dataNum = dataManager.useDataNum;
                if (dataManager.data[dataNum].clearStageNum == 5) dataManager.data[dataNum].clearStageNum = 6;
                dataManager.data[dataNum].selectStageNum = 5;
                dataManager.SaveData(dataManager.useDataNum, dataManager.data[dataManager.useDataNum].playerName, dataManager.data[dataNum].clearStageNum, dataManager.data[dataNum].selectStageNum);
            }
            SceneManager.LoadScene("StageSelect");
        }
    }

    // 左下エリアの敵撃破でボタン出現
    // ボタンは左下エリアの回転ギミック
    public void Gimmick1()
    {
        if (enterArea[2].enterAreaFlag) Gate(gateObjects[0], false, 2, 0, true, ref enterArea[2].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(lightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            ActiveObject(buttonObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0], true, 2, 0, true, ref defeatGateFlag[0]);
        }
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2],  1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアのボタンを同時押しでボタン出現
    // ボタンは右下エリアの回転ギミック
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) ActiveObject(buttonObjects[3], 2, 0, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[3], 1, 90, 2, 1, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右上エリアの敵撃破で扉開放ギミック
    public void Gimmick3()
    {
        if (enterArea[1].enterAreaFlag) Gate(gateObjects[1], false, 2, 1, true, ref enterArea[1].enterAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(lightObjects[1], 2, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[1], true, 2, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[2], true, 2, 2, true, ref defeatGateFlag[1]);
        }
    }
    // 
    public void Gimmick4()
    {

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
    public void AreaRotation(GameObject area, int direction, int degree, float time, int i, bool end, ref bool flag)
    {
        if (rotationTimer[i] == 0) originDegree = area.transform.localEulerAngles.y;
        if (rotationTimer[i] > time)
        {
            status = GameStatus.play;
            rotationTimer[i] = 0;
            area.transform.rotation = Quaternion.Euler(0, originDegree + direction * degree, 0);
            if (end) flag = false;
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
            else if (!open)
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
    // メニュー関数
    void MenuControl()
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
                    if (menuSelectNum == 0) SceneManager.LoadScene("2-1");
                    else if (menuSelectNum == 1)
                    {
                        if (GameObject.Find("DataManager") != null)
                        {
                            DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                            int dataNum = dataManager.useDataNum;
                            dataManager.data[dataNum].selectStageNum = 5;
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
    void TextAnime(GameObject textOb, bool flag)
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

