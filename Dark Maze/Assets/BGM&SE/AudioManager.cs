using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の音響（BGM・SE）を一括管理するマネージャークラス
/// シングルトンパターンを採用し、シーンを跨いで存在し続ける
/// </summary>
public class AudioManager : MonoBehaviour
{
    // どこからでもアクセス可能な静的インスタンス
    public static AudioManager Instance;
    // セットアップが完了したかどうかのフラグ
    public bool isReady { get; private set; }

    // BGMのカテゴリー定義（拡張可能）
    public enum BGMName
    {
        gameBgms,
    }
    // SEのカテゴリー定義
    public enum SEName
    {
        gameSes,
        playerSes,
        enemySes,
        gimmickSes,
    }

    // オーディオソースを格納する配列群
    AudioSource[] gameBGMs;
    public AudioSource[] gameSEs;
    public AudioSource[] playerSEs;
    public AudioSource[] enemySEs;
    public AudioSource[] gimmickSEs;

    // ポーズ（一時停止）時に、どの音が鳴っていたかを記憶するためのフラグ群
    bool[] gameBGMisPauseFlags;
    bool[] gameSEisPauseFlags;
    bool[] playerSEisPauseFlags;
    bool[] enemySEisPauseFlags;
    bool[] gimmickSEisPauseFlags;

    private void Awake()
    {
        // --- シングルトンの初期化 ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 重複していれば自身を破棄
            return;
        }

