using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public static event Action<bool> LoginEvent;

    public static event Action GameStartEvent;

    [SerializeField]
    private MainUIController mainUIController;

    public string tok = "";

    private static string token = "";

    [SerializeField]
    private WebSocketService webSocketService;



    public static Action<UserListResponse> UpdatePlayingRoom;

    public string cur_uuid = "";

    public string cur_game_type = "";

    public string cur_word = "";

    public int cur_round = 0;

    public string cur_email = "";

    public int cur_score = 0;

    public bool startTrigger = true;

    public RoundStartMessage roundStartMessage = null;

    public int MaxRound = 2;

    public string apikey = "";
    public string url = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        mainUIController.Init();
    }

    public static string getAcessToken()
    {
        return token;
    }

    public static bool haveToken()
    {
        return string.IsNullOrEmpty(token);
    }

    public static void setAcessToken(string tok)
    {
        token = tok;


    }

    public void LoginInvoke(bool isOn)
    {
        MainUIController.LoginActionInvoke(isOn);
        LoginEvent?.Invoke(isOn);

        webSocketService.StartConnect("ws://chabin37.iptime.org:32766/ws/websocket?token=" + token);

    }

    public static void UpdatePlayingRoomInvoke(UserListResponse res)
    {
        UpdatePlayingRoom?.Invoke(res);
    }

    public static void GameStartEventInvoke()
    {
        GameStartEvent?.Invoke();
    }

    public async void GameSceneLoad()
    {
        Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
        // SceneManager.LoadSceneAsync("ArScene",LoadSceneMode.Additive);
        // //StartCoroutine(LoadSceneCoroutine("ArScene"));
        roundStartMessage = null;
        startTrigger = true;
        // 씬이 이미 로드되어 있는지 확인
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == "ArScene" && loadedScene.isLoaded)
            {
                Debug.LogWarning("ArScene is already loaded!");
                //start함수들 실행해주기
                PlayingController.Instance.Init();
                return; // 중복 로드 방지
            }
        }

        // 없으면 로드
        await SceneManager.LoadSceneAsync("ArScene", LoadSceneMode.Additive);
        
        PlayingController.Instance.Init();
    }


    public float progressf = 0f;

    public IEnumerator LoadSceneCoroutine(string sceneName)
    {
        
        progressf = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);

        // (선택) 씬이 다 로드될 때까지 멈추지 않게 설정
        operation.allowSceneActivation = false;

        // 로딩 진행률 체크
        while (!operation.isDone)
        {
            Debug.Log("Loading progress: " + (operation.progress * 100f) + "%");
            progressf = operation.progress * 100f;
            // 거의 다 됐으면(90%) 수동으로 전환 허용
            if (operation.progress >= 0.9f)
            {
                // 여기서 로딩 애니메이션 완료 조건 등이 만족되면 전환
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        //SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

}
