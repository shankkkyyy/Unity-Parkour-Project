
using UnityEngine;
using System.Collections.Generic;

public class FreeCam : MonoBehaviour {

    enum FreeCamState
    {
        FreeRun,
        Combat,
        Sliding,
    }
    #region Define Variables
    [SerializeField]
    bool m_verticalInverse;
    FreeCamState myCamState;

    Camera m_Cam;
    Combat m_combat;
    Transform m_pivot, m_viewPortCenterIdeal;
    LayerMask m_clippingMask;
    Hitman_BasicMove m_basicMove;


    [SerializeField] Transform[] m_viewPortDiagnolVertexs;
    [SerializeField] Transform[] m_viewPortMidPoints;
    [SerializeField] Transform m_target;
    [SerializeField, ReadOnly] float m_pivotHeight, m_currpivotHeight;
    [SerializeField, ReadOnly] float m_camDesireDistance;
    [SerializeField]    [Range(0, 2)]         float m_pivotLocalZ;
    [SerializeField]    [Range(180, 480)]     float m_orbitSpeedM;
    [SerializeField]    [Range(0, 180)]       float m_orbitSpeedK;
    [SerializeField]    [Range(180, 360)]     float m_tiltSpeed;
    [SerializeField]    [Range(0, 20)]        float m_camLaggingSpeed;
    [SerializeField]    [Range(-90, 90)]      float m_minTiltAngle, m_maxTiltAngle;
    [SerializeField]    [Range(0, 20)]        float m_camLaggingRot;

    Quaternion m_targetYawRotation, m_targetPitchRotation;
    [SerializeField] float pivotHeight_Combat = 2.3f, camDis_Combat = 6f, 
        camDis_FreeRun = 2.5f, pivotHeight_FreeRun = 1.8f,
        camDis_sliding = 1f, pivotHeight_sliding =.5f,
        camStateChaningSpd = 5, camClipForwardDis = 0.1f;
    const float pivotHeight_Crounch = 1.8f;
    float m_pitch, m_yaw;
    float m_camBias_hor, m_camBias_vert;
    float viewPortHalfHeight_world, viewPortHalfWidth_world;
    bool  toStand, toCrounch;
    #endregion

