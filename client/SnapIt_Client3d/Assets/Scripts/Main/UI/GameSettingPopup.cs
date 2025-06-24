using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameSettingPopup : MonoBehaviour
{
    [Header("Popup Basic")]
    [SerializeField]
    private GameObject popupObj;

    [SerializeField]
    private Button closeBtn;


    [Header("Popup Content")]
    [SerializeField]
    private Slider BGM_Volume_slider;

    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private Slider Effect_Volume_slider;

    [SerializeField]
    private Slider Scaler_slider;


    void Start()
    {
        
    }

    public void Init()
    {
        UiUtil.AddButtonClickEvent(closeBtn, () => { SoundController.Instance.Play_Effect("click", SoundController.Instance.Effect_Volume, false); PopupOnoff(false); });
        popupObj.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -2500, 0);
        BGM_Volume_slider.value = CountDown.countSecond;//SoundController.Instance.BGM_Volume;
        Effect_Volume_slider.value = SoundController.Instance.Effect_Volume;
        Scaler_slider.value = ObjectDetector.scaleFactor;

        popupObj.SetActive(false);
    }

    public void PopupOnoff(bool isOn)
    {
        if (isOn)
        {
            popupObj.SetActive(true);
            var seq = DOTween.Sequence();
            seq.Append(popupObj.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack)).OnComplete(() => { });
        }
        else
        {
            var seq = DOTween.Sequence();
            seq.Append(popupObj.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-2500, 0.5f).SetEase(Ease.InBack)).OnComplete(() => { popupObj.SetActive(false); });
        }
    }


    public void changeSetting()
    {
        //SoundController.Instance.BGM_Volume = BGM_Volume_slider.value;
        timerText.text = BGM_Volume_slider.value.ToString();
        CountDown.countSecond = BGM_Volume_slider.value;
        SoundController.Instance.Effect_Volume = Effect_Volume_slider.value;

        ObjectDetector.scaleFactor = Scaler_slider.value;
    }


}
