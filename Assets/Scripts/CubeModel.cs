
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MagicCube
{
    public struct PlaneData
    {
        public Transform transform;
        public Axis axis;
        public Vector3 druggingEachCubePosition;
        public Quaternion localRotation;
        public int rotatedNum;
        public Vector3 baseEulerAngles;
        public bool isNeedUndoLog;
        public bool isScramble;
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public class CubeModel
    {
        private readonly ReactiveProperty<int> _cubeSize = new ReactiveProperty<int>();
        public IReadOnlyReactiveProperty<int> cubeSize { get => _cubeSize; }
        private ReactiveCollection<EachCubeController> _eachCubes = new ReactiveCollection<EachCubeController>();
        public IReadOnlyReactiveCollection<EachCubeController> eachCubes { get => _eachCubes; }
        private readonly ReactiveProperty<EachCubeController> _druggingEachCube = new ReactiveProperty<EachCubeController>();
        public IReadOnlyReactiveProperty<EachCubeController> druggingEachCube { get => _druggingEachCube; }
        private readonly Subject<PlaneData> _undoRotatePlaneTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> undoRotatePlaneTrigger() => _undoRotatePlaneTrigger;
        private readonly Subject<PlaneData> _onRotatePlaneFromDataTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onRotatePlaneFromDataTrigger() => _onRotatePlaneFromDataTrigger;
        private readonly ReactiveProperty<bool> _isRotatingPlane = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> isRotatingPlane { get => _isRotatingPlane; }
        private readonly Subject<Unit> _onFinishScranblePlaneTrigger = new Subject<Unit>();
        public IObservable<Unit> onFinishScranblePlaneTrigger() => _onFinishScranblePlaneTrigger;
        private readonly Subject<Unit> _onClearMagicCubeTrigger = new Subject<Unit>();
        public IObservable<Unit> onClearMagicCubeTrigger() => _onClearMagicCubeTrigger;

        private Transform parentCubeTransform;
        private Transform forRotatePlane;
        private List<EachCubeController> eachCubesOnRotatingPlane;
        private Stack<PlaneData> rotatedPlaneStack = new Stack<PlaneData>();
        private int scrambleNum = 2;
        private int scrambleCount;
        private List<Transform> eachCubeTransforms;


        public void SetParentCubeTransform(Transform parentCubeTransform)
        {
            this.parentCubeTransform = parentCubeTransform;
        }

        public void SetForRotatePlane(Transform forRotatePlane)
        {
            this.forRotatePlane = forRotatePlane;
        }

        public void SetIsRotatingPlane(bool isRotatingPlane)
        {
            _isRotatingPlane.Value = isRotatingPlane;
        }

        public void SetCubeSize(int cubeSize)
        {
            _cubeSize.Value = cubeSize;
            scrambleNum = (_cubeSize.Value*_cubeSize.Value*_cubeSize.Value);

            foreach( EachCubeController eachCube in _eachCubes )
            {
                if( eachCube.order <= cubeSize )
                {
                    eachCube.SetScale( Vector3.one, 0.5f );
                }
                else
                {
                    eachCube.SetScale( Vector3.zero, 0.5f );
                }
            }
        }

        public void AddEachCubes(EachCubeController eachCube)
        {
            _eachCubes.Add(eachCube);
        }

        public void DeleteUnnecessaryCubes()
        {
            for(int i=_eachCubes.Count-1; i>=0; i--)
            {
                if( _eachCubes[i].order > _cubeSize.Value )
                {
                    _eachCubes.RemoveAt(i);
                }
            }
            eachCubeTransforms = _eachCubes.Select( eachCube => eachCube.transform ).ToList();
        }

        public void SetDruggingEachCube(EachCubeController eachCube)
        {
            _druggingEachCube.Value = eachCube;
        }

        public void SetEachCubesParentPlane(PlaneData planeData)
        {
            float rotatingPlaneAxisPos = 0;
            Vector3 druggingEachCubePosition = planeData.druggingEachCubePosition - parentCubeTransform.position;
            switch(planeData.axis)
            {
                case Axis.X:
                    rotatingPlaneAxisPos = Mathf.Round(druggingEachCubePosition.x);
                    break;
                case Axis.Y:
                    rotatingPlaneAxisPos = Mathf.Round(druggingEachCubePosition.y);
                    break;
                case Axis.Z:
                    rotatingPlaneAxisPos = Mathf.Round(druggingEachCubePosition.z);
                    break;
            }
            
            eachCubesOnRotatingPlane = new List<EachCubeController>();
            foreach( EachCubeController eachCube in _eachCubes )
            {
                float axisPos = 0;
                Vector3 eachCubePosition = eachCube.transform.position - parentCubeTransform.position;;
                switch(planeData.axis)
                {
                    case Axis.X:
                        axisPos = Mathf.Round(eachCubePosition.x);
                        break;
                    case Axis.Y:
                        axisPos = Mathf.Round(eachCubePosition.y);
                        break;
                    case Axis.Z:
                        axisPos = Mathf.Round(eachCubePosition.z);
                        break;
                }
                if( axisPos == rotatingPlaneAxisPos )
                {
                    eachCubesOnRotatingPlane.Add(eachCube);
                    eachCube.transform.parent = planeData.transform;
                }
            }
        }

        public void OnFinishedPlaneRotate(PlaneData planeData)
        {
            if( planeData.rotatedNum != 0 && planeData.isNeedUndoLog == true)
            {
                planeData.localRotation = planeData.transform.localRotation;
                rotatedPlaneStack.Push(planeData);
            }
            foreach( EachCubeController eachCube in _eachCubes )
            {
                eachCube.transform.parent = parentCubeTransform;
                eachCube.transform.position.Round();
            }

            if (planeData.isScramble == true)
            {
                ScramblePlane();
            }
            else
            {
                _isRotatingPlane.Value = false;
            }

            // if ( planeData.isNeedUndoLog == true && planeData.isScramble == false ) ほんとはこっち
            if( planeData.isScramble == false )
            {
                IsCorrect();
            }
        }

        public void UndoRotatePlane()
        {
            if (rotatedPlaneStack.Count == 0)
            {
                return;
            }
            _isRotatingPlane.Value = true;

            PlaneData undoPlaneData = rotatedPlaneStack.Pop();
            undoPlaneData.rotatedNum = 4 - undoPlaneData.rotatedNum;
            undoPlaneData.isNeedUndoLog = false;
            undoPlaneData.isScramble = false;
            undoPlaneData.transform.localRotation = undoPlaneData.localRotation;

            SetEachCubesParentPlane(undoPlaneData);
            _undoRotatePlaneTrigger.OnNext(undoPlaneData);
        }

        public void ScramblePlane()
        {
            PlaneData planeData = new PlaneData();
            planeData.isScramble = true;

            if (scrambleCount < scrambleNum)
            {
                _isRotatingPlane.Value = true;
                RotatePlaneFromData(planeData);
                scrambleCount++;
            }
            else
            {
                _isRotatingPlane.Value = false;
                _onFinishScranblePlaneTrigger.OnNext(Unit.Default);
            }
        }
        private void RotatePlaneFromData(PlaneData planeData)
        {
            planeData.transform = forRotatePlane;
            planeData.axis = (Axis)Enum.ToObject(typeof(Axis), UnityEngine.Random.Range(0, 3));
            switch(planeData.axis)
            {
                case Axis.X:
                    forRotatePlane.forward = parentCubeTransform.right;
                    break;
                case Axis.Y:
                    forRotatePlane.forward = parentCubeTransform.up;
                    break;
                case Axis.Z:
                    forRotatePlane.forward = parentCubeTransform.forward;
                    break;
            }
            planeData.druggingEachCubePosition = _eachCubes[ UnityEngine.Random.Range(0, _eachCubes.Count) ].transform.position;
            planeData.rotatedNum = UnityEngine.Random.Range(1, 4);
            planeData.baseEulerAngles = forRotatePlane.localEulerAngles;
            planeData.isNeedUndoLog = true;
            planeData.localRotation = forRotatePlane.localRotation;

            SetEachCubesParentPlane(planeData);
            _onRotatePlaneFromDataTrigger.OnNext(planeData);
        }

        private bool IsCorrect()
        {
            int[] baseEulerAnglesInt = new int[]{ Mathf.RoundToInt(eachCubeTransforms[0].localEulerAngles.x), Mathf.RoundToInt(eachCubeTransforms[0].localEulerAngles.y), Mathf.RoundToInt(eachCubeTransforms[0].localEulerAngles.z) };
            foreach( Transform transform in eachCubeTransforms )
            {
                int eulerX = Mathf.RoundToInt(transform.localEulerAngles.x);
                int eulerY = Mathf.RoundToInt(transform.localEulerAngles.y);
                int eulerZ = Mathf.RoundToInt(transform.localEulerAngles.z);
                eulerX = eulerX == 360 ? 0 : eulerX;
                eulerY = eulerY == 360 ? 0 : eulerY;
                eulerZ = eulerZ == 360 ? 0 : eulerZ;
                if ( baseEulerAnglesInt[0] != eulerX || baseEulerAnglesInt[1] != eulerY || baseEulerAnglesInt[2] != eulerZ )
                {
                    return false;
                }
            }
            Debug.Log("正解！！");
            _onClearMagicCubeTrigger.OnNext(Unit.Default);
            return true;
        }

        public void OnDestroyCubeChildren()
        {
            _eachCubes = new ReactiveCollection<EachCubeController>();
            parentCubeTransform = null;
            scrambleCount = 0;
        }

    }
}
