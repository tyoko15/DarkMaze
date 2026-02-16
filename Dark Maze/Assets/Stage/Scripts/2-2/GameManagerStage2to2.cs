
/// <summary>
/// ステージ 2-2 専用の進行管理クラス
/// 時間制限付きのギミック（LimitActiveObject）が導入されている
/// </summary>
public class GameManagerStage2to2 : GeneralStageManager
{
    void Start()
    {
        // 親クラスで定義されている初期化処理（参照の取得など）を実行
        StartData();
    }

    void Update()
    {
        // ゲームの現在の状態に応じて処理を分岐
        switch (status)
        {
            case GameStatus.start:
                playerController.status = 0; // プレイヤーを「開始演出待ち」状態に
                StartAnime();                // ステージ開始のアニメーション
                break;
            case GameStatus.play:
                // プレイ中のメインロジック
                Gimmick1();
                JudgeOver();                 // 死亡判定チェック
                Goal();                      // ゴール判定チェック
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1;
                break;
            case GameStatus.stop:
                // ギミック演出中などの一時停止状態
                Gimmick1();
                Goal();
                playerController.status = 2; // プレイヤーを「停止」状態に
                break;
            case GameStatus.menu:
                MenuUIControl();
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;
            case GameStatus.over:
                Over();
                OverUIControl();
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }

    // --- ギミック詳細 ---

    /// <summary>
    /// スイッチによる時間制限付きの仕掛け
    /// </summary>
    public void Gimmick1()
    {
        // ボタンが押されたら「LimitActiveObject」を実行
        // おそらく、一定時間だけ扉が開く、あるいは足場が出現するなどの仕掛け
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) LimitActiveObject(gateObjects[0], lightObjects[0], 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
}