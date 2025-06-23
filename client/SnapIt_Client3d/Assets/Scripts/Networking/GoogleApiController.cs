using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;


public class GoogleApiController : MonoBehaviour
{

    public static GoogleApiController Instance;

    [SerializeField]
    public static string apiKey = "";
    [SerializeField]
    public static  string vision_linkg = "";

    private string modelName = "gemini-2.0-flash";

    public RawImage sample_img;

    public Camera CameraObj;

    public List<LocalizedObjectAnnotation> resarr;

    private Texture2D tex2D;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        apiKey = GameController.Instance.apikey;
        vision_linkg = GameController.Instance.url;

        //DontDestroyOnLoad(gameObject);
    }
    public void send_btn()
    {
        StartCoroutine(send_request());
    }

    public void send_gemini(string word)
    {
        StartCoroutine(SendGeminiRequestGemini(word));
    }

    public IEnumerator send_request()
    {
        Debug.Log("start_request");
        if (resarr != null)
        {
            resarr.Clear();
        }

        // 1. 이미지 준비: Resources에서 예시 이미지 불러오기 (또는 다른 방법으로 Texture2D 준비)
        //Texture2D tex2D = ConvertWebCamTextureToTexture2D(sample_img.texture as WebCamTexture);
        yield return StartCoroutine(ConvertCameraTextureToTexture2D(CameraObj));
        //sample_img.texture = CameraObj.targetTexture;


        byte[] imageBytes = tex2D.EncodeToPNG();  // EncodeToJPG()도 가능
        string base64Image = System.Convert.ToBase64String(imageBytes).Trim();


        if (tex2D != null)
        {

            Debug.Log("Base64 변환 완료! 길이: " + base64Image.Length);
        }
        else
        {
            Debug.Log("RawImage의 texture는 Texture2D가 아닙니다.");
        }



        // 2. 요청 JSON 구성
        VisionRequests visionRequests = new VisionRequests();
        visionRequests.requests = new List<VisionRequest>();
        VisionRequest request = new VisionRequest();
        request.image = new Image { content = base64Image };
        request.features = new List<Feature>();
        //request.features.Add(new Feature { type = "LABEL_DETECTION", maxResults = 30 });
        request.features.Add(new Feature { type = "OBJECT_LOCALIZATION", maxResults = 30 });
        visionRequests.requests.Add(request);

        string jsonData = JsonUtility.ToJson(visionRequests);
        Debug.Log("json data is : " + jsonData);
        Debug.Log(imageBytes.Length);

        UnityWebRequest webRequest = new UnityWebRequest(vision_linkg + apiKey, "POST");
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(postData);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Vision API] 요청 실패: {webRequest.error}");

        }
        else
        {
            // 5. 응답 처리
            string responseJson = webRequest.downloadHandler.text;
            Debug.Log($"[Vision API] 응답 원본: {responseJson}");

            // JSON 파싱하여 객체로 변환
            VisionResponse visionRes = JsonUtility.FromJson<VisionResponse>(responseJson);


            // 6. 결과 활용: 인식된 객체들 로그 출력
            if (visionRes.responses != null && visionRes.responses[0].localizedObjectAnnotations != null)
            {

                LocalizedObjectAnnotation[] objects = visionRes.responses[0].localizedObjectAnnotations;
                foreach (var obj in objects)
                {
                    string objectName = obj.name;
                    float confidence = obj.score;
                    NormalizedVertex[] vertices = obj.boundingPoly.normalizedVertices;
                    Debug.Log(string.Format("객체: {0}, 신뢰도: {1:P1}", objectName, confidence));
                    // 정규화 좌표 -> 실제 픽셀 좌표 변환 (이미지 크기 이용)
                    if (vertices != null && vertices.Length > 0)
                    {
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float px = vertices[i].x * sample_img.texture.width;
                            float py = vertices[i].y * sample_img.texture.height;
                            Debug.Log(string.Format(" - vertex {0}: ({1:F1}, {2:F1}) px", i, px, py));
                        }
                    }
                    resarr.Add(obj);
                }
            }
        }
    }

    public Texture2D ConvertWebCamTextureToTexture2D(WebCamTexture webcamTexture)
    {
        // WebCamTexture에서 현재 프레임 픽셀 가져오기
        Texture2D tex2D = new Texture2D(webcamTexture.width, webcamTexture.height);
        tex2D.SetPixels(webcamTexture.GetPixels());  // 픽셀 복사
        tex2D.Apply();  // 실제 적용

        return tex2D;
    }

    public IEnumerator ConvertCameraTextureToTexture2D(Camera camera)
    {
        yield return new WaitForEndOfFrame();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;
        Debug.Log(rt);
        camera.Render();
        RenderTexture.active = rt;

        Texture2D tex2Ds = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex2Ds.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex2Ds.Apply();

        sample_img.texture = tex2Ds;

        camera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

        tex2D = tex2Ds;
    }

    private IEnumerator SendGeminiRequestGemini(string word)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        // 요청 body 구성
        string jsonBody = @"
        {
            ""contents"": [
                {
                    ""parts"": [
                        {
                            ""text"": ""Give a object name in English and Korean word that can be in this place " + word + @", and the word must be simple""
                        }
                    ]
                }
            ],
            ""generationConfig"": {
        ""responseMimeType"": ""application/json"",
        ""responseSchema"": {
          ""type"": ""ARRAY"",
          ""items"": {
            ""type"": ""OBJECT"",
            ""properties"": {
              ""ObjectNameEng"": { ""type"": ""STRING"" },
              ""ObjectNameKor"": { ""type"": ""STRING"" }
            },
            ""propertyOrdering"": [""ObjectNameEng"", ""ObjectNameKor""]
          }
        }
      }
            
        }";

        Debug.Log("json body is " + jsonBody);
        // UTF-8 Encoding
        byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

        // UnityWebRequest 생성
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return request.SendWebRequest();

        // 응답 처리
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);


            GameController.Instance.cur_word = GetRandomElement(ExtractObjectPairs(request.downloadHandler.text));
            Debug.Log("result is : " + GameController.Instance.cur_word);
            //TODO : scan btn 활성화, 그리고 카운트다운시작, ui update하기

            //카운트다운시작하기, scanbtn활성화하기
            StateTester.checkState = true;
            WordList.BtnInteractable = true;

            UIController.Instance.SetRoundInfo(GameController.Instance.cur_round.ToString(), GameController.Instance.cur_word.Split('-')[1]);

            UIController.ScanInvoke(true);

            UIController.CountDownEventInvoke(1);
        }
        else
        {
            Debug.LogError("Request Failed: " + request.error);
        }



    }


    public static List<string> ExtractObjectPairs(string json)
    {
        // 1️⃣ Step1: candidates[0].content.parts[0].text 추출
        CandidateResponse root = JsonUtility.FromJson<CandidateResponse>(json);

        if (root == null || root.candidates == null || root.candidates.Length == 0)
        {
            Debug.LogError("Failed to parse root candidates.");
            return null;
        }

        var firstPart = root.candidates[0].content.parts[0].text;

        Debug.Log("Extracted text: " + firstPart);

        // 2️⃣ Step2: text는 JSON string → 다시 파싱
        // JsonUtility는 배열 파싱에 제약이 있으므로 MiniJSON 또는 Newtonsoft.Json 사용하는 것이 좋음.
        // 여기서는 간단하게 JsonUtility로 하기 위해 Wrapper 사용

        // Wrapper 사용
        string wrappedText = "{\"objects\": " + firstPart + "}";

        ObjectWrapper wrapper = JsonUtility.FromJson<ObjectWrapper>(wrappedText);

        List<string> result = new List<string>();

        foreach (var obj in wrapper.objects)
        {
            result.Add($"{obj.ObjectNameEng}-{obj.ObjectNameKor}");
        }

        return result;
    }
    
    public static string GetRandomElement(List<string> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("List is null or empty!");
            return null;
        }

        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }

}


