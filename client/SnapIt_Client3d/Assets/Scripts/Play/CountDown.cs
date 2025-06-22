using TMPro;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    [SerializeField]
    private float countSecond = 15f;

    [SerializeField]
    private TextMeshProUGUI countDownText;

    private bool isCounting = false;
    private float time = 0f;

    void OnDisable()
    {
        UIController.CountDOwnEvent -= StartCount;
    }

    void OnEnable()
    {
        UIController.CountDOwnEvent += StartCount;
    }

    public void InitCountDown()
    {
        isCounting = false;
        time = 0f;

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

    private void TimeOver()
    {
        Debug.Log("time over");
        UIController.CountDownEventInvoke(2);
    }
}
