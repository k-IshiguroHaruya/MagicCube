
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

namespace MagicCube
{
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

        private readonly Subject<Transform> _onStartCubeViewTrigger = new Subject<Transform>();
        public IObservable<Transform> onStartCubeViewTrigger() => _onStartCubeViewTrigger;
        private readonly Subject<Transform> _onInitForRotatePlaneTrigger = new Subject<Transform>();
        public IObservable<Transform> onInitForRotatePlaneTrigger() => _onInitForRotatePlaneTrigger;
        private readonly Subject<int> _onSliderValueChangedTrigger = new Subject<int>();
        public IObservable<int> onSliderValueChangedTrigger() => _onSliderValueChangedTrigger;
        private readonly Subject<EachCubeController> _onInitEachCubeControllerTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onInitEachCubeControllerTrigger() => _onInitEachCubeControllerTrigger;
        private readonly Subject<EachCubeController> _onButtonDownOnEachCubeTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onButtonDownOnEachCubeTrigger() => _onButtonDownOnEachCubeTrigger;
        private readonly Subject<PlaneData> _onChangedForRotatePlaneTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onChangedForRotatePlaneTrigger() => _onChangedForRotatePlaneTrigger;
        private readonly Subject<PlaneData> _onFinishedPlaneRotateTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onFinishedPlaneRotateTrigger() => _onFinishedPlaneRotateTrigger;
        private readonly Subject<Unit> _onClickUndoButtonTrigger = new Subject<Unit>();
        public IObservable<Unit> onClickUndoButtonTrigger() => _onClickUndoButtonTrigger;
        private readonly Subject<bool> _onToggleIsRotatingPlaneTrigger = new Subject<bool>();
        public IObservable<bool> onToggleIsRotatingPlaneTrigger() => _onToggleIsRotatingPlaneTrigger;
        
