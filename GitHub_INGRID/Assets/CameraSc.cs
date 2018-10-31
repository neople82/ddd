using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class CameraSc : MonoBehaviour {

    public float m_MoveSpeed = 12220f;
    public float m_RotateSpeed = 2f;
    public float ZoomSpeed, MinZoom, MaxZoom = 0;
    //--------------------
    float Camzoom, SaveZoomfloat = 0.0f;
    private bool doubleclick, doubleclickcancel, CamMode = false;
    
    private bool CamRot, RayOn = false;
    private float x, y;
    private Vector3 rotateValue;
    private GameObject targetObject;

    void Awake()
    {
        doubleclick = false;
        doubleclickcancel = false;
        SaveZoomfloat = 3.0f;
        CamRot = false;
        RayOn = false;
    }

    void Start()
    {
        this.targetObject = new GameObject();
        set_target();
    }

    
    void Update() {

        //카메라가 오소그래픽 or 프로스펙티브를 구분
        switch (CamMode)
        {
            case true://오소그래픽일 경우
                //오소그래픽의 사이즈를 Camzoom 변수에 대입
                Camzoom = gameObject.GetComponent<Camera>().orthographicSize;
                break;

            case false://프로스펙티브일 경우
                //생성된 타겟 오브젝트와 카메라 사이의 값을 계산하여 Camzoom 변수에 대입
                float dist = Vector3.Distance(targetObject.transform.position, this.transform.position);
                Camzoom = dist;
                break;
        }



        /// <summary>
        /// -----------------------------------터치 디스플레이---------------------------------------
        /// </summary>
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //======이동=====
            for (int i = 0; i < Input.touchCount; i++)//터치의 카운트를 센다
            {
                //UI에서는 작동하지 않게 조건문
                if (!EventSystem.current.IsPointerOverGameObject(i))
                {
                    Touch touch = Input.GetTouch(i);//세었던 터치 카운트를 변수에 대입
                    Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;//터치의 처음 위치값을 대입

                    //1터치로 이동을 했을 경우
                    if ((Input.touchCount < 2 && Input.touchCount > 0) && touch.phase == TouchPhase.Moved)
                    {
                        gameObject.transform.Translate(-touchDeltaPosition.x * (m_MoveSpeed * (Camzoom * 0.1f)) * Time.deltaTime, 
                            -touchDeltaPosition.y * (m_MoveSpeed * (Camzoom * 0.1f)) * Time.deltaTime, 0);
                    }
                }
            }

            //=====회전=====
            for (int i = 0; i < Input.touchCount; i++)//터치의 카운트를 센다.
            {
                //UI에서는 작동하지 않게 조건문
                if (!EventSystem.current.IsPointerOverGameObject(i))
                {
                    Touch touch = Input.GetTouch(i);//세었던 터치 카운트를 변수에 대입
                    Vector2 touchDeltaPosition = touch.deltaPosition;//터치의 처음 위치값을 대입

                    //3터치로 이동했을 경우
                    if ((Input.touchCount < 4 && Input.touchCount > 2) && touch.phase == TouchPhase.Moved)
                    {

                        if (!CamRot)
                        {
                            this.set_target();
                            CamRot = true;
                        }

                        switch (RayOn)
                        {
                            case true://레이캐스트에 타겟팅이 되었을 경우 실행하는 코드
                                      //타겟팅된 지점을 중심으로 카메라가 회전함
                                gameObject.transform.LookAt(targetObject.transform);
                                this.x = Input.GetAxis("Mouse X") * m_RotateSpeed;
                                this.y = Input.GetAxis("Mouse Y") * m_RotateSpeed;
                                gameObject.transform.RotateAround(targetObject.transform.position, transform.up, (float)this.x);
                                gameObject.transform.RotateAround(targetObject.transform.position, transform.right, (float)this.y * -1f);
                                break;

                            case false://레이캐스트에 타겟팅이 되지 않았을 경우 실행하는 코드
                                       //카메라 자체가 회전함
                                y = Input.GetAxis("Mouse X") * m_RotateSpeed;
                                x = Input.GetAxis("Mouse Y") * m_RotateSpeed;
                                rotateValue = new Vector3(x, y * -1, 0);
                                transform.eulerAngles = transform.eulerAngles - rotateValue;
                                break;
                        }
                    }
                }
            }
            
            //=====줌=====
            if (Input.touchCount == 2)//터치 카운트가 정확히 2개일 경우
            {
                //UI에서는 작동하지 않게 조건문
                if (!EventSystem.current.IsPointerOverGameObject(2))
                {
                    Touch touchZero = Input.GetTouch(0);//처음 터치한 부분을 변수 대입
                    Touch touchOne = Input.GetTouch(1);//두번째 터치한 부분을 변수 대입

                    //---터치의 첫 지점과 끝 지점 사이의 값을 계산하는 함수---
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                    //-----------------------------------------------------------

                    switch (CamMode)
                    {
                        case true://오소그래픽일 경우
                            //Min값 이상이거나 Max값 이하 값일 경우(ex: 10~60, Now value: 30)
                            if (gameObject.GetComponent<Camera>().orthographicSize >= MinZoom || gameObject.GetComponent<Camera>().orthographicSize <= MaxZoom)
                            {
                                gameObject.GetComponent<Camera>().orthographicSize += deltaMagnitudeDiff * (Camzoom * 0.1f);
                                gameObject.GetComponent<Camera>().orthographicSize = Mathf.Max(gameObject.GetComponent<Camera>().orthographicSize, 0.03f);
                                doubleclick = false;
                            }
                            //그 반대로 Min값 이하이거나 Max값 이상 값일 경우(ex: 10~60, Now value: 5 or 60)
                            if (gameObject.GetComponent<Camera>().orthographicSize <= MinZoom || gameObject.GetComponent<Camera>().orthographicSize >= MaxZoom)
                            {
                                gameObject.GetComponent<Camera>().orthographicSize -= deltaMagnitudeDiff * (Camzoom * 0.1f);
                                gameObject.GetComponent<Camera>().orthographicSize = Mathf.Max(gameObject.GetComponent<Camera>().orthographicSize, 0.03f);
                                doubleclick = false;
                            }
                            break;

                        case false://프로스펙티브일 경우
                            set_target();//타겟팅과의 거리 값 계산
                            //Min값 이상이거나 Max값 이하 값일 경우(ex: 10~60, Now value: 30)
                            if (Camzoom >= MinZoom || Camzoom <= MaxZoom)
                            {
                                gameObject.transform.Translate(Vector3.forward * ZoomSpeed * Time.deltaTime);
                                doubleclick = false;
                            }
                            //그 반대로 Min값 이하이거나 Max값 이상 값일 경우(ex: 10~60, Now value: 5 or 60)
                            if (Camzoom <= MinZoom || Camzoom >= MaxZoom)
                            {
                                gameObject.transform.Translate(-Vector3.forward * ZoomSpeed * Time.deltaTime);
                                doubleclick = false;
                            }
                            break;
                    }
                    
                }
            }
            
            //=====더블 터치=====
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (!EventSystem.current.IsPointerOverGameObject(i) && !doubleclick)
                {
                    Touch touch = Input.GetTouch(i);

                    if ((Input.touchCount > 0 && Input.touchCount < 2) && touch.phase == TouchPhase.Ended && !doubleclick)
                    {
                        doubleclick = true;
                        //StartCoroutine(doubletouchDelay());
                    }
                    else if ((Input.touchCount > 0 && Input.touchCount < 2) && touch.phase == TouchPhase.Ended && doubleclick)
                    {
                        doubleclick = false;
                        if (gameObject.GetComponent<Camera>().orthographicSize > 3f)
                        {
                            Vector3 touchPos;
                            touchPos = gameObject.GetComponent<Camera>().ScreenToWorldPoint(touch.position);
                            gameObject.transform.position = touchPos;
                            doubleclick = true;
                            SaveZoomfloat = gameObject.GetComponent<Camera>().orthographicSize;
                        }
                        if (gameObject.GetComponent<Camera>().orthographicSize <= 3f)
                            doubleclickcancel = true;
                    }
                }
            }
        }
        

        /// <summary>
        /// -----------------------------------PC 마우스---------------------------------------
        /// </summary>
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //=====이동=====
            if (Input.GetMouseButton(0))
            {
                gameObject.transform.Translate(-Vector3.right * Input.GetAxis("Mouse X") * (m_MoveSpeed * (Camzoom * 0.1f)) * Time.deltaTime);
                gameObject.transform.Translate(-Vector3.up * Input.GetAxis("Mouse Y") * (m_MoveSpeed * (Camzoom * 0.1f)) * Time.deltaTime);
            }
            //=====회전=====
            if (Input.GetMouseButton(1))
            {
                /**
                Vector3 eulerAngles = gameObject.transform.eulerAngles;
                eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * m_RotateSpeed;
                eulerAngles.y += Input.GetAxis("Mouse X") * 359f * m_RotateSpeed;
                Vector3 eulerX = new Vector3(Input.GetAxis("Mouse X") * Camzoom, 0, 0);
                gameObject.transform.eulerAngles = eulerAngles;**/
                
                //레이캐스트를 쏘는 함수를 호출, 불린값 때문에 한번만 실행함
                if (!CamRot)
                {
                    this.set_target();
                    CamRot = true;
                }

                switch (RayOn)
                {
                    case true://레이캐스트에 타겟팅이 되었을 경우 실행하는 코드
                        //타겟팅된 지점을 중심으로 카메라가 회전함
                        gameObject.transform.LookAt(targetObject.transform);

                        gameObject.transform.parent = targetObject.transform;//생성된 타겟 오브젝트에 카메라를 자식으로 둠

                        this.x = Input.GetAxis("Mouse X") * m_RotateSpeed;
                        this.y = Input.GetAxis("Mouse Y") * m_RotateSpeed;
                        //gameObject.transform.RotateAround(targetObject.transform.position, transform.up, (float)this.x);
                        //gameObject.transform.RotateAround(targetObject.transform.position, transform.right, (float)this.y * -1f);

                        //그리고 타겟된 오브젝트 자체를 돌리면 카메라가 부드럽게 보이겠지?
                        targetObject.transform.Rotate(transform.up * (float)this.x);
                        targetObject.transform.Rotate(transform.right * (float)this.y * -1f);

                        //카메라 회전시에 타겟오브젝트로 회전하기 때문에 그 상태로 계속 돌리다보면 어색하게 회전이 됨. 이것을 방지하기 위해서
                        //카메라 회전,이동을 멈추면 바로 카메라를 자식에서 빼내고 타겟 오브젝트 회전을 초기화 함.
                        if(x <= 0.4f && y <= 0.4f)
                        {
                            gameObject.transform.parent = null;
                            targetObject.transform.rotation = new Quaternion(0, 0, 0, 1);
                        }

                        break;

                    case false://레이캐스트에 타겟팅이 되지 않았을 경우 실행하는 코드
                        //카메라 자체가 회전함
                        y = Input.GetAxis("Mouse X") * m_RotateSpeed;
                        x = Input.GetAxis("Mouse Y") * m_RotateSpeed;
                        rotateValue = new Vector3(x, y * -1, 0);
                        transform.eulerAngles = transform.eulerAngles - rotateValue;
                        break;
                }
                
            }

            //우클릭을 떼었을 경우 레이캐스트 한번만 실행하는 불린을 초기화시킴.
            else if (Input.GetMouseButtonUp(1))
            {
                CamRot = false;
                gameObject.transform.parent = null;//자식으로 넣어둔 카메라를 다시 빼 냄
            }

            //=====확대,축소=====
            switch (CamMode)
            {
                case true://오소그래픽 모드

                    if (Input.GetAxis("Mouse ScrollWheel") < 0)//축소
                    {
                        //set_target();
                        //오소그래픽 사이즈로 줌을 조절
                        if (gameObject.GetComponent<Camera>().orthographicSize < MaxZoom)
                            gameObject.GetComponent<Camera>().orthographicSize += ZoomSpeed * 0.1f;
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)//확대
                    {
                        //set_target();
                        //오소그래픽 사이즈로 줌을 조절
                        if (gameObject.GetComponent<Camera>().orthographicSize > MinZoom)
                            gameObject.GetComponent<Camera>().orthographicSize -= ZoomSpeed * 0.1f;
                    }
                    break;

                case false://프로스펙티브 모드

                    if (Input.GetAxis("Mouse ScrollWheel") < 0)//축소
                    {
                        set_target();//<--이 함수 내에는 레이캐스트가 존재해 타겟 오브젝트와 카메라 사이의 값을 계산 할 수 있어서 최소, 최대 값까지 이동시키게 할 수 있음.
                        if (Camzoom < MaxZoom)
                            gameObject.transform.Translate(-Vector3.forward * ZoomSpeed * Time.deltaTime);
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)//확대
                    {
                        set_target();//<--이 함수 내에는 레이캐스트가 존재해 타겟 오브젝트와 카메라 사이의 값을 계산 할 수 있어서 최소, 최대 값까지 이동시키게 할 수 있음.
                        if (Camzoom > MinZoom && RayOn)
                            gameObject.transform.Translate(Vector3.forward * ZoomSpeed * Time.deltaTime);
                    }
                    break;
            }
        }



        //----더블 클릭 후 줌 인 코드-------------------
        if (doubleclick && RayOn)
        {
            switch (CamMode)
            {
                case true://오소그래픽일 경우
                    if (gameObject.GetComponent<Camera>().orthographicSize > MinZoom)
                        gameObject.GetComponent<Camera>().orthographicSize -= 1.0f;

                    else if (gameObject.GetComponent<Camera>().orthographicSize <= MinZoom)
                        doubleclick = false;
                    break;

                case false://프로스펙티브일 경우
                    if (Camzoom > MinZoom)
                        gameObject.transform.Translate(Vector3.forward * (ZoomSpeed * 2) * Time.deltaTime);
                    
                    else if (Camzoom <= MinZoom)
                        doubleclick = false;
                    break;
            }
            
        }
        //------더블 클릭 후 줌 아웃 코드---------------
        else if (doubleclickcancel)
        {
            switch (CamMode)
            {
                case true://오소그래픽일 경우
                    if (gameObject.GetComponent<Camera>().orthographicSize < SaveZoomfloat)
                        gameObject.GetComponent<Camera>().orthographicSize += (ZoomSpeed * 0.2f);

                    else if (gameObject.GetComponent<Camera>().orthographicSize >= SaveZoomfloat)
                        doubleclickcancel = false;
                    break;

                case false://프로스펙티브일 경우
                    if (gameObject.transform.localPosition.z > SaveZoomfloat)
                        gameObject.transform.Translate(-Vector3.forward * (ZoomSpeed * 2) * Time.deltaTime);

                    else if (gameObject.transform.localPosition.z <= SaveZoomfloat)
                        doubleclickcancel = false;
                    break;
            }
            
        }
    }

    


    //---------------더블 클릭 이벤트-----------------------------
    void OnGUI()
    {
        if(Input.touchCount == 0)
        {
            if(!EventSystem.current.IsPointerOverGameObject() && Event.current.clickCount == 2)
            {
                set_target();

                //----더블 클릭 전----
                if (!doubleclick && (Camzoom > MinZoom || gameObject.GetComponent<Camera>().orthographicSize > MinZoom))
                {
                    switch (CamMode)
                    {
                        case true://오소그래픽일 경우
                            if (gameObject.GetComponent<Camera>().orthographicSize > MinZoom)
                            {
                                doubleclick = true;
                                SaveZoomfloat = gameObject.GetComponent<Camera>().orthographicSize;
                            }
                            break;

                        case false://프로스펙티브일 경우
                            if (Camzoom > MinZoom)
                            {
                                doubleclick = true;
                                SaveZoomfloat = gameObject.transform.localPosition.z;
                            }
                            break;
                    }

                }
                
                //----더블 클릭 후----
                else if (!doubleclickcancel && (Camzoom <= MinZoom || gameObject.GetComponent<Camera>().orthographicSize <= MinZoom))
                {
                    
                    switch (CamMode)
                    {
                        case true:
                            if (gameObject.GetComponent<Camera>().orthographicSize <= MinZoom)
                                doubleclickcancel = true;
                            break;

                        case false:
                            set_target();
                            if (Camzoom <= MinZoom)
                                doubleclickcancel = true;
                            break;
                    }
                }

            }
        }
    }
    
    //회전할 때 호출하는 함수, 한 번만 실행함, 레이캐스트의 경우 한 번 실행하는 것이 최적화에 좋음.
    private void set_target()
    {
        Ray ray = gameObject.GetComponent<Camera>().ScreenPointToRay(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)));
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit))
        {
            RayOn = true;
            targetObject.transform.rotation = new Quaternion(0, 0, 0, 1);
            targetObject.transform.position = raycastHit.point;
        }
        else
        {
            RayOn = false;
        }
    }


    //카메라모드 변환하는 코드, 버튼에 적용하면 됨
    public void CamModeChange_Btn()
    {
        switch (CamMode)
        {
            case true://프로스펙티브로 전환
                CamMode = false;
                break;

            case false://오소그래픽으로 전환
                CamMode = true;
                break;
        }
    }
}