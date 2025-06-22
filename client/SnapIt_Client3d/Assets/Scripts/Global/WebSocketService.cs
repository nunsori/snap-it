using UnityEngine;
using NativeWebSocket;
// using Best.WebSockets;
// using Best.HTTP.Request.Authentication;
// using Best.STOMP;
// using Best.STOMP.Builders;
using System.Text;
using System;
using System.Threading.Tasks;

public class WebSocketService : MonoBehaviour
{
    public static WebSocketService Instance;
    //private Client stompClient;

    [SerializeField]
    private string WebSocketLink = "";


    private WebSocket websocket;

    private Coroutine reconnectCoroutine;
    private Coroutine pingCoroutine;

    private float networkLossTimer = 0f;
    private float socketFailTimer = 0f;
    private const float NetworkTimeout = 5f;
    private const float SocketTimeout = 5f;
    private const float PingInterval = 10f;

    private string lastConnectUrl = "";

    private int id_count = 0;
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

    private void OnDestroy()
    {
        Networking.NetworkChangeAction -= OnNetworkChanged;
        StopAllCoroutines();
    }

    public void StartConnect(string link)
    {
        Networking.NetworkChangeAction += OnNetworkChanged;
        InitWebSocket(link);
    }

    public async void InitWebSocket(string link)
    {
        if(websocket != null){ CloseSocket(); };
        websocket = new WebSocket(link);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected.");
            lastConnectUrl = link;
            SendStompConnect();
        };

        websocket.OnMessage += ReceiveMessage;

        websocket.OnError += (e) => Debug.LogError("WebSocket Error: " + e);
        websocket.OnClose += (e) => Debug.LogWarning("WebSocket Closed");

        try
        {
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }


        // stompClient = new Client();

        // // 이벤트 핸들러 등록
        // stompClient.OnConnected += OnConnected;
        // stompClient.OnDisconnected += OnDisconnected;
        // stompClient.OnFrame += OnFrame;



        // //연결 파라미터 설정

        // var credentials = new Credentials(AuthenticationTypes.Basic, "accessToken", GameController.getAcessToken());

        // var token = GameController.getAcessToken();

        // var parameters = new ConnectParametersBuilder()
        //     .WithHost("chabin37.iptime.org", 32766) // 포트는 필요에 따라 조정
        //     .WithTransport(SupportedTransports.WebSocket)
        //     //.WithPath("/ws?token=" + GameController.getAcessToken())
        //     .WithPath($"/ws?token={token}")
        //     //.WithVirtualHost("/") // 필요 시 설정
        //     //.WithCredentials(credentials)
        //     //.WithHeader("Sec-WebSocket-Protocol", "token=" + GameController.getAcessToken())
        //     //.WithHeader("Authorization", "Bearer " + GameController.getAcessToken())
        //     .WithHeartBeat(TimeSpan.Zero,TimeSpan.Zero,TimeSpan.Zero)
        //     .WithHeader("accept-version", "1.2")
        //     .WithHeader("host", "chabin37.iptime.org")
        //     //.WithVirtualHost("/") // 필요 시 설정
        //     .Build();

