
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using DG.Tweening;
namespace MagicCube
{
    public enum CubeRotateStatus
    {
        ニュートラル,
        横回転中,
        縦回転中,
    }
    public class CubeView : MonoBehaviour
    {
        [SerializeField] private Slider cubeSizeSlider;
        [SerializeField] private GameObject eachCubePrefab;
        [SerializeField] private float mouseDeltaThreshold;

        private readonly Subject<int> _onSliderValueChangedTrigger = new Subject<int>();
        public IObservable<int> onSliderValueChangedTrigger() => _onSliderValueChangedTrigger;
        private readonly Subject<EachCubeController> _onInitEachCubeControllerTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onInitEachCubeControllerTrigger() => _onInitEachCubeControllerTrigger;
        private readonly Subject<EachCubeController> _onButtonDownOnEachCubeTrigger = new Subject<EachCubeController>();
        public IObservable<EachCubeController> onButtonDownOnEachCubeTrigger() => _onButtonDownOnEachCubeTrigger;
        private readonly Subject<PlaneData> _onChangedForRotatePlaneTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onChangedForRotatePlaneTrigger() => _onChangedForRotatePlaneTrigger;
        private readonly Subject<Transform> _onFinishedPlaneRotateTrigger = new Subject<Transform>();
        public IObservable<Transform> onFinishedPlaneRotateTrigger() => _onFinishedPlaneRotateTrigger;

        private float eachCubeMargin;
        private float cubeSize;
        private Vector3 targetPos;
        private CubeRotateStatus cubeRotateStatus;
        private Vector3 cubeSurfaceAxis;
        private EachCubeController druggingEachCube;
        private Transform forRotatePlane;
        private Vector3 axisVectorOnWorld;

        void Start()
        {
            SetSubscribe();
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
                            cubeSurfaceAxis = hit_info.transform.parent.transform.position - hit_info.transform.position;
                            OnDrugCube();
                            _onButtonDownOnEachCubeTrigger.OnNext(hit_info.transform.parent.GetComponent<EachCubeController>());
                        }
                    }
                })
                .AddTo(this);
            Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) )
                .Subscribe( _ =>
                {
                    _onFinishedPlaneRotateTrigger.OnNext(this.transform);
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
            forRotatePlane = new GameObject().transform;
            forRotatePlane.transform.parent = this.transform;
            forRotatePlane.name = "For Rotate Plane";
            _onSliderValueChangedTrigger.OnNext( (int)cubeSizeSlider.value );
        }

        public void OnChangeCubeSize(int cubeSize)
        {
            this.cubeSize = cubeSize;
            targetPos = new Vector3( (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f );
            this.transform
                .DOMove( targetPos, 0.5f )
                .SetEase( Ease.OutQuint );
        }

        private void OnDrugCube()
        {
            Vector2 mouseDeltaProduct = Vector2.zero;
            cubeRotateStatus = CubeRotateStatus.ニュートラル;

            var mouseUp = Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) );
            var drug = Observable.EveryUpdate()
                .TakeUntil(mouseUp)
                .Subscribe( _ =>
                {
                    float mouseDeltaX = Input.GetAxis("Mouse X");
                    float mouseDeltaY = Input.GetAxis("Mouse Y");
                    switch(cubeRotateStatus)
                    {
                        case CubeRotateStatus.ニュートラル:
                            mouseDeltaProduct += new Vector2( mouseDeltaX, mouseDeltaY );
                            if ( Mathf.Abs(mouseDeltaProduct.x) >= mouseDeltaThreshold )
                            {
                                cubeRotateStatus = CubeRotateStatus.横回転中;
                                SetForRotatePlane(mouseDeltaX, Camera.main.transform.right);
                            }
                            if ( Mathf.Abs(mouseDeltaProduct.y) >= mouseDeltaThreshold)
                            {
                                cubeRotateStatus = CubeRotateStatus.縦回転中;
                                SetForRotatePlane(mouseDeltaY, Camera.main.transform.up);
                            }
                            break;
                        case CubeRotateStatus.横回転中:
                            RotatePlane(mouseDeltaX);
                            break;
                        case CubeRotateStatus.縦回転中:
                            RotatePlane(mouseDeltaY);
                            break;
                    }
                })
                .AddTo(this);
        }
        private void SetForRotatePlane(float delta, Vector3 basisVector)
        {
            Vector3 deltaVectorOnWorld = basisVector * Mathf.Abs(delta);
            axisVectorOnWorld = Vector3.Cross(deltaVectorOnWorld, cubeSurfaceAxis).normalized;
            float absDotX = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.right ) );
            float absDotY = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.up ) );
            float absDotZ = Mathf.Abs( Vector3.Dot( axisVectorOnWorld, this.transform.forward ) );
            float maxDot = Mathf.Max( Mathf.Max( absDotX, absDotY ), absDotZ );

            PlaneData planeData = new PlaneData();
            if ( maxDot == absDotX )
            {
                forRotatePlane.transform.position = new Vector3( eachCubeMargin/2f + eachCubeMargin * (druggingEachCube.posOnCube.x - cubeSize/2f), 0, 0 );
                forRotatePlane.transform.forward = this.transform.right;
                planeData.axis = Axis.X;
            }
            if ( maxDot == absDotY )
            {
                forRotatePlane.transform.position = new Vector3( 0, eachCubeMargin/2f + eachCubeMargin * (druggingEachCube.posOnCube.y - cubeSize/2f), 0 );
                forRotatePlane.transform.forward = this.transform.up;
                planeData.axis = Axis.Y;
            }
            if ( maxDot == absDotZ )
            {
                forRotatePlane.transform.position = new Vector3( 0, 0, eachCubeMargin/2f + eachCubeMargin * (druggingEachCube.posOnCube.z - cubeSize/2f) );
                forRotatePlane.transform.forward = this.transform.forward;
                planeData.axis = Axis.Z;
            }
            planeData.transform = forRotatePlane;
            _onChangedForRotatePlaneTrigger.OnNext(planeData);
        }

        private void RotatePlane(float delta)
        {
            delta *= 10f;
            forRotatePlane.Rotate(axisVectorOnWorld, delta, Space.World);
        }

        public void OnSetDruggingEachCube(EachCubeController eachCube)
        {
            druggingEachCube = eachCube;
        }

    }
}