        private float eachCubeMargin;
        private float cubeSize;
        private Vector3 parentCubeCenter;
        private Vector2 hitSurfacePosOnScreen;
        private Vector3 hitSurfacePos;
        private Vector3 cubeSurfaceAxis;
        private EachCubeController druggingEachCube;
        private List<DruggingEachCubeAxisData> druggingEachCubeAxisDatas;
        private PlaneData forRotatePlaneData;
        private Vector3 axisVectorOnWorld;
        private Vector3 planeEulerAnglesOnRotateStart;
        private bool isRotatingPlane;

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
        }

        public void OnClickUndoButton()
        {
            if (isRotatingPlane)
            {
                return;
            }
            _onClickUndoButtonTrigger.OnNext(Unit.Default);
        }

        public void SetIsRotatingPlane(bool isRotatingPlane)
        {
            this.isRotatingPlane = isRotatingPlane;
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

        public void InitOnMainScreen()
        {
            forRotatePlaneData.transform.localPosition = -parentCubeCenter;
            _onInitForRotatePlaneTrigger.OnNext(forRotatePlaneData.transform);
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

        }

        public void OnDrugCube(RaycastHit hit)
        {
            Vector2 onButtonDownMousePos = Input.mousePosition;
            Vector2 mouseVector = Vector2.zero;
            Vector2 druggingEachCubeAxisVectorOnScreen = Vector2.zero;
            Vector2 lastMousePos = onButtonDownMousePos;

            hitSurfacePosOnScreen = onButtonDownMousePos;
            hitSurfacePos = hit.point;
            cubeSurfaceAxis = hit.transform.parent.transform.position - hit.transform.position;

            var mouseUp = Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) );
            var drug = Observable.EveryUpdate()
                .TakeUntil(mouseUp)
                .Subscribe( _ =>
                {
                    mouseVector = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - onButtonDownMousePos;
                    if (isRotatingPlane == false)
                    {
                        foreach( DruggingEachCubeAxisData data in druggingEachCubeAxisDatas )
                        {
                            if( (Vector2.Dot(data.screenVector.normalized, mouseVector.normalized) * mouseVector).magnitude - data.screenVector.magnitude >= 1f )
                            {
                                _onToggleIsRotatingPlaneTrigger.OnNext(true);
                                druggingEachCubeAxisVectorOnScreen = data.screenVector;
                                SetForRotatePlane( mouseVector.magnitude, data.worldVector );
                                break;
                            }
                        }
                    }
                    else
                    {
                        float deltaX = (Input.mousePosition.x - lastMousePos.x) / Screen.width;
                        float deltaY = (Input.mousePosition.y - lastMousePos.y) / Screen.height;
                        RotatePlane( Vector3.Dot( druggingEachCubeAxisVectorOnScreen, new Vector2( deltaX, deltaY )) );
                    }
                    lastMousePos = Input.mousePosition;
                }, () =>
                {
                    if (isRotatingPlane == true)
                    {
                        ComplementRotatePlane(); 
                    }
                })
                .AddTo(this);

            _onButtonDownOnEachCubeTrigger.OnNext(hit.transform.parent.GetComponent<EachCubeController>());
        }
        private void SetForRotatePlane(float delta, Vector3 basisVector)
        {
            Vector3 deltaVectorOnWorld = basisVector * delta;
            axisVectorOnWorld = Vector3.Cross(deltaVectorOnWorld, cubeSurfaceAxis).normalized;
            Vector3 dots = new Vector3( Vector3.Dot( axisVectorOnWorld, this.transform.right ), Vector3.Dot( axisVectorOnWorld, this.transform.up ), Vector3.Dot( axisVectorOnWorld, this.transform.forward ) );
            Vector3 absDots = new Vector3( Mathf.Abs(dots.x), Mathf.Abs(dots.y), Mathf.Abs(dots.z) );
            float maxDot = Mathf.Max( Mathf.Max( absDots.x, absDots.y ), absDots.z );

            if( maxDot == absDots.x )
            {
                forRotatePlaneData.transform.forward = this.transform.right * dots.x;
                forRotatePlaneData.axis = Axis.X;
            }
            if( maxDot == absDots.y )
            {
                forRotatePlaneData.transform.forward = this.transform.up * dots.y;
                forRotatePlaneData.axis = Axis.Y;
            }
            if( maxDot == absDots.z )
            {
                forRotatePlaneData.transform.forward = this.transform.forward * dots.z;
                forRotatePlaneData.axis = Axis.Z;
            }
            planeEulerAnglesOnRotateStart = forRotatePlaneData.transform.localEulerAngles;
            forRotatePlaneData.druggingEachCubePosition = druggingEachCube.transform.position;
            _onChangedForRotatePlaneTrigger.OnNext(forRotatePlaneData);
        }

        private void RotatePlane(float delta)
        {
            forRotatePlaneData.transform.localEulerAngles += new Vector3( 0, 0, delta );
        }

        private void ComplementRotatePlane()
        {            
            float deltaPlaneRotate = (forRotatePlaneData.transform.localEulerAngles - planeEulerAnglesOnRotateStart).magnitude;
            int arrangedDelta = (int)deltaPlaneRotate + 45;
            arrangedDelta = arrangedDelta >= 360 ? arrangedDelta - 360 : arrangedDelta;
            forRotatePlaneData.rotatedNum = arrangedDelta / 90;
            forRotatePlaneData.baseEulerAngles = planeEulerAnglesOnRotateStart;
            forRotatePlaneData.isNeedUndoLog = true;
            
            RotatePlaneAnimation(forRotatePlaneData, 0.5f);
        }

        public void RotatePlaneAnimation(PlaneData planeData, float animationDuration)
        {
            Vector3 targetEulerAngles = Vector3.zero;
            switch(planeData.axis)
            {
                case Axis.X:
                    targetEulerAngles = new Vector3( planeData.transform.localEulerAngles.x, planeData.transform.localEulerAngles.y, planeData.baseEulerAngles.z + planeData.rotatedNum * 90 );
                    break;
                case Axis.Y:
                    targetEulerAngles = new Vector3( planeData.transform.localEulerAngles.x, planeData.baseEulerAngles.y + planeData.rotatedNum * 90, planeData.transform.localEulerAngles.z );
                    break;
                case Axis.Z:
                    targetEulerAngles = new Vector3( planeData.transform.localEulerAngles.x, planeData.transform.localEulerAngles.y, planeData.baseEulerAngles.z + planeData.rotatedNum * 90 );
                    break;
            }

            planeData.transform
                .DOLocalRotate( targetEulerAngles, animationDuration, RotateMode.Fast )
                .SetEase(Ease.OutQuint)
                .OnComplete( () =>
                {
                    planeData.baseEulerAngles = targetEulerAngles;
                    _onFinishedPlaneRotateTrigger.OnNext(planeData);
                });
        }

    }
}
