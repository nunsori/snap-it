using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject PopupObject;

    [SerializeField]
    private Image BGImage;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI userText;

    [SerializeField]
    private TextMeshProUGUI ScoreText;

    [SerializeField]
    private TextMeshProUGUI UserScoreText;

    [SerializeField]
    private Button ContinueBtn;

    [SerializeField]
    private Button ExitBtn;

    [SerializeField]
    private TextMeshProUGUI[] wordTexts;

    public void Init()
    {
        UiUtil.AddButtonClickEvent(ContinueBtn, () => { ContinueBtnEvent(); });
        UiUtil.AddButtonClickEvent(ExitBtn, () => { ExitBtnEvent(); });

        DisablePopup();
    }

    public void DisablePopup()
    {
        PopupObject.SetActive(false);
    }

    public void StartPopup(bool isEnd, string user, string Score, string UserScore, int round)
    {
        PopupObject.SetActive(true);

        titleText.text = (GameController.Instance.MaxRound == round) ? "게임 결과" : "라운드 결과";

        userText.text = user;

        ScoreText.text = Score;

        UserScoreText.text = UserScore;

        // for (int i = 0; i < wordTexts.Length; i++)
        // {
        //     if (wordTexts == null) continue;
        //     wordTexts[i].text = worlds[i];
        // }

        ContinueBtn.gameObject.SetActive(!(GameController.Instance.MaxRound == round));
        // ExitBtn.gameObject.SetActive(GameController.Instance.MaxRound == round);
    }

    public async void ContinueBtnEvent()
    {
        // TimeOverInfo overInfo = new TimeOverInfo
        // {
        //     timeOver = true,
        //     gameType = GameController.Instance.cur_game_type,
        //     round = GameController.Instance.cur_round
        // };
        //await WebSocketService.Instance.SendMessage("/app/room/" + GameController.Instance.cur_uuid + "/end", JsonUtility.ToJson(overInfo));
        if (GameController.Instance.roundStartMessage == null)
        {
            return;
        }
        MessageDistributer.startRoundTrigger();

        DisablePopup();
    }

    public void ExitBtnEvent()
    {
        UIController.Instance.btnEvents.QuitPlayScene();
        DisablePopup();
        //Quit하기
    }
}
