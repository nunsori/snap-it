using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextChange : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private Slider slider;

    public void changeText()
    {
        text.text = slider.value.ToString();
    }
}
