using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;
public class StateTester : MonoBehaviour
{
    [SerializeField]
    private GameObject CameraObj;

    [SerializeField]
    private TextMeshProUGUI InfoText;


    //public LineRenderer lineRenderer;
    public float maxRayLength = 10f;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public ARRaycastManager raycastManager;
    
    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private LineRenderer lineRenderer;

    //scan을 할 수 있는지 확인
    public static bool checkState = true;
    public void Init(){
        arCamera = CameraObj.GetComponent<Camera>();
    }

    public void CheckMeshDistance(){
        // 1. 화면 중앙에서 Raycast 실행
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = arCamera.ScreenPointToRay(screenCenter);


        float distance = 0f;
        float meshDistance = 0f;


        if (raycastManager.Raycast(screenCenter, hits, TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon))
        {   
            Pose hitPose = hits[0].pose;
            distance = Vector3.Distance(arCamera.transform.position, hitPose.position);
            //Debug.Log("추정된 물체 또는 바닥까지 거리: " + distance.ToString("F2") + "m");
        }

        RaycastHit physicsHit;
        if (Physics.Raycast(ray, out physicsHit, 10f))  // 길이 제한은 적절히
        {
            meshDistance = Vector3.Distance(arCamera.transform.position, physicsHit.point);
            //Debug.Log("ARMesh와의 거리: " + meshDistance.ToString("F2") + "m");
            if(lineRenderer != null){
                // lineRenderer.SetPosition(0,arCamera.transform.position);
                // lineRenderer.SetPosition(1, physicsHit.point);
                // lineRenderer.enabled = true;
            }
        }


        InfoText.text = "Info\nCPos : " + CameraObj.transform.position + "\nCRot :" + CameraObj.transform.rotation + "\ndistance: " + distance.ToString("F2") + "m" + "\nmesh distance: " + meshDistance.ToString("F2") + "m";

        if(checkState){
            UIController.ScanInvoke(!(meshDistance <= 0f));
        }
    }
}
