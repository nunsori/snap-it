using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;


public class PlayingController : MonoBehaviour
{
    public static PlayingController Instance;

    public UIController uIController;
    public ARController aRController;
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
        uIController.Init();
        aRController.Init();

        StateTester.checkState = false;
        UIController.ScanInvoke(false);

        Debug.Log("enter room : " + GameController.Instance.cur_uuid);
        //StartCoroutine(PostRequest("https://chabin37.iptime.org:32766/app/room/join?token=" + GameController.getAcessToken()));
        WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/join", "{}"); //"{\"roomUUID\": \""+ GameController.Instance.cur_uuid +"\"}"

        
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator PostRequest(string url)
    {
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"roomUUID\": \"" + GameController.Instance.cur_uuid + "\"}");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            //TODO : quit room;
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);

        }
    }
}
