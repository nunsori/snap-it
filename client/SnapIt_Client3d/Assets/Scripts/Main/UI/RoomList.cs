using System.Collections.Generic;
using UnityEngine;

public class RoomList : MonoBehaviour
{
    [SerializeField]
    private GameObject roomListPrefab;

    [SerializeField]
    private GameObject roomListParent;

    private List<RoomUI> roomList = new List<RoomUI>();

    void OnDisable()
    {
        MainUIController.updateRoomList -= UpdateRoomList;
    }

    void OnEnable()
    {
        MainUIController.updateRoomList += UpdateRoomList;
    }

    public void Init()
    {
        //MainUIController.updateRoomList += UpdateRoomList;
        //roomList.ForEach((element) => { Destroy(element.gameObject); });
    }

    public void UpdateRoomList(RoomListResponse Data)
    {
        string title;
        GameObject tempobj;
        RoomUI tempUI;

        roomList.ForEach((element) => { if(element!= null) Destroy(element.gameObject); });
        roomList.Clear();

        Data.body.roomList.ForEach((element) =>
        {
            title = element.title + "\n" + element.currentCapacity + " / " + element.maxCapacity + "\n" + element.gameType;

            tempobj = Instantiate(roomListPrefab, Vector3.zero, Quaternion.identity, roomListParent.transform);
            tempUI = tempobj.GetComponent<RoomUI>();

            tempUI.Init(element.title, element.currentCapacity + "/" + element.maxCapacity, element.gameType, element.roomUUID);

            roomList.Add(tempUI);
            
            
        });
    }
}
