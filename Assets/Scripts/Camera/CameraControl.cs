﻿using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float dampTime = 0.4f;                 
    public float screenEdgeBuffer = 4f;           
    public float minSize = 6.5f;
    [HideInInspector] public Transform[] targets; 


    private Camera camera;                        
    private float zoomSpeed;                      
    private Vector3 moveVelocity;                 
    private Vector3 desiredPosition;

    private Transform lastHit;
    private Color lastColor;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    // fixed update?
    private void LateUpdate()
    {
        //FadeObjectBetweenCameraAndPlayer();
        Move();
        Zoom();
    }

    
    void FadeObjectBetweenCameraAndPlayer()
    {
        Transform cameraObject = null;
        RaycastHit hit;
        Debug.DrawLine(targets[0].position, camera.transform.position - 100000 * camera.transform.forward, Color.red, 0.1f);
        
        if (Physics.Linecast(targets[0].position, camera.transform.position - 100000 * camera.transform.forward, out hit))
        {
            if(lastHit != null)
            {
                //lastHit.transform.GetComponent<MeshRenderer>().enabled = true;
                lastHit.transform.GetComponent<MeshRenderer>().material.color = lastColor;
                lastHit = null;
            }
            //hit.transform.GetComponent<MeshRenderer>().enabled = false;
            Color colorB = hit.transform.GetComponent<MeshRenderer>().material.color;
            lastColor = colorB;
            colorB.a = 0.4f;
            hit.transform.GetComponent<MeshRenderer>().material.color = colorB;
            lastHit = hit.transform;
        } else
        {
            if (lastHit != null)
            {
                //lastHit.transform.GetComponent<MeshRenderer>().enabled = true;
                lastHit.transform.GetComponent<MeshRenderer>().material.color = lastColor;
                lastHit = null;
            }
        }
    }
    
    private void Move()
    {
        FindAveragePosition();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampTime);
    }

    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        for (int i = 0; i < targets.Length; i++)
        {
            if (!targets[i].gameObject.activeSelf)
                continue;

            averagePos += targets[i].position;
            numTargets++;
        }

        if (numTargets > 0)
            averagePos /= numTargets;

        averagePos.y = transform.position.y;

        desiredPosition = averagePos;
    }


    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, requiredSize, ref zoomSpeed, dampTime);
    }


    private float FindRequiredSize()
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(desiredPosition);

        float size = 0f;

        for (int i = 0; i < targets.Length; i++)
        {
            if (!targets[i].gameObject.activeSelf)
                continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].position);

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));

            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / camera.aspect);
        }
        
        size += screenEdgeBuffer;

        size = Mathf.Max(size, minSize);

        return size;
    }

    


    public void SetStartPositionAndSize()
    {
        FindAveragePosition();

        transform.position = desiredPosition;

        camera.orthographicSize = FindRequiredSize();
    }
}