        // // STOMP 서버에 연결 시도
        // stompClient.BeginConnect(parameters);
    }

    // Update is called once per frame
    void Update()
    {
        //         if (websocket != null)
        //         {
        // #if !UNITY_WEBGL || UNITY_EDITOR
        //             websocket.DispatchMessageQueue();
        // #endif
        //         }
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            //Debug.Log("websocket status is open");
            websocket.DispatchMessageQueue();
        }

        // 네트워크 연결 여부 확인
        if (!Networking.Instance.IsConnected())
        {
            networkLossTimer += Time.deltaTime;
            if (networkLossTimer > NetworkTimeout)
            {
                //Debug.LogError("네트워크 연결이 5초 이상 끊어졌습니다. 웹소켓 종료.");
                CloseSocket();
            }
        }
        else
        {
            networkLossTimer = 0f;

            // 웹소켓 연결 상태 확인
            if (websocket != null && websocket.State != WebSocketState.Open)
            {
                socketFailTimer += Time.deltaTime;
                if (socketFailTimer > SocketTimeout)
                {
                    Debug.LogError("웹소켓 서버 응답 없음. 서버 오류 가능성 있음.");
                    socketFailTimer = 0f;
                }
            }
            else
            {
                socketFailTimer = 0f;
            }
        }


    }

    public async void SubCribe(string destination)
    {
        if (websocket.State == WebSocketState.Open)
        {

        }
        else
        {
            Debug.LogWarning("websocket is not open");
        }

    }

    public async void testGUI()
    {
        string createRoomJson = "{\n" +
                                               "\t\"roomUUID\": \"123e4567-e89b-12d3-a456-426614174000\",\n" +
                                               "\t\"title\": \"방 이름 1\",\n" +
                                               "\t\"maxCapacity\": 8,\n" +
                                               "\t\"gameType\": \"COOPERATE\"\n" +
                                               "}";

        Debug.Log("12345");
        await SendMessage("/app/room/create", createRoomJson);
        Debug.Log("123456");
        await Subscribe("topic/room/123e4567-e89b-12d3-a456-426614174000");
        Debug.Log("123457");
    }

    public async void testGUI2()
    {
        string createRoomJson = "{\n" +
                                               "\t\"roomUUID\": \"123e4567-e89b-12d3-a456-426614174000\",\n" +
                                               "\t\"title\": \"방 이름 1\",\n" +
                                               "\t\"maxCapacity\": 8,\n" +
                                               "\t\"gameType\": \"COOPERATE\"\n" +
                                               "}";
        await SendMessage("/app/room/list", "{}");
        // await websocket.SendText("!@#$%^&*()");
        // await websocket.SendText("");
        //await SendMessage("/app/openrooms", "{}");
    }

    private void OnNetworkChanged(bool isOnline)
    {
        if (isOnline)
        {
            Debug.Log("네트워크 복구됨. 웹소켓 재연결 시도.");
            //TryConnectSocket();
            InitWebSocket(lastConnectUrl);
        }
        else
        {
            Debug.LogWarning("네트워크 끊김 감지. 웹소켓 종료.");
            CloseSocket();
        }
    }

    // private async void TryConnectSocket()
    // {

    //     if (websocket.State != WebSocketState.Connecting)
    //     {
    //         try
    //         {
    //             await websocket.Connect();
    //             Debug.Log("웹소켓 연결 성공");
    //         }
    //         catch (System.Exception e)
    //         {
    //             Debug.LogError($"웹소켓 연결 실패: {e.Message}");
    //             // 필요하면 재시도, 대기 타이머, UI 표시 등
    //         }
    //     }
    // }

    private void CloseSocket()
    {
        if (websocket != null)
        {
            websocket.Close();
            websocket = null;
        }
        StopPingRoutine();
    }

    private void StartPingRoutine()
    {
        if (pingCoroutine != null) StopCoroutine(pingCoroutine);
        //pingCoroutine = StartCoroutine(PingLoop());
    }

    private void StopPingRoutine()
    {
        if (pingCoroutine != null)
        {
            StopCoroutine(pingCoroutine);
            pingCoroutine = null;
        }
    }


    private void OnApplicationQuit()
    {
        CloseSocket();
    }



    #region startline


    private async void SendStompConnect()
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }
        string connectFrame =
            "CONNECT\n" +
            "accept-version:1.2\n" +
            "host:chabin37.iptime.org\n" +
            "heart-beat:1000,1000\n\n" +
            "\0";

        Debug.Log("Sending CONNECT frame:\n" + connectFrame.Replace("\0", "\\0"));
        await websocket.SendText(connectFrame);

        

    }

    public async Task Subscribe(string destination)
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }
        string id = "";
        if (destination == "/topic/openrooms")
        {
            id = GameController.getAcessToken();
        }
        else if (destination.Contains("/topic/room/"))
        {
            id = GameController.getAcessToken() + "second" + id_count;
            id_count++;
        }

        string subscribeFrame =
            "SUBSCRIBE\n" +
            "id:" + id + "\n" +
            $"destination:{destination}\n" +
            "ack:auto\n" +
            "receipt:sub-0-receipt\n\n" +
            "\0";

        Debug.Log("Sending SUBSCRIBE frame:\n" + subscribeFrame.Replace("\0", "\\0"));
        await websocket.SendText(subscribeFrame);
    }

    public async Task Unsubscribe(string destination)
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }

        string id = "";
        if (destination == "/topic/openrooms")
        {
            id = GameController.getAcessToken();
        }
        else if (destination.Contains("/topic/room/"))
        {
            id = GameController.getAcessToken() + "second" + (id_count-1);
        }

        string unsubscribeFrame =
            "UNSUBSCRIBE\n" +
            "id:" + id + "\n\n" +
            "\0";

        Debug.Log("Sending UNSUBSCRIBE frame:\n" + unsubscribeFrame.Replace("\0", "\\0"));
        await websocket.SendText(unsubscribeFrame);
    }

    public async Task SendMessage(string destination, string body)
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }

        string sendFrame2 =
            $"SEND\n" +
            $"destination:{destination}\n" +
            $"content-type:application/json\n" +
            $"content-length:{Encoding.UTF8.GetByteCount(body)}\n\n" +
            $"{body}\0";

        string sendFrame = "SEND\n" +
                                            "destination:" + destination + "\n" +
                                            "content-type:application/json;charset=utf-8\n" +
                                            "content-length:" + Encoding.UTF8.GetByteCount(body) + "\n" +
                                            "\n" +
                                            body + "\0";


        Debug.Log("Sending STOMP SEND frame:\n" + sendFrame.Replace("\0", "\\0"));
        await websocket.SendText(sendFrame);
    }

    public async Task SendMessageW2V(string destination, string body, string firstWord, string secondWord)
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }

        string sendFrame = "SEND\n" +
                                            "destination:" + destination + "\n" +
                                            "first_word:" + firstWord + "\n" +
                                            "second_word:" + secondWord + "\n" +
                                            "content-length:" + Encoding.UTF8.GetByteCount(body) + "\n" +
                                            "\n" +
                                            body + "\0";


        Debug.Log("Sending STOMP SEND frame:\n" + sendFrame.Replace("\0", "\\0"));
        await websocket.SendText(sendFrame);
    }


    private void Disconnect()
    {
        if (!Networking.Instance.IsConnected())
        {
            return;
        }
        string disconnectFrame = "DISCONNECT\n\n\0";
        websocket.SendText(disconnectFrame);
        websocket.Close();
    }

    private async void ReceiveMessage(byte[] bytes)
    {
        string msg = Encoding.UTF8.GetString(bytes);
        Debug.Log("Received: " + msg);

        if (msg.StartsWith("CONNECTED"))
        {
            Debug.Log("STOMP Connected.");
            // Subscribe("/ws");
            //Subscribe("/topic/openrooms");
            await Subscribe("/topic/openrooms");

            await SendMessage("/app/room/list", "{}");
        } else if (msg.StartsWith("MESSAGE")){
            MessageDistributer.DistributeMessage(msg);
        }
    }
    


    #endregion
}
