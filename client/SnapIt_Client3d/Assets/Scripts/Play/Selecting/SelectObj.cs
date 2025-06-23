using UnityEngine;

public class SelectObj : MonoBehaviour
{
    public string TagName;

    private bool interactable = false;

    private Camera cameraObj;


    private void OnDisable()
    {
        
        TouchInputDispatcher.OnTouchBegan -= HandleTouch;
    }

    void OnEnable()
    {
        TouchInputDispatcher.OnTouchBegan += HandleTouch;
    }

    public void Init(Camera cam)
    {
        cameraObj = cam;

        SetInteract(true);
        
    }
    

    public void SetInteract(bool interact){
        interactable = interact;
    }

    void HandleTouch(Vector2 screenPosition){
        if(!interactable) return;

        Debug.Log("이 오브젝트가 터치됨! 위치: " + screenPosition);

        Ray ray = cameraObj.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            Debug.Log("터치한 오브젝트: " + hit.collider.tag);
            if (hit.collider.CompareTag(TagName))
            {
                LookAtCamera hittemp = hit.collider.GetComponent<LookAtCamera>();
                if (hittemp != null)
                {
                    // WordList.Instance.AddWord(hittemp.havname);
                    // Destroy(hittemp.gameObject);
                    // break;
                    string[] words = hittemp.havname.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (string word in words)
                    {
                        WordList.Instance.AddWord(word);
                    }

                    Destroy(hittemp.gameObject);
                    break;
                }
            }
        }
    }
}
