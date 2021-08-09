
using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

namespace MagicCube
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private Transform cubeViewTransform;

        private readonly Subject<Unit> _onFinishedClearedCameraAnimationTrigger = new Subject<Unit>();
        public IObservable<Unit> onFinishedClearedCameraAnimationTrigger() => _onFinishedClearedCameraAnimationTrigger;

        private CompositeDisposable disposables;
        private Vector3 positionMargin;
        private Quaternion rotationMargin;
        private Vector3 positionMarginVector;
        private Vector3 positionGameStart;
        private Vector3 eulerAnglesGameStart;
        private Axis druggingScreenAxis;
        private bool isRotatingCamera;
        private const float drugCoefficient = 50f;

        void Start()
        {
            positionMargin = this.transform.position;
            rotationMargin = this.transform.rotation;
            positionMarginVector = positionMargin.normalized;

            this.transform.LookAt(Vector3.zero);
        }

        public void OnCubeSizeChanged(int size)
        {
            this.transform
                .DOMove( positionMargin + positionMarginVector * (size-1) * 2f, 0.5f )
                .SetEase(Ease.OutQuint)
                .OnComplete( () =>
                {
                    positionGameStart = this.transform.position;
                    eulerAnglesGameStart = this.transform.eulerAngles;
                });
        }

        public void OnDrugScreen()
        {
            if (disposables == null)
            {
                disposables = new CompositeDisposable();
            }

            Vector2 onButtonDownMousePos = Input.mousePosition;
            Vector2 mouseVector = Vector2.zero;
            Vector2 lastMousePos = onButtonDownMousePos;
            Vector3 rotateAxis = Vector3.zero;
            float delta = 0;

            float threshold = Screen.width * 0.01f;

            var mouseUp = Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) );
            Observable.EveryUpdate()
                .TakeUntil(mouseUp)
                .Subscribe( _ =>
                {
                    mouseVector = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - onButtonDownMousePos;
                    if( isRotatingCamera == false )
                    {
                        if( Mathf.Abs(mouseVector.x) >= threshold )
                        {
                            druggingScreenAxis = Axis.Y;
                            isRotatingCamera = true;
                        }
                        else if( Mathf.Abs(mouseVector.y) >= threshold )
                        {
                            druggingScreenAxis = Axis.X;
                            isRotatingCamera = true;
                        }
                    }
                    else
                    {
                        switch(druggingScreenAxis)
                        {
                            case Axis.Y:
                                rotateAxis = Vector3.up;
                                delta = (Input.mousePosition.x - lastMousePos.x) / Screen.width;
                                break;
                            case Axis.X:
                                rotateAxis = this.transform.right;
                                delta = -(Input.mousePosition.y - lastMousePos.y) / Screen.height;
                                break;
                        }
                        RotateCamera( rotateAxis, delta * drugCoefficient );
                        lastMousePos = Input.mousePosition;
                    }
                }, () =>
                {
                    isRotatingCamera = false;
                })
                .AddTo(disposables);
        }
        private void RotateCamera( Vector3 rotateAxis, float deltaAngle )
        {
            this.transform.RotateAround( Vector3.zero, rotateAxis, deltaAngle );
        }

        public void Zoom(float zoom)
        {
            Camera.main.transform.position += Camera.main.transform.forward * zoom;
        }

        public void OnClearMagicCube()
        {
            this.transform
                .DOMove( positionGameStart, 1f )
                .SetEase( Ease.InOutQuint );
            this.transform
                .DORotate( eulerAnglesGameStart, 1f )
                .SetEase( Ease.InOutQuint )
                .OnComplete( () =>
                {
                    Vector3 rotateAxis = (this.transform.up + this.transform.right).normalized;
                    float deltaAngle = 0;
                    DOTween
                        .To( () => deltaAngle, (x) => deltaAngle = x, 15, 3f)
                        .SetEase(Ease.InOutQuint)
                        .OnUpdate( () => RotateCamera( rotateAxis, deltaAngle ) )
                        .OnComplete( () =>
                        {
                            this.transform
                                .DOMove( positionGameStart, 1.5f )
                                .SetEase( Ease.OutQuint );
                            this.transform
                                .DORotate( eulerAnglesGameStart, 1.5f )
                                .SetEase( Ease.OutQuint )
                                .OnComplete( () =>
                                {
                                    _onFinishedClearedCameraAnimationTrigger.OnNext(Unit.Default);
                                });
                        });
                });
        }

        public void DisposeDrugScreen()
        {
            if (disposables != null)
            {
                disposables.Dispose();
            }
        }


    }
}
