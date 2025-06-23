using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;

public class ARController : MonoBehaviour
{
    [SerializeField]
    private Camera cameraObj;

    [SerializeField]
    private GoogleApiController GApiController;

    
    [SerializeField]
    private StateTester tester;

    [SerializeField]
    private ObjectDetector detector;
    
    [SerializeField]
    private SelectObj selector;
    


    public void Init()
    {
        GApiController.CameraObj = cameraObj;

        tester.Init();
        detector.Init();
        selector.Init(cameraObj);

        
    }

    // Update is called once per frame
    void Update()
    {
        if(tester != null){
            tester.CheckMeshDistance();
        }
    }

    public void ScanFun(){
        if(!StateTester.checkState) return;

        SoundController.Instance.Play_Effect("snap", SoundController.Instance.Effect_Volume, false);

        detector.StartCoroutine(detector.MeshRayCast());
    }

    
    

}
