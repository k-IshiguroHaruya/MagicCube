
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MagicCube
{
    public struct PlaneData
    {
        public Transform transform;
        public Axis axis;
        public int plusMinus;
        public int rotationNum;
        public Quaternion localRotation;
        public bool isUndoRotate;
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

        private Transform parentCubeTransform;
        private List<EachCubeController> eachCubesOnRotatingPlane;
        private Stack<PlaneData> rotatedPlaneStack = new Stack<PlaneData>();


        public void SetParentCubeTransform(Transform transform)
        {
            parentCubeTransform = transform;
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
            int rotatingPlaneAxisPos = 0;
            Vector3 druggingEachCubePosition = _druggingEachCube.Value.transform.position;
            switch(planeData.axis)
            {
                case Axis.X:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(druggingEachCubePosition.x);
                    break;
                case Axis.Y:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(druggingEachCubePosition.y);
                    break;
                case Axis.Z:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(druggingEachCubePosition.z);
                    break;
            }
            
            eachCubesOnRotatingPlane = new List<EachCubeController>();
            foreach( EachCubeController eachCube in _eachCubes )
            {
                int axisPos = 0;
                Vector3 eachCubePosition = eachCube.transform.position;
                switch(planeData.axis)
                {
                    case Axis.X:
                        axisPos = Mathf.FloorToInt(eachCubePosition.x);
                        break;
                    case Axis.Y:
                        axisPos = Mathf.FloorToInt(eachCubePosition.y);
                        break;
                    case Axis.Z:
                        axisPos = Mathf.FloorToInt(eachCubePosition.z);
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
            if ( planeData.rotationNum != 0 && planeData.isUndoRotate == false)
            {
                planeData.localRotation = planeData.transform.localRotation;
                // Debug.Log
                // (
                //     "planeData.transform : " + planeData.transform + 
                //     ", planeData.axis : " + planeData.axis +
                //     ", planeData.plusMinus : " + planeData.plusMinus +
                //     ", planeData.rotationNum : " + planeData.rotationNum +
                //     ", planeData.localRotation : " + planeData.localRotation +
                //     ", planeData.isUndoRotate : " + planeData.isUndoRotate
                // );
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
            undoPlaneData.rotationNum = 4 - undoPlaneData.rotationNum;
            undoPlaneData.isUndoRotate = true;
            undoPlaneData.transform.localRotation = undoPlaneData.localRotation;

            SetEachCubesParentPlane(undoPlaneData);
            _undoRotatePlaneTrigger.OnNext(undoPlaneData);
        }

    }
}
