using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestPopup : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField toekn_input;

    [SerializeField]
    private TMP_InputField email_input;


    [SerializeField]
    private Button AdjustBtn;

    void Start()
    {
        UiUtil.AddButtonClickEvent(AdjustBtn, () => { applytoken(); });
    }

    public void applytoken()
    {
        Debug.Log("add toekn");
        GameController.setAcessToken(toekn_input.text);
        GameController.Instance.cur_email = email_input.text;

        // PlayerPrefs에도 저장
        PlayerPrefs.SetString("access_token", toekn_input.text);
        PlayerPrefs.SetString("email", email_input.text);
        PlayerPrefs.Save(); // 즉시 저장 (생략 시 앱 종료 시 저장됨)
    }
}