        Instance = this;
        name = "AudioManager";
        DontDestroyOnLoad(gameObject); // シーン遷移で破棄されないように設定
    }

    void Start()
    {
        GetAudio();        // 子オブジェクトからAudioSourceを自動収集
        SetisPauseFlag();  // 各種フラグ用配列の初期化
        isReady = true;    // 準備完了
    }

    private void OnEnable()
    {
        // シーンが読み込まれた際のイベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // イベント解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンが切り替わった時に自動で呼ばれる
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("シーンが切り替わったよ: " + scene.name);
        if (isReady) StopAll();
    }

    /// <summary>
    /// 子オブジェクトの階層構造を走査して、AudioSourceを取得する
    /// ヒエラルキーの順序に依存するため、構造を変える際は注意
    /// </summary>
    public void GetAudio()
    {
        // BGM：第1階層(Child 0)の下にあるオーディオを取得
        gameBGMs = new AudioSource[transform.GetChild(0).transform.childCount];
        for (int i = 0; i < gameBGMs.Length; i++) gameBGMs[i] = transform.GetChild(0).transform.GetChild(i).GetComponent<AudioSource>();
        
        // SE：第2階層(Child 1)の下にある各カテゴリーからオーディオを取得
        gameSEs = new AudioSource[transform.GetChild(1).transform.GetChild(0).transform.childCount];
        playerSEs = new AudioSource[transform.GetChild(1).transform.GetChild(1).transform.childCount];
        enemySEs = new AudioSource[transform.GetChild(1).transform.GetChild(2).transform.childCount];
        gimmickSEs = new AudioSource[transform.GetChild(1).transform.GetChild(3).transform.childCount];

        for (int i = 0; i < gameSEs.Length; i++) gameSEs[i] = transform.GetChild(1).transform.GetChild(0).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < playerSEs.Length; i++) playerSEs[i] = transform.GetChild(1).transform.GetChild(1).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < enemySEs.Length; i++) enemySEs[i] = transform.GetChild(1).transform.GetChild(2).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < gimmickSEs.Length; i++) gimmickSEs[i] = transform.GetChild(1).transform.GetChild(3).transform.GetChild(i).GetComponent<AudioSource>();
    }

    /// <summary>
    /// 各一時停止フラグ配列を初期化（ソースの数に合わせる）
    /// </summary>
    void SetisPauseFlag()
    {
        gameBGMisPauseFlags = new bool[gameBGMs.Length];
        gameSEisPauseFlags = new bool[gameSEs.Length];
        playerSEisPauseFlags = new bool[playerSEs.Length];
        enemySEisPauseFlags = new bool[enemySEs.Length];
        gimmickSEisPauseFlags = new bool [gimmickSEs.Length];
    }

    // --- 再生・停止・一時停止の個別メソッド群 ---

    public void PlayGameBGM(int i)
    {
        gameBGMs[i].PlayOneShot(gameBGMs[i].clip);
    }

    public void PlayGameSE(int i)
    {
        gameSEs[i].PlayOneShot(gameSEs[i].clip);
    }

    /// <summary>
    /// 一時停止フラグが立っている全ての音を再開させる
    /// </summary>
    public void ResumeAll()
    {
        for (int i = 0; i < gameBGMisPauseFlags.Length; i++) 
        {
            if (gameBGMisPauseFlags[i])
            {
                gameBGMs[i].Play();
                gameBGMisPauseFlags[i] = false;
            }
        }
        for (int i = 0; i < gameSEisPauseFlags.Length; i++) 
        {
            if (gameSEisPauseFlags[i])
            {
                gameSEs[i].Play();
                gameSEisPauseFlags[i] = false;
            }            
        }
        for (int i = 0; i < playerSEisPauseFlags.Length; i++)
        {
            if (playerSEisPauseFlags[i])
            {
                playerSEs[i].Play();
                playerSEisPauseFlags[i] = false;
            }                    
        }
        for (int i = 0; i < enemySEisPauseFlags.Length; i++) 
        {
            if (enemySEisPauseFlags[i])
            {
                enemySEs[i].Play();
                enemySEisPauseFlags[i] = false;
            }                
        }
        for (int i = 0; i < gimmickSEisPauseFlags.Length; i++) 
        {
            if (gimmickSEisPauseFlags[i])
            {
                gimmickSEs[i].Play();
                gimmickSEisPauseFlags[i] = false;
            }
        }
    }

    public void PlayBGM(BGMName bgmName, int i)
    {
        switch (bgmName) 
        {
            case BGMName.gameBgms:
                gameBGMs[i].Play();
                break;
        }
    }

    /// <summary>
    /// 現在鳴っている全ての音を一時停止し、状態を保存する
    /// </summary>
    public void PauseAll()
    {
        for (int i = 0; i < gameBGMs.Length; i++) if (gameBGMs[i].isPlaying)
        {
            gameBGMs[i].Pause();
            gameBGMisPauseFlags[i] = true;
        }
        for (int i = 0; i < gameSEs.Length; i++) if (gameSEs[i].isPlaying)
        {
            gameSEs[i].Pause();
            gameSEisPauseFlags[i] = true;
        }
        for (int i = 0; i < playerSEs.Length; i++) if (playerSEs[i].isPlaying)
        {
            playerSEs[i].Pause();
            playerSEisPauseFlags[i] = true;
        }
        for (int i = 0; i < enemySEs.Length; i++) if (enemySEs[i].isPlaying)
        {
            enemySEs[i].Pause();
            enemySEisPauseFlags[i] = true;
        }
        for (int i = 0; i < gimmickSEs.Length; i++) if (gimmickSEs[i].isPlaying)
        {
            gimmickSEs[i].Pause();
            gimmickSEisPauseFlags[i] = true;
        }
    }

    public void PausePlayerSE()
    {
        for(int i = 0; i < playerSEs.Length; i++) if (playerSEs[i].isPlaying) playerSEs[i].Pause();
    }

    public void PauseBGM(BGMName bgmName, int i)
    {
        switch (bgmName) 
        {
            case BGMName.gameBgms:
                gameBGMs[i].Pause();
                break;
        }
    }

    /// <summary>
    /// 全ての音を完全に停止させる（状態は保存しない）
    /// </summary>
    public void StopAll()
    {
        for (int i = 0; i < gameBGMs.Length; i++) if (gameBGMs[i].isPlaying) gameBGMs[i].Stop();
        for (int i = 0; i < gameSEs.Length; i++) if (gameSEs[i].isPlaying) gameSEs[i].Stop();
        for (int i = 0; i < playerSEs.Length; i++) if (playerSEs[i].isPlaying) playerSEs[i].Stop();
        for (int i = 0; i < enemySEs.Length; i++) if (enemySEs[i].isPlaying) enemySEs[i].Stop();
        for (int i = 0; i < gimmickSEs.Length; i++) if (gimmickSEs[i].isPlaying) gimmickSEs[i].Stop();
    }

    public void StopEnemySE()
    {
        for (int i = 0; i < enemySEs.Length; i++) if (enemySEs[i].isPlaying) enemySEs[i].Stop();
    }

    public void StopBGM(BGMName bgmName, int i)
    {
        switch (bgmName) 
        {
            case BGMName.gameBgms:
                gameBGMs[i].Stop();
                break;
        }
    }

    /// <summary>
    /// 指定されたSEを一度だけ再生（重複可能）
    /// </summary>
    public void PlayOneShotSE(SEName seName, int i)
    {
        switch (seName) 
        {
            case SEName.gameSes:
                gameSEs[i].PlayOneShot(gameSEs[i].clip);
                break;
            case SEName.playerSes:
                playerSEs[i].PlayOneShot(playerSEs[i].clip);
                break;
            case SEName.enemySes:
                enemySEs[i].PlayOneShot(enemySEs[i].clip);
                break;
            case SEName.gimmickSes:
                gimmickSEs[i].PlayOneShot(gimmickSEs[i].clip);
                break;
        }
    }

    /// <summary>
    /// 指定されたSEを通常再生（ループSEなどに適している）
    /// </summary>
    public void PlaySE(SEName seName, int i)
    {
        switch (seName) 
        {
            case SEName.gameSes:
                gameSEs[i].Play();
                break;
            case SEName.playerSes:
                playerSEs[i].Play();
                break;
            case SEName.enemySes:
                enemySEs[i].Play();
                break;
            case SEName.gimmickSes:
                gimmickSEs[i].Play();
                break;
        }
    }
    public void PauseSE(SEName seName, int i)
    {
        switch (seName) 
        {
            case SEName.gameSes:
                gameSEs[i].Pause();
                break;
            case SEName.playerSes:
                playerSEs[i].Pause();
                break;
            case SEName.enemySes:
                enemySEs[i].Pause();
                break;
            case SEName.gimmickSes:
                gimmickSEs[i].Pause();
                break;
        }
    }
    public void StopSE(SEName seName, int i)
    {
        switch (seName) 
        {
            case SEName.gameSes:
                gameSEs[i].Stop();
                break;
            case SEName.playerSes:
                playerSEs[i].Stop();
                break;
            case SEName.enemySes:
                enemySEs[i].Stop();
                break;
            case SEName.gimmickSes:
                gimmickSEs[i].Stop();
                break;
        }
    }
}
