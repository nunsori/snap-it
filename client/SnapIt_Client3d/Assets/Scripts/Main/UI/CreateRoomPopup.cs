using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPopup : MonoBehaviour
{
    [SerializeField]
    public TMP_InputField inputField;

    [SerializeField]
    public Slider amount;

    [SerializeField]
    public Toggle mode;

    [SerializeField]
    public Button btn1;

    [SerializeField]
    public Button btn2;

    public static bool isPersonal = false;

    void OnEnable()
    {
        if (btn1 != null)
        {
            UiUtil.AddButtonClickEvent(btn1, () => { isPersonal = true; });
        }

        if (btn2 != null)
        {
            UiUtil.AddButtonClickEvent(btn2, () => { isPersonal = false; });
        }
    }


}
