using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public bool isReady { get; private set; }
    public enum BGMName
    {
        gameBgms,
    }
    public enum SEName
    {
        gameSes,
        playerSes,
        enemySes,
        gimmickSes,
    }
    AudioSource[] gameBGMs;
    public AudioSource[] gameSEs;
    public AudioSource[] playerSEs;
    public AudioSource[] enemySEs;
    public AudioSource[] gimmickSEs;

    bool[] gameBGMisPauseFlags;
    bool[] gameSEisPauseFlags;
    bool[] playerSEisPauseFlags;
    bool[] enemySEisPauseFlags;
    bool[] gimmickSEisPauseFlags;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        name = "AudioManager";
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {               
        GetAudio();
        SetisPauseFlag();
        isReady = true;
    }

    void Update()
    {

    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("ÉVÅ[ÉìÇ™êÿÇËë÷ÇÌÇ¡ÇΩÇÊ: " + scene.name);
        StopAll();
    }


    public void GetAudio()
    {
        // BGM
        gameBGMs = new AudioSource[transform.GetChild(0).transform.childCount];
        for (int i = 0; i < gameBGMs.Length; i++) gameBGMs[i] = transform.GetChild(0).transform.GetChild(i).GetComponent<AudioSource>();
        // SE
        gameSEs = new AudioSource[transform.GetChild(1).transform.GetChild(0).transform.childCount];
        playerSEs = new AudioSource[transform.GetChild(1).transform.GetChild(1).transform.childCount];
        enemySEs = new AudioSource[transform.GetChild(1).transform.GetChild(2).transform.childCount];
        gimmickSEs = new AudioSource[transform.GetChild(1).transform.GetChild(3).transform.childCount];
        for (int i = 0; i < gameSEs.Length; i++) gameSEs[i] = transform.GetChild(1).transform.GetChild(0).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < playerSEs.Length; i++) playerSEs[i] = transform.GetChild(1).transform.GetChild(1).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < enemySEs.Length; i++) enemySEs[i] = transform.GetChild(1).transform.GetChild(2).transform.GetChild(i).GetComponent<AudioSource>();
        for (int i = 0; i < gimmickSEs.Length; i++) gimmickSEs[i] = transform.GetChild(1).transform.GetChild(3).transform.GetChild(i).GetComponent<AudioSource>();
    }

    void SetisPauseFlag()
    {
        gameBGMisPauseFlags = new bool[gameBGMs.Length];
        gameSEisPauseFlags = new bool[gameSEs.Length];
        playerSEisPauseFlags = new bool[playerSEs.Length];
        enemySEisPauseFlags = new bool[enemySEs.Length];
        gimmickSEisPauseFlags = new bool [gimmickSEs.Length];
    }

    public void PlayGameBGM(int i)
    {
        gameBGMs[i].PlayOneShot(gameBGMs[i].clip);
    }

    public void PlayGameSE(int i)
    {
        gameSEs[i].PlayOneShot(gameSEs[i].clip);
    }

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
