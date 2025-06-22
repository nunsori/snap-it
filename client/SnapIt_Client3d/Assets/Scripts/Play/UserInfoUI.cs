using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserInfoUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI info;

    [SerializeField]
    private UnityEngine.UI.Image image;
    private string userEmail = "";
    

    public void Init(string text, string email)
    {
        //color 초기화
        if (userEmail != GameController.Instance.cur_email)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.yellow;
        }
        
        info.text = text;
        userEmail = email;
    }

    public void ChangeInfo(string text)
    {
        //color 변경
        if (userEmail != GameController.Instance.cur_email)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.yellow;
        }
        
        info.text = text;
    }

    public void InitColor()
    {
        if (userEmail != GameController.Instance.cur_email)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.yellow;
        }

    }

    public string GetEmail()
    {
        return userEmail;
    }

}
