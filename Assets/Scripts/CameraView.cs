
using UnityEngine;
using DG.Tweening;
using UniRx;

namespace MagicCube
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private Transform cubeViewTransform;

        private Vector3 positionMargin;
        private Quaternion rotationMargin;
        private Vector3 positionMarginVector;
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
                .SetEase(Ease.OutQuint);
        }

        public void OnDrugScreen()
        {
            Vector2 onButtonDownMousePos = Input.mousePosition;
            Vector2 mouseVector = Vector2.zero;
            Vector2 lastMousePos = onButtonDownMousePos;
            Vector3 rotateAxis = Vector3.zero;
            float delta = 0;

            float threshold = Screen.width * 0.01f;

            var mouseUp = Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonUp(0) );
            var drug = Observable.EveryUpdate()
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
                .AddTo(this);
        }
        private void RotateCamera( Vector3 rotateAxis, float deltaAngle )
        {
            this.transform.RotateAround( Vector3.zero, rotateAxis, deltaAngle );
        }

        public void Zoom(float zoom)
        {
            Camera.main.transform.position += Camera.main.transform.forward * zoom;
        }

    }
}
