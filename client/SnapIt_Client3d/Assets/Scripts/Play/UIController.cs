using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public static event Action<bool> OnScaninteract;

    public static event Action<bool> ButtonInteract;

    public static event Action<bool> CountDOwnEvent;


    [SerializeField]
    public ScanBtn scanBtn;

    [SerializeField]
    public BtnEvents btnEvents;

    [SerializeField]
    public WordList wordList;

    [SerializeField]
    public CountDown countDown;

    [Header("User Info")]

    [SerializeField]
    private GameObject userinfoPrefab;

    [SerializeField]
    private GameObject userinfoParent;


    [Header("Game UI")]

    [SerializeField]
    private TextMeshProUGUI gameRoundInfo;

    [SerializeField]
    private TextMeshProUGUI gameWordInfo;

    [SerializeField]
    private TextMeshProUGUI countingInfo;


    [Header("Button Touching")]
    [SerializeField]
    private GameObject selectBtnPrefab;
    [SerializeField]
    private GameObject selectBtnParent;

    [Header("Popup")]
    [SerializeField]
    public ResultPopup resultPopup;


    void OnDisable()
    {
        GameController.UpdatePlayingRoom -= UpdateUserList;
    }

    void OnEnable()
    {
        GameController.UpdatePlayingRoom += UpdateUserList;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        SetRoundInfo("0", "word");
        scanBtn.Init();
        btnEvents.Init();
        wordList.ResetWord();
        countDown.InitCountDown();

        resultPopup.Init();


    }


    void Update()
    {

    }


    public static void ScanInvoke(bool isOn)
    {
        OnScaninteract?.Invoke(isOn);
    }

    public static void ButtonInteractInvoke(bool isOn)
    {
        ButtonInteract?.Invoke(isOn);
    }

    public static async void CountDownEventInvoke(int type)
    {
        if (type == 1)
        {
            //start
            CountDOwnEvent?.Invoke(true);

        }
        else if (type == 2)
        {
            //end 무엇을할까용
            //scan btn interactable false하기
            //state test의 chekck info false하기
            ScanInvoke(false);
            StateTester.checkState = false;

            GameData data = new GameData
            {
                round = GameController.Instance.cur_round,
                score = 0,
                gameType = GameController.Instance.cur_game_type,
                stuff = "diohfoiwe(필요없는부분)"
            };

            // JSON string 으로 변환

            string jsonString = JsonUtility.ToJson(data);
            Debug.Log("json string is : " + jsonString);


            //추가로 0점이라고 보내기
            await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/score", jsonString);

            TimeOverInfo overInfo = new TimeOverInfo
            {
                timeOver = true,
                gameType = GameController.Instance.cur_game_type,
                round = GameController.Instance.cur_round
            };
            //타이머 종료 웹소켓 보내기
            await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/end", JsonUtility.ToJson(overInfo));
        }
        else if (type == 3)
        {
            //제출완료로 타이머종료
            CountDOwnEvent?.Invoke(false);
        }
    }

    public void UpdateUserList(UserListResponse res)
    {
        Debug.Log("update user LIst @@@@");
        // 기존 유저 정보 초기화
        for (int j = 0; j < userinfoParent.transform.childCount; j++)
        {
            Destroy(userinfoParent.transform.GetChild(j).gameObject);
        }

        // GameInfoResponse 기반으로 유저 정보 UI 생성
        for (int i = 0; i < res.body.userList.Count; i++)
        {
            Instantiate(userinfoPrefab, Vector3.zero, Quaternion.identity, userinfoParent.transform)
            .GetComponent<UserInfoUI>().Init("0",res.body.userList[i]/*res.body.userList[i]*/);
        }

        Debug.Log("cur user email is : " + GameController.Instance.cur_email);
        //내 정보는 가장 아래에
        foreach (Transform child in userinfoParent.transform)
        {
            UserInfoUI ui = child.GetComponent<UserInfoUI>();
            if (ui.GetEmail() == GameController.Instance.cur_email)
            {
                ui.gameObject.transform.SetAsLastSibling();
                break;
            }
        }
    }

    public void SetRoundInfo(string round, string word)
    {
        gameRoundInfo.text = "ROUND" + round;
        gameWordInfo.text = word;
    }

    public void UpdateUserScore(GameInfoResponse res)
    {
        // 유저 목록 전체 순회
        foreach (Transform child in userinfoParent.transform)
        {
            UserInfoUI ui = child.GetComponent<UserInfoUI>();
            Debug.Log("update user infooooo");
            if (ui == null)
                continue;

            // 현재 UI에 설정된 이메일 가져오기
            string uiEmail = ui.GetEmail();

            // GameInfoResponse에서 해당 이메일을 가진 유저 정보 찾기
            foreach (UserInfo user in res.body.userInfoList)
            {
                if (user.email == GameController.Instance.cur_email)
                {
                    GameController.Instance.cur_score = user.score + user.score2;
                }

                if (user.email == uiEmail)
                {
                    int totalScore = user.score + user.score2;
                    //ui.ChangeInfo($"{user.email} | 점수: {totalScore}");
                    ui.ChangeInfo(totalScore.ToString());
                    break;
                }
            }
        }
    }

    public void InitUserColor()
    {
        foreach (Transform child in userinfoParent.transform)
        {
            UserInfoUI ui = child.GetComponent<UserInfoUI>();
            ui.InitColor();
        }
    }

    
}
