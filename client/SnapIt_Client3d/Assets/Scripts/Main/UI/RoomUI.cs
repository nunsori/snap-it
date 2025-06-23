using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI RoomInfoText;

    [SerializeField]
    private Button EnterBtn;

    [SerializeField]
    private TextMeshProUGUI room_type;

    [SerializeField]
    private TextMeshProUGUI waiting_info;

    private string uuid;

    public void Init(string roomTitle, string roomAmount, string roomType, string uuids)
    {
        // string[] lines = text.Split('\n');

        // // 각 줄을 변수에 담기
        // string roomTitle = lines.Length > 0 ? lines[0] : "";
        // string roomAmount = lines.Length > 1 ? lines[1] : "";
        // string roomType = lines.Length > 2 ? lines[2] : "";


        RoomInfoText.text = roomTitle;
        room_type.text = roomType;
        waiting_info.text = roomAmount;

        uuid = uuids;

        UiUtil.AddButtonClickEvent(EnterBtn, async() => {Debug.Log("click;;;"); await WebSocketService.Instance.Subscribe("/topic/room/" + uuid); GameController.Instance.cur_uuid = uuid;  MainUIController.RoomEnterEventInvoke();});
    }


}
