
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System;

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
        private readonly ReactiveCollection<EachCubeController> _eachCubes = new ReactiveCollection<EachCubeController>();
        public IReadOnlyReactiveCollection<EachCubeController> eachCubes { get => _eachCubes; }
        private readonly ReactiveProperty<EachCubeController> _druggingEachCube = new ReactiveProperty<EachCubeController>();
        public IReadOnlyReactiveProperty<EachCubeController> druggingEachCube { get => _druggingEachCube; }
        private readonly Subject<PlaneData> _undoRotatePlaneTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> undoRotatePlaneTrigger() => _undoRotatePlaneTrigger;
        private readonly Subject<PlaneData> _onRotatePlaneFromDataTrigger = new Subject<PlaneData>();
        public IObservable<PlaneData> onRotatePlaneFromDataTrigger() => _onRotatePlaneFromDataTrigger;

        private Transform parentCubeTransform;
        private Transform forRotatePlane;
        private List<EachCubeController> eachCubesOnRotatingPlane;
        private Stack<PlaneData> rotatedPlaneStack = new Stack<PlaneData>();


        public void SetParentCubeTransform(Transform parentCubeTransform)
        {
            this.parentCubeTransform = parentCubeTransform;
        }

        public void SetForRotatePlane(Transform forRotatePlane)
        {
            this.forRotatePlane = forRotatePlane;
        }

        public void SetCubeSize(int cubeSize)
        {
            _cubeSize.Value = cubeSize;

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
        }

        public void UndoRotatePlane()
        {
            if (rotatedPlaneStack.Count == 0)
            {
                return;
            }
            PlaneData undoPlaneData = rotatedPlaneStack.Pop();
            undoPlaneData.rotatedNum = 4 - undoPlaneData.rotatedNum;
            undoPlaneData.isNeedUndoLog = false;
            undoPlaneData.transform.localRotation = undoPlaneData.localRotation;

            SetEachCubesParentPlane(undoPlaneData);
            _undoRotatePlaneTrigger.OnNext(undoPlaneData);
        }
        public void RotatePlaneFromData()
        {
            PlaneData planeData = new PlaneData();
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

    }
}
