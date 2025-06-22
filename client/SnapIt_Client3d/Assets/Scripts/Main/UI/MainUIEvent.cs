using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIEvent : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private Canvas mainCanvas;

    [SerializeField]
    private Button singleTestBtn;

    [SerializeField]
    private Button BackToTitleBtn;

    [SerializeField]
    private Button CreateButton;

    [SerializeField]
    private Button CreateButtonOn;

    [SerializeField]
    private Button CancelCreateBtn;

    [SerializeField]
    private Button UserInfoBtn;

    [SerializeField]
    private LoginWebview loginWebview;

    [SerializeField]
    private GameObject LoginPopup;

    [SerializeField]
    private TextMeshProUGUI userinfotxt;

    [SerializeField]
    private RoomList roomList;

    [SerializeField]
    private CreateRoomPopup createRoomPopup;


    [Header("Test Popup")]
    [SerializeField]
    private TestPopup testPopup;

    [SerializeField]
    private Button testPopupBtn;

    [SerializeField]
    private Button testPopupCloseBtn;

    [Header("Api Popup")]
    [SerializeField]
    private APiISettingPopup apiPopup;

    [SerializeField]
    private Button apiPopupBtn;

    [SerializeField]
    private Button apiPopupCloseBtn;


    public void OnDisable()
    {
        MainUIController.InteractionEvent -= SetMainInteraction;
        MainUIController.EnterRoom -= ActiveUI;
        MainUIController.LoginAction -= LoginStatus;
        MainUIController.RoomEnterEvent -= PlaySceneLoading;

    }

    void OnEnable()
    {
        MainUIController.InteractionEvent += SetMainInteraction;
        MainUIController.EnterRoom += ActiveUI;
        MainUIController.LoginAction += LoginStatus;
        MainUIController.RoomEnterEvent += PlaySceneLoading;
    }

    public void Init()
    {
        mainCanvas.gameObject.SetActive(true);
        testPopup.gameObject.SetActive(false);


        LoginPopup.SetActive(false);
        if (GameController.haveToken())
        {
            userinfotxt.text = "Login needed";
        }


        roomList.Init();


        UiUtil.AddButtonClickEvent(singleTestBtn, () => { Testing(); });
        UiUtil.AddButtonClickEvent(BackToTitleBtn, () => { BackToTitle(); });
        UiUtil.AddButtonClickEvent(CreateButton, () => { createRoomOpen(); });
        UiUtil.AddButtonClickEvent(CreateButtonOn, () => { CreateRoomBtn(); LoginPopup.SetActive(false); });
        UiUtil.AddButtonClickEvent(UserInfoBtn, () => { if (!GameController.haveToken()) { ActiveLogin(1); } });
        UiUtil.AddButtonClickEvent(CancelCreateBtn, () => { CancelCreate(); });

        UiUtil.AddButtonClickEvent(testPopupBtn, () => { TestPopupOpen(); });
        UiUtil.AddButtonClickEvent(testPopupCloseBtn, () => { testPopup.gameObject.SetActive(false); });


        UiUtil.AddButtonClickEvent(apiPopupBtn, () => { apiPopupOnOff(true); });
        UiUtil.AddButtonClickEvent(apiPopupCloseBtn, () => { apiPopupOnOff(false); });
    }

    public void BackToTitle()
    {
        //TODO : back to title
        MainUIController.EnterRoomInvoke(0);
    }

    public void Testing()
    {
        // MainUIController.InteractInvoke(false);
        // mainCanvas.gameObject.SetActive(false);
        // StartCoroutine(AsyncSceneLoader.LoadSceneCoroutine("ArScene"));
    }

    public void PlaySceneLoading()
    {

        mainCanvas.gameObject.SetActive(false);
        MainUIController.InteractInvoke(false);
        GameController.Instance.GameSceneLoad();

    }

    public void ReturnLobby()
    {
        mainCanvas.gameObject.SetActive(true);
        MainUIController.InteractInvoke(true);

    }


    public void SetMainInteraction(bool isOn)
    {
        mainCanvas.gameObject.SetActive(isOn);
        singleTestBtn.interactable = isOn;
        BackToTitleBtn.interactable = isOn;
        CreateButton.interactable = isOn;
        CancelCreateBtn.interactable = isOn;
        UserInfoBtn.interactable = isOn;
    }

    public void ActiveLogin(int type)
    {
        //loginWebview.WebStart(Domains.GetLogInDomain(type), Screen.width, Screen.height);
        loginWebview.Login();
    }

    public void createRoomOpen()
    {
        if (!Networking.Instance.IsConnected())
        {
            Debug.LogWarning("network check needed");
            return;
        }

        if (GameController.haveToken())
        {
            Debug.LogWarning("login needed");
            ActiveLogin(1);
            return;
        }

        LoginPopup.SetActive(true);
    }

    public async Task CreateRoomBtn()
    {
        string uuid = Guid.NewGuid().ToString();
        string createRoomJson = "{\n" +
                                               "\t\"roomUUID\": \"" + uuid + "\",\n" +
                                               "\t\"title\": \"" + createRoomPopup.inputField.text + "\",\n" +
                                               "\t\"maxCapacity\": " + createRoomPopup.amount.value + ",\n" +
                                               "\t\"gameType\": \"" + (!(CreateRoomPopup.isPersonal) ? "COOPERATE" : "PERSONAL") + "\"\n" +
                                               "}";
        //TODO - send message to create
        await WebSocketService.Instance.SendMessage("/app/room/create", createRoomJson);

        await WebSocketService.Instance.Subscribe("/topic/room/" + uuid);

        GameController.Instance.cur_uuid = uuid;
        MainUIController.RoomEnterEventInvoke();
    }

    public void CancelCreate()
    {
        LoginPopup.SetActive(false);
    }

    private void ActiveUI(int type)
    {
        mainCam.enabled = true;
        //GameController.Instance.cur_uuid = "";
        switch (type)
        {
            case 0: SetMainInteraction(false); break;

            case 1: Init(); SetMainInteraction(true); break;

            default: break;
        }
    }

    public void LoginStatus(bool isOn)
    {
        if (isOn)
        {
            userinfotxt.text = "Login okay";
        }
        else
        {
            userinfotxt.text = "Login needed";
        }
    }


    public void TestPopupOpen()
    {
        testPopup.gameObject.SetActive(true);
    }

    public void apiPopupOnOff(bool isOn)
    {
        apiPopup.gameObject.SetActive(isOn);
    }
    
}
