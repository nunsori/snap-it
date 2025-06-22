using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using System.Linq;
using System;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField]
    private ARMeshManager meshManager;

    [SerializeField]
    private GoogleApiController GApiController;

    [SerializeField]
    private GameObject sampleObj;
    private List<GameObject> objectArr;

    [SerializeField]
    private Camera arCamera;

    //public LineRenderer lineRenderer;
    public float maxRayLength = 10f;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();



    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private float savedFOV, savedAspect, savedNear;
    private Vector2 savedScreenSize;

    [SerializeField]
    private float scaleFactor = 2f; 


    public void Init()
    {
        //meshManager.enabled = false;
        if (objectArr != null)
        {
            objectArr.ForEach((element) => { Destroy(element); });
            objectArr.Clear();
        }
        else
        {
            objectArr = new List<GameObject>();
        }
    }


    public IEnumerator MeshRayCast()
    {
        //화면 Lock 도 걸면 좋을듯?

        //mesh scan 활성화
        meshManager.enabled = true;
        StateTester.checkState = false;

        //scan btn 비활성화
        UIController.ScanInvoke(false);


        float meshDistance = -1f;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 1. 화면 중앙에서 Raycast 실행
        Ray ray = arCamera.ScreenPointToRay(screenCenter);
        RaycastHit physicsHit;

        while (meshDistance == -1f)
        {
            if (Physics.Raycast(ray, out physicsHit, 10f))  // 길이 제한은 적절히
            {
                meshDistance = Vector3.Distance(arCamera.transform.position, physicsHit.point);
                Debug.Log("ARMesh와의 거리: " + meshDistance.ToString("F2") + "m");
            }

            yield return new WaitForSeconds(0.1f);
        }

        savedCameraPosition = arCamera.transform.position;
        savedCameraRotation = arCamera.transform.rotation;
        savedFOV = arCamera.fieldOfView;
        savedAspect = arCamera.aspect;
        savedNear = arCamera.nearClipPlane;
        savedScreenSize = new Vector2(Screen.width, Screen.height);

        //scan 진행후 api요청 보내기
        yield return StartCoroutine(GApiController.send_request());


        //api답변온거 확인
        //raycast 해당 사각형 위치들에서 실행하기
        InstantiateObj();

        //이후 해당 거리만큼 위치에 사각형 오브젝트생성 -> 오브젝트는 생성되고나서 계속 카메라(플레이어)를 바라봐야함
        StateTester.checkState = true;
    }

    private void InstantiateObj()
    {
        Debug.Log("result count = " + GApiController.resarr.Count);
        if (GApiController.resarr != null && GApiController.resarr.Count != 0)
        {
            for (int i = 0; i < GApiController.resarr.Count; i++)
            {

                //사각형 포인트 정보 얻기
                // Vector2 point = Vector2.zero;
                // point.x = CheckCenter(i).x * Screen.width;
                // point.y = (1f - CheckCenter(i).y) * Screen.height;

                Vector2 point = new Vector2(CheckCenter(i).x * savedScreenSize.x,
                            (1f - CheckCenter(i).y) * savedScreenSize.y);


                Ray ray1 = GenerateRayFromScreenPoint(point);//arCamera.ScreenPointToRay(point);
                Debug.Log("Point is  : " + point.x + " , " + point.y + "byobj : " + GApiController.resarr[i].name);

                //ray cast 진행으로 거리 얻기
                RaycastHit pointHis;
                float distances = -1f;

                

                if (Physics.Raycast(ray1, out pointHis, 10f))
                {
                    distances = Vector3.Distance(arCamera.transform.position, pointHis.point);
                    Debug.Log(GApiController.resarr[i].name + " 거리 : " + distances);


                    (float widthRatio, float heightRatio) = CheckRectSize(i); // normalized 화면 비율
                    float distance = Vector3.Distance(arCamera.transform.position, pointHis.point);

                    // 1. 거리 기준 화면의 실제 물리적 크기 계산
                    float screenHeightAtDist = 2f * Mathf.Tan(0.5f * arCamera.fieldOfView * Mathf.Deg2Rad) * distance;
                    float screenWidthAtDist = screenHeightAtDist * arCamera.aspect;

                    // 2. 해당 rect가 차지하는 실제 공간 크기 (월드 단위)
                    float realWidth = widthRatio * screenWidthAtDist * scaleFactor;
                    float realHeight = heightRatio * screenHeightAtDist * scaleFactor;

                    GameObject tempObj = Instantiate(sampleObj, pointHis.point, Quaternion.identity, gameObject.transform);

                    tempObj.transform.localScale = new Vector3(realWidth, realHeight, tempObj.transform.localScale.z);


                    objectArr.Add(tempObj);
                    tempObj.GetComponent<LookAtCamera>().Init(GApiController.resarr[i].name, arCamera, CheckRectSize(i));
                }

            }
        }
    }


    private (float x, float y) CheckCenter(int idx)
    {
        return (((GApiController.resarr[idx].boundingPoly.normalizedVertices.Max(v => v.x) + GApiController.resarr[idx].boundingPoly.normalizedVertices.Min(v => v.x)) / 2),
        (GApiController.resarr[idx].boundingPoly.normalizedVertices.Max(v => v.y) + GApiController.resarr[idx].boundingPoly.normalizedVertices.Min(v => v.y)) / 2);
    }

    private (float x, float y) CheckRectSize(int idx)
    {
        return (Math.Abs(GApiController.resarr[idx].boundingPoly.normalizedVertices.Max(v => v.x) - GApiController.resarr[idx].boundingPoly.normalizedVertices.Min(v => v.x)),
        Math.Abs(GApiController.resarr[idx].boundingPoly.normalizedVertices.Max(v => v.y) - GApiController.resarr[idx].boundingPoly.normalizedVertices.Min(v => v.y)));
    }
    
    private Ray GenerateRayFromScreenPoint(Vector2 screenPoint)
    {
        // 1. NDC 좌표계로 변환 ([-1,1] 범위)
        float x = (screenPoint.x / savedScreenSize.x - 0.5f) * 2f;
        float y = (screenPoint.y / savedScreenSize.y - 0.5f) * 2f;

        // 2. View space 방향 계산
        float tanFov = Mathf.Tan(savedFOV * 0.5f * Mathf.Deg2Rad);
        Vector3 dir = new Vector3(
            x * savedAspect * tanFov,
            y * tanFov,
            1f // camera looks down +z in view space
        );

        dir.Normalize();

        // 3. 카메라 좌표계로 변환
        Vector3 worldDir = savedCameraRotation * dir;
        return new Ray(savedCameraPosition, worldDir);
    }
}
