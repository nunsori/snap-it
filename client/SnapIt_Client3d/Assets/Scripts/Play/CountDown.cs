using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    public static float countSecond = 15f;

    [SerializeField]
    private TextMeshProUGUI countDownText;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TextMeshProUGUI sliderText;

    private bool isCounting = false;
    private float time = 0f;

    void OnDisable()
    {
        UIController.CountDOwnEvent -= StartCount;
    }

    void OnEnable()
    {
        UIController.CountDOwnEvent += StartCount;
        slider.maxValue = countSecond;
    }

    public void InitCountDown()
    {
        isCounting = false;
        time = 0f;
        slider.maxValue = countSecond;

        countDownText.text = countSecond.ToString("F1");
    }

    public void StartCount(bool isStart)
    {
        if (isStart)
        {
            isCounting = true;
        }
        else
        {
            InitCountDown();
        }
        
    }


    void Update()
    {
        slider.value = countSecond - time;

        if (isCounting)
        {
            time += Time.deltaTime;

            if (countSecond - time <= 0f)
            {
                countDownText.text = "0";
                time = 0f;
                isCounting = false;
                TimeOver();
            }
            else
            {
                countDownText.text = (countSecond - time).ToString("F1");
            }



        }

        
    }

    public void setSliderText()
    {
        sliderText.text = ((int)slider.value).ToString();
    }

    private void TimeOver()
    {
        Debug.Log("time over");
        UIController.CountDownEventInvoke(2);
    }
}
