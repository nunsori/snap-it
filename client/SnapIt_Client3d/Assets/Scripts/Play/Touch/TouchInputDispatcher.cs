using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInputDispatcher : MonoBehaviour
{
    public static event Action<Vector2> OnTouchBegan;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject()){
                OnTouchBegan?.Invoke(Input.mousePosition);                
            }

        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!EventSystem.current.IsPointerOverGameObject()){
                OnTouchBegan?.Invoke(Input.GetTouch(0).position);
            }
            
        }
#endif
    }
}