using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TestWebRequest : MonoBehaviour
{
    [SerializeField]
    private string apiKey = "";
    [SerializeField]
    private string vision_linkg = "";

    public Image sample_img;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void send_request(){
        Texture2D imagetexture = (Texture2D)sample_img.mainTexture;
        byte[] imageBytes = imagetexture.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(imageBytes);
    }
}
