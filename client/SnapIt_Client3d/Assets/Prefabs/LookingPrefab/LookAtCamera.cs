using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Drawing;
using UnityEngine.SocialPlatforms;
using System;


public class LookAtCamera : MonoBehaviour
{

    [SerializeField]
    private TextMeshPro TMP;

    
    public Camera targetCamera;

    private Vector2 size = Vector2.zero;
    private Vector3 initialScale;

    public string havname = "";


    public void Init(string text, Camera cam, (float x, float y) sizes){
        targetCamera = cam;
        TMP.text = text;
        size.x = sizes.x ;
        size.y = sizes.y ;

        havname = text;
    }

    public void setSize(){
        gameObject.transform.localScale = new Vector2((float)Math.Pow(1.5f,1f+size.x)* gameObject.transform.localScale.x, (1f + size.y) * gameObject.transform.localScale.y);
    }
    void Start()
    {
        //setSize();
    }
    void Update()
    {
        // Vector3 direction = targetCamera.transform.position - transform.position;
        // direction.x = (x) ? 0 : direction.x;
        // direction.y = (y) ? 0 : direction.y;
        // direction.z = (z) ? 0 : direction.z;
        // transform.rotation = Quaternion.LookRotation(direction);
        
        gameObject.transform.LookAt(targetCamera.transform);

        
        
    }

    
}
