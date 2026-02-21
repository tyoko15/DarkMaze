using TMPro;
using UnityEngine;

public class StageDebugManager : MonoBehaviour
{
    // ===== DebugUI =====
    bool debugFlag;
    [SerializeField] GameObject debugUI;

    TextMeshProUGUI[] debugTexts = new TextMeshProUGUI[10];

    // ===== 外部アクセス =====
    PlayerController playerController;
    GeneralStageManager stageManager;
    void Start()
    {
        GetDebugUI();

        // 外部アクセス
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        stageManager = GameObject.Find("GameManager").GetComponent<GeneralStageManager>();
    }


    // デバック用UIの初期化
    void GetDebugUI()
    {
        for (int i = 0; i < debugUI.transform.childCount; i++)
        {
            debugTexts[i] = debugUI.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        DisplayDebug();
    }

    void DisplayDebug()
    {
        if (Input.GetKeyDown(KeyCode.L)) debugFlag = (!debugFlag) ? true : false;

        debugUI.SetActive(debugFlag);

        if (playerController != null) debugTexts[1].text = $"PlayerStatus : {playerController.status}";
        if (stageManager != null)
        {
            debugTexts[0].text = $"GameStats : {stageManager.status}";
            debugTexts[2].text = $"RotationTime : {stageManager.GetRotationTimer(0)}";
            debugTexts[3].text = $"EndCameraWorkFlag : {stageManager.GetEndCameraWorkFlag(0)}";
        }
    }
}
