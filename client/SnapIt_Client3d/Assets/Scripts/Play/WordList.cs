using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WordList : MonoBehaviour
{
    public static WordList Instance;
    public static List<string> word_list;

    public static bool BtnInteractable = false;

    [SerializeField]
    private GameObject BtnPref;

    [SerializeField]
    private GameObject BtnListParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BtnInteractable = false;
    }



    public void ResetWord()
    {
        for (int i = 1; i < BtnListParent.transform.childCount; i++)
        {
            Destroy(BtnListParent.transform.GetChild(i).gameObject);
        }
    }

    public void AddWord(string word)
    {
        GameObject obj = Instantiate(BtnPref, Vector3.zero, Quaternion.identity, BtnListParent.transform);
        obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = word;
        UiUtil.AddButtonClickEvent(obj.GetComponent<Button>(), () => { if (!BtnInteractable) return; sendWord(word); Destroy(obj); });

    }

    public void sendWord(string word)
    {
        if (!BtnInteractable) return;
        BtnInteractable = false;
        Debug.Log("send word" + word.Split('-')[0].Replace(" ",""));

        WebSocketService.Instance.SendMessageW2V("/app/room/" + GameController.Instance.cur_uuid + "/similarity", "{}", GameController.Instance.cur_word.Split('-')[0], word.Split('-')[0].Replace(" ",""));
    }
}
