using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    // ===== DebugUI =====
    bool debugFlag;
    [SerializeField] GameObject debugUI;
    TextMeshProUGUI playerStatusText;
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
        debugTexts[0] = debugUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        playerStatusText = debugUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        DisplayDebug();
    }

    void DisplayDebug()
    {
        if (Input.GetKeyDown(KeyCode.L)) debugFlag = (!debugFlag) ? true : false;

        if (debugFlag) debugUI.SetActive(true);
        else debugUI.SetActive(false);

        if (playerController != null) playerStatusText.text = $"PlayerStatus : {playerController.status}";
        if (stageManager != null) debugTexts[0].text = $"GameStats : {stageManager.status}";
    }
}
