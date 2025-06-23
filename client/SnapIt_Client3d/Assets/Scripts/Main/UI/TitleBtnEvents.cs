using UnityEngine;
using UnityEngine.UI;

public class TitleBtnEvents : MonoBehaviour
{
    [SerializeField]
    private Canvas TitleCanvas;

    [SerializeField]
    private Button EnterBtn;

    [SerializeField]
    private Button QuitBtn;

    void OnDisable()
    {
        MainUIController.InteractionEvent -= SetMainInteraction;
        MainUIController.EnterRoom -= EnterRoom;
        MainUIController.RoomEnterEvent -= EnterGame;
    }

    void OnEnable()
    {
        MainUIController.InteractionEvent += SetMainInteraction;
        MainUIController.EnterRoom += EnterRoom;
        MainUIController.RoomEnterEvent += EnterGame;
    }

    public void Init()
    {
        TitleCanvas.gameObject.SetActive(true);
        


        UiUtil.AddButtonClickEvent(EnterBtn, () => { EnterMain(); });
        UiUtil.AddButtonClickEvent(QuitBtn, () => { ExitApp(); });
    }



    public void EnterMain()
    {
        //TODO : room list load
        MainUIController.EnterRoomInvoke(1);
    }

    public void ExitApp()
    {
        //TODO : quit
        Application.Quit();
    }

    public void SetMainInteraction(bool isOn)
    {
        TitleCanvas.gameObject.SetActive(isOn);
        EnterBtn.interactable = isOn;
        QuitBtn.interactable = isOn;
    }

    public void EnterRoom(int type)
    {
        switch (type)
        {
            case 0: Init(); SetMainInteraction(true); break;

            case 1: SetMainInteraction(false); break;

            default: break;
        }
    }

    public void EnterGame()
    {
        SetMainInteraction(false);
        TitleCanvas.gameObject.SetActive(false);

    }

    public void ReturnLobby()
    {
        SetMainInteraction(true);
        TitleCanvas.gameObject.SetActive(true);
    }
}
