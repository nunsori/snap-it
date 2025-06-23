using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering.Universal;
using System.ComponentModel;

public class TestWebRequest : MonoBehaviour
{
    [SerializeField]
    private string apiKey = "";
    [SerializeField]
    private string vision_linkg = "";

    public RawImage sample_img;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void send_btn(){
        StartCoroutine(send_request());
    }

    IEnumerator send_request(){
        Debug.Log("start_request");

        // 1. 이미지 준비: Resources에서 예시 이미지 불러오기 (또는 다른 방법으로 Texture2D 준비)

        Texture2D tex2D = ConvertWebCamTextureToTexture2D(sample_img.texture as WebCamTexture);

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

        if (webRequest.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"[Vision API] 요청 실패: {webRequest.error}");

        } else {
            // 5. 응답 처리
            string responseJson = webRequest.downloadHandler.text;
            Debug.Log($"[Vision API] 응답 원본: {responseJson}");

            // JSON 파싱하여 객체로 변환
            VisionResponse visionRes = JsonUtility.FromJson<VisionResponse>(responseJson);

            // 6. 결과 활용: 인식된 객체들 로그 출력
            if (visionRes.responses != null && visionRes.responses.Length > 0) {
                LocalizedObjectAnnotation[] objects = visionRes.responses[0].localizedObjectAnnotations;
                foreach (var obj in objects) {
                    string objectName = obj.name;
                    float confidence = obj.score;
                    NormalizedVertex[] vertices = obj.boundingPoly.normalizedVertices;
                    Debug.Log(string.Format("객체: {0}, 신뢰도: {1:P1}", objectName, confidence));
                    // 정규화 좌표 -> 실제 픽셀 좌표 변환 (이미지 크기 이용)
                    if (vertices != null && vertices.Length > 0) {
                        for (int i = 0; i < vertices.Length; i++) {
                            float px = vertices[i].x * sample_img.texture.width;
                            float py = vertices[i].y * sample_img.texture.height;
                            Debug.Log(string.Format(" - vertex {0}: ({1:F1}, {2:F1}) px", i, px, py));
                        }
                    }
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
}
