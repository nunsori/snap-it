using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class webcam_controller : MonoBehaviour
{
    public RawImage image;

    WebCamDevice[] webcam_arr;
    WebCamTexture webcam_texture;

    public TextMeshProUGUI cur_index;
    

    private int index_ = 0;

    public TMP_InputField[] inputFields;

    // Start is called before the first frame update
    void Start()
    {
        index_ = 0;
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        
        inputFields[0].text = image.uvRect.x.ToString();
        inputFields[1].text = image.uvRect.y.ToString();
        inputFields[2].text = image.uvRect.width.ToString();
        inputFields[3].text = image.uvRect.height.ToString();

        inputFields[4].text = image.transform.localScale.x.ToString();
        inputFields[5].text = image.transform.localScale.y.ToString();
        //Init(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void on_off(){
        if(webcam_texture != null && webcam_texture.isPlaying){
            webcam_texture.Stop();
            image.texture = null;
        }else{
            Init(index_);
        }
    }


    public void Init(int index__){
        webcam_arr = WebCamTexture.devices;
        index_ += index__;

        if(index_ < 0 || index_ >= webcam_arr.Length){
            if(index_ < 0){
                index_ = webcam_arr.Length - 1;
            }else{
                index_ = 0;
            }
            //return;
        }


        //index_ = 0;

        webcam_texture = new WebCamTexture(webcam_arr[index_].name,300,300,15);
        webcam_texture.filterMode = FilterMode.Trilinear;

        image.texture = webcam_texture;

        webcam_texture.Play();

        cur_index.text = "device_count : " + webcam_arr.Length +"\n\ncur index : " + index_.ToString() + "\n\ndevice name : \n" + webcam_arr[index_].name
        + "\n\nveritcally_mirrored : \n" + webcam_texture.videoVerticallyMirrored;
    }

    public void change_uv_rect(){
        image.uvRect = new Rect(float.Parse(inputFields[0].text),float.Parse(inputFields[1].text),float.Parse(inputFields[2].text),float.Parse(inputFields[3].text));
    }

    public void change_scale(){
        image.transform.localScale = new Vector3(float.Parse(inputFields[4].text),float.Parse(inputFields[5].text),1f);
    }
}
