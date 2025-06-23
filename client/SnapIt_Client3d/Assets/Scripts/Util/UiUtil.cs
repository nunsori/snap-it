using System;

using UnityEngine;
//using UnityEngine.Rendering;
using UnityEngine.UI;
//using System.Linq;

public static class UiUtil
{
    public static void AddButtonClickEvent(Button button, Action onClickAction)
    {
        if (button == null)
        {
            Debug.LogWarning("Button is null. 이벤트를 추가할 수 없습니다.");
            return;
        }

        if (onClickAction == null)
        {
            Debug.LogWarning("onClickAction is null. 버튼 이벤트를 연결할 수 없습니다.");
            return;
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() => onClickAction());
    }
}
