
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

namespace MagicCube
{
    public enum CubeRotateStatus
    {
        ニュートラル,
        回転中,
    }
    public struct DruggingEachCubeAxisData
    {
        public Vector3 worldPosition { get; set; }
        public Vector3 worldVector { get; set; }
        public Vector2 screenVector { get; set; }
        public Axis axis { get; set; }
        
        public DruggingEachCubeAxisData( Vector3 worldPosition, Vector3 worldVector, Vector2 screenVector, Axis axis )
        {
            this.worldPosition = worldPosition;
            this.worldVector = worldVector;
            this.screenVector = screenVector;
            this.axis = axis;
        }
    }
    public class CubeView : MonoBehaviour
    {
        [SerializeField] private Slider cubeSizeSlider;
        [SerializeField] private GameObject eachCubePrefab;
        [SerializeField] private float mouseDeltaThreshold;
        [SerializeField] private Image[] debugImages;

        private readonly Subject<Transform> _onStartCubeViewTrigger = new Subject<Transform>();
        public IObservable<Transform> onStartCubeViewTrigger() => _onStartCubeViewTrigger;
        private readonly Subject<int> _onSliderValueChangedTrigger = new Subject<int>();
        public IObservable<int> onSliderValueChangedTrigger() => _onSliderValueChangedTrigger;
        private readonly Subject<EachCubeController> _onInitEachCubeControllerTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onInitEachCubeControllerTrigger() => _onInitEachCubeControllerTrigger;
        private readonly Subject<EachCubeController> _onButtonDownOnEachCubeTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onButtonDownOnEachCubeTrigger() => _onButtonDownOnEachCubeTrigger;
        private readonly Subject<PlaneData> _onChangedForRotatePlaneTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onChangedForRotatePlaneTrigger() => _onChangedForRotatePlaneTrigger;
        private readonly Subject<int> _onFinishedPlaneRotateTrigger = new Subject<int>();
        public IObservable<int> onFinishedPlaneRotateTrigger() => _onFinishedPlaneRotateTrigger;

        private float eachCubeMargin;
        private float cubeSize;
        private Vector3 parentCubeCenter;
        private CubeRotateStatus cubeRotateStatus;
        private Vector2 hitSurfacePosOnScreen;
        private Vector3 hitSurfacePos;
        private Vector3 cubeSurfaceAxis;
        // private List<Vector3> druggingEachCubeAxisVectorsOnScreen;
        private EachCubeController druggingEachCube;
        private List<DruggingEachCubeAxisData> druggingEachCubeAxisDatas;
        private PlaneData forRotatePlaneData;
        private Vector3 axisVectorOnWorld;
        private Vector3 planeRotateOnRotateStart;

        void Start()
        {
            SetSubscribe();

            _onStartCubeViewTrigger.OnNext(this.transform);
        }

