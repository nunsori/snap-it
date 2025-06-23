using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class APiISettingPopup : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField toekn_input;

    [SerializeField]
    private TMP_InputField url_input;

    [SerializeField]
    private Button AdjustBtn;

    void Start()
    {
        UiUtil.AddButtonClickEvent(AdjustBtn, () => {SoundController.Instance.Play_Effect("click", SoundController.Instance.Effect_Volume, false); applytoken(); });

        url_input.text = "https://vision.googleapis.com/v1/images:annotate?key=";
    }


    public void applytoken()
    {
        Debug.Log("add toekn");
        GameController.Instance.apikey = toekn_input.text;
        GameController.Instance.url = url_input.text;

        
    }

}
