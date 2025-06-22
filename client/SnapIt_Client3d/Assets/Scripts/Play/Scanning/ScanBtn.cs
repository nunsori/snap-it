using UnityEngine;
using UnityEngine.UI;

public class ScanBtn : MonoBehaviour
{
    private Button scanBtn;

    void Awake()
    {
        scanBtn = gameObject.GetComponent<Button>();
    }

    void OnDisable()
    {
        UIController.OnScaninteract -= SetInteract;
    }

    void OnEnable()
    {
        UIController.OnScaninteract += SetInteract;
    }
    public void Init()
    {

        
        scanBtn.interactable = false;

    }

    public void SetInteract(bool interact){
        scanBtn.interactable = interact;
    }

}