        private void SetSubscribe()
        {
            cubeSizeSlider
                .OnValueChangedAsObservable()
                .Subscribe( size => _onSliderValueChangedTrigger.OnNext( (int)size ) )
                .AddTo(this);

            Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonDown(0) )
                .Subscribe( _ =>
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit_info = new RaycastHit();
                    float max_distance = 100f;

                    if ( Physics.Raycast(ray, out hit_info, max_distance) )
                    {
                        if (hit_info.transform.tag == "Surface")
                        {
                            hitSurfacePosOnScreen = Input.mousePosition;
                            hitSurfacePos = hit_info.point;
                            cubeSurfaceAxis = hit_info.transform.parent.transform.position - hit_info.transform.position;
                            OnDrugCube(Input.mousePosition);
                            _onButtonDownOnEachCubeTrigger.OnNext(hit_info.transform.parent.GetComponent<EachCubeController>());
                        }
                    }
                })
                .AddTo(this);
        }

        public void InitCube()
        {
            int max = (int)cubeSizeSlider.maxValue;
            eachCubeMargin = eachCubePrefab.transform.localScale.x;
            for(int x=0; x<max; x++)
            {
                for(int y=0; y<max; y++)
                {
                    for(int z=0; z<max; z++)
                    {
                        GameObject eachCube = Instantiate(eachCubePrefab);
                        eachCube.name = eachCubePrefab.name;
                        eachCube.transform.parent = this.transform;
                        eachCube.transform.localPosition += new Vector3( x*eachCubeMargin, y*eachCubeMargin, z*eachCubeMargin );
                        eachCube.transform.localScale = Vector3.zero;
                        EachCubeController eachCubeController = eachCube.GetComponent<EachCubeController>();
                        eachCubeController.SetPosOnCube( new Vector3(x,y,z) );
                        eachCubeController.SetOrder( Math.Max( Math.Max(x,y), z) + 1 );
                        _onInitEachCubeControllerTrigger.OnNext(eachCubeController);
                    }
                }
            }
            forRotatePlaneData = new PlaneData();
            forRotatePlaneData.transform = new GameObject().transform;
            forRotatePlaneData.transform.parent = this.transform;
            forRotatePlaneData.transform.name = "For Rotate Plane";
            _onSliderValueChangedTrigger.OnNext( (int)cubeSizeSlider.value );
        }

        public void OnChangeCubeSize(int cubeSize)
        {
            this.cubeSize = cubeSize;
            parentCubeCenter = new Vector3( (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f );
            this.transform
                .DOMove( parentCubeCenter, 0.5f )
                .SetEase( Ease.OutQuint );
        }

        public void DestoryUnnecessaryCubes(EachCubeController eachCube)
        {
            Destroy(eachCube.gameObject);
        }

        public void OnSetDruggingEachCube(EachCubeController eachCube)
        {
            druggingEachCube = eachCube;

            druggingEachCubeAxisDatas = new List<DruggingEachCubeAxisData>();

            druggingEachCubeAxisDatas.Add( new DruggingEachCubeAxisData( hitSurfacePos + druggingEachCube.transform.right,   druggingEachCube.transform.right,   Vector2.zero, Axis.X ) );
            druggingEachCubeAxisDatas.Add( new DruggingEachCubeAxisData( hitSurfacePos + druggingEachCube.transform.up,      druggingEachCube.transform.up,      Vector2.zero, Axis.Y ) );
            druggingEachCubeAxisDatas.Add( new DruggingEachCubeAxisData( hitSurfacePos + druggingEachCube.transform.forward, druggingEachCube.transform.forward, Vector2.zero, Axis.Z ) );

            for(int i=0; i<druggingEachCubeAxisDatas.Count; i++)
            {
                float dot = Vector3.Dot( cubeSurfaceAxis.normalized, druggingEachCubeAxisDatas[i].worldVector.normalized );
                if ( Math.Abs(dot) < 0.9f )
                {
                    DruggingEachCubeAxisData data = druggingEachCubeAxisDatas[i];
                    data.screenVector = ( Camera.main.WorldToScreenPoint(druggingEachCubeAxisDatas[i].worldPosition) - Camera.main.WorldToScreenPoint(hitSurfacePos) );
                    druggingEachCubeAxisDatas[i] = data;
                }
            }
            druggingEachCubeAxisDatas = druggingEachCubeAxisDatas
                .Where( data => data.screenVector != Vector2.zero )
                .ToList();

            // デバグ用
            for(int i=0; i<druggingEachCubeAxisDatas.Count; i++)
            {
                debugImages[i].rectTransform.position = new Vector2( druggingEachCubeAxisDatas[i].screenVector.x, druggingEachCubeAxisDatas[i].screenVector.y ) + hitSurfacePosOnScreen;
            }
        }

        private void OnDrugCube(Vector2 onButtonDownMousePos)
        {
            Vector2 mouseVector = Vector2.zero;
            cubeRotateStatus = CubeRotateStatus.ニュートラル;
            Vector2 druggingEachCubeAxisVectorOnScreen = Vector2.zero;
            Vector2 lastMousePos = onButtonDownMousePos;
            forRotatePlaneData.transform.localPosition = -parentCubeCenter;

            var mouseUp = Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) );
            var drug = Observable.EveryUpdate()
                .TakeUntil(mouseUp)
                .Subscribe( _ =>
                {
                    mouseVector = new Vector2(Input.mousePosition.x, Input.mousePosition.y)  - onButtonDownMousePos;
                    switch(cubeRotateStatus)
                    {
                        case CubeRotateStatus.ニュートラル:
                            foreach( DruggingEachCubeAxisData data in druggingEachCubeAxisDatas )
                            {
                                if( (Vector2.Dot(data.screenVector.normalized, mouseVector.normalized) * mouseVector).magnitude - data.screenVector.magnitude >= 1f )
                                {
                                    cubeRotateStatus = CubeRotateStatus.回転中;
                                    druggingEachCubeAxisVectorOnScreen = data.screenVector;
                                    SetForRotatePlane( mouseVector.magnitude, data.worldVector );
                                }
                            }
                            break;
                        case CubeRotateStatus.回転中:
                            float deltaX = (Input.mousePosition.x - lastMousePos.x) / Screen.width;
                            float deltaY = (Input.mousePosition.y - lastMousePos.y) / Screen.height;
                            RotatePlane( Vector3.Dot( druggingEachCubeAxisVectorOnScreen, new Vector2( deltaX, deltaY )) );
                            break;
                    }
                    lastMousePos = Input.mousePosition;
                }, () =>
                {
                    if (cubeRotateStatus == CubeRotateStatus.回転中)
                    {
                        ComplementRotate(); 
                    }
                })
                .AddTo(this);
        }
        private void SetForRotatePlane(float delta, Vector3 basisVector)
        {
            Vector3 deltaVectorOnWorld = basisVector * delta;
            axisVectorOnWorld = Vector3.Cross(deltaVectorOnWorld, cubeSurfaceAxis).normalized;
            float absDotX = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.right ) );
            float absDotY = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.up ) );
            float absDotZ = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.forward ) );
            float maxDot = Mathf.Max( Mathf.Max( absDotX, absDotY ), absDotZ );

            if( maxDot == absDotX )
            {
                forRotatePlaneData.transform.forward = this.transform.right;
                forRotatePlaneData.axis = Axis.X;
                Debug.Log("X軸回転");
            }
            if( maxDot == absDotY )
            {
                forRotatePlaneData.transform.forward = -this.transform.up;
                forRotatePlaneData.axis = Axis.Y;
                Debug.Log("Y軸回転");
            }
            if( maxDot == absDotZ )
            {
                forRotatePlaneData.transform.forward = -this.transform.forward;
                forRotatePlaneData.axis = Axis.Z;
                Debug.Log("Z軸回転");
            }
            planeRotateOnRotateStart = forRotatePlaneData.transform.localEulerAngles;
            _onChangedForRotatePlaneTrigger.OnNext(forRotatePlaneData);
        }

        private void RotatePlane(float delta)
        {
            forRotatePlaneData.transform.localEulerAngles += new Vector3( 0, 0, delta );
        }

        private void ComplementRotate()
        {
            float deltaPlaneRotate = (forRotatePlaneData.transform.localEulerAngles - planeRotateOnRotateStart).magnitude;
            int arrangedDelta = (int)deltaPlaneRotate + 45;
            arrangedDelta = arrangedDelta >= 360 ? arrangedDelta - 360 : arrangedDelta;
            int rotationNum = arrangedDelta / 90;
            
            Vector3 targetEulerAngles = Vector3.zero;
            switch(forRotatePlaneData.axis)
            {
                case Axis.X:
                    targetEulerAngles = new Vector3( forRotatePlaneData.transform.localEulerAngles.x, forRotatePlaneData.transform.localEulerAngles.y, planeRotateOnRotateStart.z + rotationNum * 90 );
                    break;
                case Axis.Y:
                    targetEulerAngles = new Vector3( forRotatePlaneData.transform.localEulerAngles.x, planeRotateOnRotateStart.y + rotationNum * 90, forRotatePlaneData.transform.localEulerAngles.z );
                    break;
                case Axis.Z:
                    targetEulerAngles = new Vector3( forRotatePlaneData.transform.localEulerAngles.x, forRotatePlaneData.transform.localEulerAngles.y, planeRotateOnRotateStart.z + rotationNum * 90 );
                    break;
            }

            forRotatePlaneData.transform
                .DOLocalRotate( targetEulerAngles, 0.5f, RotateMode.Fast )
                .SetEase(Ease.OutQuint)
                .OnComplete( () =>
                {
                    Debug.Log("rotationNum : " + rotationNum);
                    _onFinishedPlaneRotateTrigger.OnNext(rotationNum);
                });
        }

    }
}
