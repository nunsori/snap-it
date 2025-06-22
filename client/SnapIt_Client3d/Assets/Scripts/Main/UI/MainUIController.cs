using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    public static event Action<bool> InteractionEvent;

    public static event Action<int> EnterRoom;

    public static event Action<bool> LoginAction;

    public static event Action<RoomListResponse> updateRoomList;

    public static event Action RoomEnterEvent;

    [SerializeField]
    private TitleBtnEvents titleBtns;

    [SerializeField]
    private MainUIEvent mainUIEvent;


    public static void InteractInvoke(bool isOn)
    {
        InteractionEvent?.Invoke(isOn);
    }

    public static void EnterRoomInvoke(int type)
    {
        EnterRoom?.Invoke(type);
    }

    public static void LoginActionInvoke(bool isOn)
    {
        LoginAction?.Invoke(isOn);
    }

    public static void RoomListUpdateInvoke(RoomListResponse Data)
    {
        updateRoomList?.Invoke(Data);
    }

    public static void RoomEnterEventInvoke()
    {
        Debug.Log("room enter invoke @@@@@@@@@@@@@@@@@@@@@@");
        RoomEnterEvent?.Invoke();
    }



    public void Init()
    {

        titleBtns.Init();
        mainUIEvent.Init();

        MainUIController.InteractInvoke(false);

        EnterRoomInvoke(0);
    }

    
}