    void Start()
    {
        m_Cam = Camera.main;
        #region 1. Set Up ViewPort Frame
        // Get View port size
        viewPortHalfHeight_world =  Vector3.Distance(m_Cam.ViewportToWorldPoint(new Vector3(0, 0, m_Cam.nearClipPlane)),
        m_Cam.ViewportToWorldPoint(new Vector3(0, .5f, m_Cam.nearClipPlane)));
        viewPortHalfWidth_world = viewPortHalfHeight_world * m_Cam.aspect;

        m_pivot = m_Cam.transform.parent.transform;
        m_viewPortCenterIdeal = m_pivot.transform.GetChild(1);

        m_viewPortDiagnolVertexs[0].localPosition = new Vector3(-viewPortHalfWidth_world, -viewPortHalfHeight_world, 0);
        m_viewPortDiagnolVertexs[1].localPosition = new Vector3(-viewPortHalfWidth_world, viewPortHalfHeight_world, 0);
        m_viewPortDiagnolVertexs[2].localPosition = new Vector3(viewPortHalfWidth_world, viewPortHalfHeight_world, 0);
        m_viewPortDiagnolVertexs[3].localPosition = new Vector3(viewPortHalfWidth_world, -viewPortHalfHeight_world, 0);

        m_viewPortMidPoints[0].localPosition = new Vector3(0, -viewPortHalfHeight_world, 0);
        m_viewPortMidPoints[1].localPosition = new Vector3(-viewPortHalfWidth_world, 0, 0);
        m_viewPortMidPoints[2].localPosition = new Vector3(0, viewPortHalfHeight_world, 0);
        m_viewPortMidPoints[3].localPosition = new Vector3(viewPortHalfWidth_world, 0, 0);



        //1 ***** 2 ***** 2
        //*****************
        //1 ***** X ***** 3
        //*****************
        //0 ***** 0 ***** 3


        #endregion
        #region 2. Initailization for Cam movement  

        m_pivotLocalZ = 0f;
        m_orbitSpeedM = 180;//By mouse
        m_orbitSpeedK = 0;  //By key
        m_tiltSpeed = 200;
        m_minTiltAngle = -10;
        m_maxTiltAngle = 35;
        m_camLaggingSpeed = 5;
        m_camLaggingRot = 5;

        m_pivotHeight = pivotHeight_FreeRun;
        m_camDesireDistance = camDis_FreeRun;

        #endregion

        myCamState = FreeCamState.FreeRun;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        m_combat = player.GetComponent<Combat>();
        m_basicMove = player.GetComponent<Hitman_BasicMove>();
        m_clippingMask = 1 << LayerMask.NameToLayer("Default")| 1 << LayerMask.NameToLayer("parkour");
        // get viewPort Size in world space
        m_camBias_hor = 0;
        m_camBias_vert = 0;
    }
    void Update()
    {

        #region 1. Calculate pitch and yaw based on user input    
        // Get input from input manager
        m_pitch += (m_verticalInverse) ? (1) : (-1) * m_tiltSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
        m_yaw += (m_orbitSpeedM * Input.GetAxis("Mouse X") + m_orbitSpeedK * Input.GetAxis("Horizontal")) * Time.deltaTime;

        // lock the tilt angle
        m_pitch = Mathf.Clamp(m_pitch, m_minTiltAngle, m_maxTiltAngle);
        // pivot only rotate on X axis, 
        // the camera height can adapt to different charactor pose, etc crouch
        m_targetPitchRotation = Quaternion.Euler(m_pitch, transform.rotation.y, transform.rotation.z);
        // the root only rotate on y axis
        m_targetYawRotation = Quaternion.Euler(0, m_yaw, 0);
        #endregion

        #region 2. Build a proper cam spring arm for various movement

        if (m_combat.IsTargetting)
            myCamState = FreeCamState.Combat;
        else if (m_basicMove.AmSliding)
            myCamState = FreeCamState.Sliding;
        else
            myCamState = FreeCamState.FreeRun;

        switch (myCamState)
        {
            case FreeCamState.FreeRun:
                SetUpCamSpring(pivotHeight_FreeRun, camDis_FreeRun, 0.01f);
                break;
            case FreeCamState.Combat:
                SetUpCamSpring(pivotHeight_Combat, camDis_Combat, 0.01f);
                break;
            case FreeCamState.Sliding:
                SetUpCamSpring(pivotHeight_sliding, camDis_sliding, 0.01f);
                break;
        }
        #endregion

        #region draw desired viewPort
        Debug.DrawRay(m_pivot.position, -m_pivot.transform.forward * m_camDesireDistance, Color.red);

        //Debug.DrawLine(m_viewPortDiagnolVertexs[1].position, m_viewPortDiagnolVertexs[2].position, Color.red);
        //Debug.DrawLine(m_viewPortDiagnolVertexs[0].position, m_viewPortDiagnolVertexs[3].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[0].position, m_viewPortDiagnolVertexs[3].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[0].position, m_viewPortDiagnolVertexs[0].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[1].position, m_viewPortDiagnolVertexs[0].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[1].position, m_viewPortDiagnolVertexs[1].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[2].position, m_viewPortDiagnolVertexs[1].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[2].position, m_viewPortDiagnolVertexs[2].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[3].position, m_viewPortDiagnolVertexs[2].position, Color.red);
        Debug.DrawLine(m_viewPortMidPoints[3].position, m_viewPortDiagnolVertexs[3].position, Color.red);
        #endregion

    }
    void FixedUpdate()
    {
        CamPosition();
        m_pivot.localPosition = new Vector3(0, m_pivotHeight, m_pivotLocalZ);

        float m_camActualDistance = m_camDesireDistance;
        if (myCamState != FreeCamState.Sliding)
        {
            #region Prevent Camera Clipping

            RaycastHit hitInfo;
            RaycastHit hitInfo_comp;
            // prevent anything objects between viewport and target
            if (Physics.Raycast(m_pivot.position, -m_pivot.forward, out hitInfo, m_camDesireDistance, m_clippingMask))
                //m_camDesireDistance = hitInfo.distance;
                m_camActualDistance = hitInfo.distance - camClipForwardDis;
            // update desire viewPort location
            m_viewPortCenterIdeal.localPosition = new Vector3(0, 0, -(m_camActualDistance - m_Cam.nearClipPlane));
            // prevent cliping from left, right, up, down side

            // check left side
            Physics.Linecast(m_viewPortMidPoints[2].position, m_viewPortDiagnolVertexs[1].position, out hitInfo, m_clippingMask);
            Physics.Linecast(m_viewPortMidPoints[0].position, m_viewPortDiagnolVertexs[0].position, out hitInfo_comp, m_clippingMask);
            if (hitInfo.collider == null && hitInfo_comp.collider != null)
                m_camBias_hor = viewPortHalfWidth_world - hitInfo_comp.distance;
            else if (hitInfo_comp.collider == null && hitInfo.collider != null)
                m_camBias_hor = viewPortHalfWidth_world - hitInfo.distance;
            else if (hitInfo_comp.collider != null && hitInfo.collider != null)
                m_camBias_hor = viewPortHalfWidth_world - Mathf.Min(hitInfo_comp.distance, hitInfo.distance);
            else
            {
                // check right side
                Physics.Linecast(m_viewPortMidPoints[2].position, m_viewPortDiagnolVertexs[2].position, out hitInfo, m_clippingMask);
                Physics.Linecast(m_viewPortMidPoints[0].position, m_viewPortDiagnolVertexs[3].position, out hitInfo_comp, m_clippingMask);
                if (hitInfo.collider == null && hitInfo_comp.collider != null)
                    m_camBias_hor = -(viewPortHalfWidth_world - hitInfo_comp.distance);
                else if (hitInfo_comp.collider == null && hitInfo.collider != null)
                    m_camBias_hor = -(viewPortHalfWidth_world - hitInfo.distance);
                else if (hitInfo_comp.collider != null && hitInfo.collider != null)
                    m_camBias_hor = -(viewPortHalfWidth_world - Mathf.Min(hitInfo_comp.distance, hitInfo.distance));
                else
                    m_camBias_hor = 0;
            }

            // check up side

            if (Physics.Linecast(m_viewPortCenterIdeal.position, m_viewPortMidPoints[2].position, out hitInfo, m_clippingMask))
                m_camBias_vert = -(viewPortHalfHeight_world - hitInfo.distance);
            else if (Physics.Linecast(m_viewPortCenterIdeal.position, m_viewPortMidPoints[0].position, out hitInfo, m_clippingMask))
                m_camBias_vert = viewPortHalfHeight_world - hitInfo.distance;
            else m_camBias_vert = 0;
            #endregion
        }

        //m_Cam.transform.localPosition = new Vector3(m_camBias_hor, m_camBias_vert, -m_camDesireDistance);
        m_Cam.transform.localPosition = new Vector3(m_camBias_hor, m_camBias_vert, -m_camActualDistance);

    }
    void CamPosition()
    {
        if (m_camLaggingSpeed != 0)
        {
            transform.position = Vector3.Lerp(transform.position, m_target.position, m_camLaggingSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = m_target.position;
        }
        if (m_camLaggingRot != 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_targetYawRotation, m_camLaggingRot * Time.deltaTime);
            m_pivot.localRotation = Quaternion.Slerp(m_pivot.localRotation, m_targetPitchRotation, m_camLaggingRot * Time.deltaTime);
        }
        else
        {
            transform.rotation = m_targetYawRotation;
            m_pivot.localRotation = m_targetPitchRotation;
        }
    }

    void SetUpCamSpring(float _pivotHeight, float _camDis, float _threshold)
    {
        if (Mathf.Abs(m_pivotHeight - _pivotHeight) > _threshold)
            m_pivotHeight = Mathf.Lerp(m_pivotHeight, _pivotHeight, camStateChaningSpd * Time.deltaTime);
        if (Mathf.Abs(m_camDesireDistance - _camDis) > _threshold)
            m_camDesireDistance = Mathf.Lerp(m_camDesireDistance, _camDis, camStateChaningSpd * Time.deltaTime);
    }


    /// <summary>
    /// un normalized world space moving direction
    /// </summary>
    public void ZoomInToFreeRun()
    {
        myCamState = FreeCamState.FreeRun;
    }
    public void ZoomOutToCombat()
    {
        myCamState = FreeCamState.Combat;
    }

}
