using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BtnEvents : MonoBehaviour
{
    [SerializeField]
    private Canvas BtnCanvas;



    [SerializeField]
    private Button QuitBtn;

    [SerializeField]
    private Button GameStartBtn;

    [SerializeField]
    private Camera camera;

    private void OnDisable()
    {
        UIController.ButtonInteract -= ButtonInteract;
        GameController.GameStartEvent -= playButtonActiveFalse;
    }

    void OnEnable()
    {
        UIController.ButtonInteract += ButtonInteract;
        GameController.GameStartEvent += playButtonActiveFalse;
    }

    public void Init()
    {
        camera.gameObject.SetActive(true);

        GameStartBtn.gameObject.SetActive(true);

        UiUtil.AddButtonClickEvent(QuitBtn, () => { QuitPlayScene(); });
        UiUtil.AddButtonClickEvent(GameStartBtn, () => { GameStart(); });
    }


    public void ButtonInteract(bool isOn)
    {
        QuitBtn.interactable = isOn;
    }

    public void QuitPlayScene()
    {
        MainUIController.EnterRoomInvoke(1);
        UIController.Instance.countDown.InitCountDown();
        WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/leave", "{}");
        WebSocketService.Instance.Unsubscribe("/topic/room/" + GameController.Instance.cur_uuid);
        GameController.Instance.cur_uuid = "";
        GameController.Instance.cur_word = "";
        GameController.Instance.cur_game_type = "";
        GameController.Instance.cur_round = 0;
        //SceneManager.UnloadSceneAsync("ArScene");
        camera.gameObject.SetActive(false);

    }

    public async void GameStart()
    {
        GameStartBtn.gameObject.SetActive(false);
        await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/start", "{}");
    }

    public void playButtonActiveFalse() {
        GameStartBtn.gameObject.SetActive(false);
    }

}
