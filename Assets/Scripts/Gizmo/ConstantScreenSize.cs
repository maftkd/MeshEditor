using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantScreenSize : MonoBehaviour
{
    public float widthInPercentage;

    private Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = 0;
        Plane plane = new Plane(mainCam.transform.forward, transform.position);
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        if (plane.Raycast(ray, out float distance))
        {
            dist = distance;
        }
        else
        {
            return;
        }
        float width = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad) * dist;
        float widthInWorld = width * widthInPercentage / 100;
        transform.localScale = widthInWorld * Vector3.one;

    }
}
