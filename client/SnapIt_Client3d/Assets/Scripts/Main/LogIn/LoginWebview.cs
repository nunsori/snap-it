using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Collections;

public class LoginWebview : MonoBehaviour
{
    private UniWebView webView = null;

    [SerializeField]
    private GameObject targetObj;

    [SerializeField]
    bool chacheClear = false;

    public void Start()
    {
        
    }


    public void WebStart(string targetlink, float width, float height)
    {
        if (webView != null)
        {
            Destroy(webView);
        }
        webView = targetObj.AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, width, height);
        webView.ReferenceRectTransform = targetObj.GetComponent<RectTransform>();
        webView.SetZoomEnabled(false);
        webView.SetAllowFileAccessFromFileURLs(true); // 테스트 목적
        // webView.SetUserAgent(
        //     "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1"
        // );

        string js = @"
            if (document.querySelector('meta[name=viewport]') == null) {
                var meta = document.createElement('meta');
                meta.name = 'viewport';
                meta.content = 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no';
                document.head.appendChild(meta);
            }
        ";
        webView.EvaluateJavaScript(js);
        
        if (chacheClear)
            UniWebView.ClearCookies(() => { Debug.Log("캐시클리어"); });



        //webView = gameObject.AddComponent<UniWebView>();
        //webView.Frame = new Rect(0, 0, width, height);
        //webView.Frame = new uniwebviewedge
        //webView.SetTransform

        webView.OnPageFinished += OnPageFinished;
        webView.OnPageStarted += OnPageStarted;
        webView.OnMessageReceived += (view, message) =>
        {
            // if (message.Path == "login_success") {
            //     string token = message.Args["token"];
            //     Debug.Log("로그인 완료! 토큰: " + token);
            // }
            Debug.Log("on message : " + message);
        };



        webView.Load(targetlink);  // 구글 로그인 URL
        webView.Show();


    }

    private void OnPageStarted(UniWebView webView, string url)
    {
        Debug.Log("Started loading: " + url);
    }

    public void Login()
    {

        // 1. PlayerPrefs에서 저장된 값 불러오기
        string savedToken = PlayerPrefs.GetString("access_token", "");
        string savedEmail = PlayerPrefs.GetString("email", "");

        // 2. 값이 없으면 로그인 중단
        if (string.IsNullOrEmpty(savedToken) || string.IsNullOrEmpty(savedEmail))
        {
            Debug.LogWarning("PlayerPrefs에 저장된 토큰 또는 이메일이 없습니다. 로그인 중단.");
            return;
        }

        //GameController.setAcessToken(GameController.Instance.tok);
        // if (GameController.getAcessToken() == "" || GameController.Instance.cur_email == "")
        // {
        //     Debug.LogWarning("이메일과 토큰 설정필요");
        //     return;
        // }
        GameController.setAcessToken(savedToken);
        GameController.Instance.cur_email = savedEmail;

        Debug.Log("log in insert : " + GameController.getAcessToken() + "\n" + GameController.Instance.cur_email);
        GameController.Instance.LoginInvoke(true);
    }

    private void OnPageFinished(UniWebView webView, int statusCode, string url)
    {
        Debug.Log("Finished loading: " + url);

        UniWebView.GetCookie(url, "accessToken", false, (cookie) =>
        {
            Debug.Log("accessToken : " + cookie);
            GameController.setAcessToken(cookie);
            if (!GameController.haveToken())
            {

                webView.Hide();
                Destroy(webView);

                //TODO : login state
                GameController.Instance.LoginInvoke(true);
            }

        });

        UniWebView.GetCookie(url, "email", false, (cookie) =>
        {
            Debug.Log("user email is : " + cookie);
        });
        // // 예: 로그인 후 리디렉션되는 페이지 주소 확인
        // string jsCode = @"
        //     var target = document.getElementById('login-status');
        //     if (target) {
        //         var observer = new MutationObserver(function(mutationsList, observer) {
        //             window.location.href = 'uniwebview://dom_changed';
        //         });
        //         observer.observe(target, { childList: true, subtree: true });
        //     }
        // ";
        // webView.EvaluateJavaScript(jsCode);
        //StartCoroutine(CheckCookies(url));
    }

    private IEnumerator CheckCookies(string url)
    {
        string urll = url;
        string token = "";

        while (string.IsNullOrEmpty(token))
        {
            //urll = webView.Url;
            webView.EvaluateJavaScript("document.cookie", (payload) =>
            {
                Debug.Log(payload.resultCode);
                Debug.Log(payload.data);
                Debug.Log(payload.extra);
            // if (payload.resultCode == 0)  // 0 = Success
                // {
                //     string cookieString = payload.data;
                //     Debug.Log("현재 쿠키: " + cookieString);
                // }
                // else
                // {
                //     Debug.LogError($"쿠키 가져오기 실패: {payload.extra}");
                // }
            });
            yield return new WaitForFixedUpdate();
        }

        // Debug.Log("url changed");

        // UniWebView.GetCookie(url, "accessToken", (cookie) => { Debug.Log("쿠키쿠키" + cookie); token = cookie; });
        Debug.Log("webview 종료");
        
    }

    private void HandleLoginSuccess(string token)
    {
        // 이 토큰으로 서버 API 호출 또는 인증 처리 가능
    }
}
