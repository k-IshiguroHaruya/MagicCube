
using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MagicCube
{
    public struct PlaneData
    {
        public Transform transform;
        public Axis axis;
        public int plusMinus;
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

        private Transform parentCubeTransform;
        private PlaneData rotatingPlaneData;
        private List<EachCubeController> eachCubesOnRotatingPlane;

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
            Debug.Log("触ったやつのキューブ座標 : " + _druggingEachCube.Value.posOnCube);
        }

        public void SetEachCubesParentPlane(PlaneData planeData)
        {
            rotatingPlaneData = planeData;
            eachCubesOnRotatingPlane = new List<EachCubeController>();

            int rotatingPlaneAxisPos = 0;
            switch(rotatingPlaneData.axis)
            {
                case Axis.X:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(_druggingEachCube.Value.posOnCube.x);
                    break;
                case Axis.Y:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(_druggingEachCube.Value.posOnCube.y);
                    break;
                case Axis.Z:
                    rotatingPlaneAxisPos = Mathf.FloorToInt(_druggingEachCube.Value.posOnCube.z);
                    break;
            }
            foreach( EachCubeController eachCube in _eachCubes )
            {
                int pos = 0;
                switch(rotatingPlaneData.axis)
                {
                    case Axis.X:
                        pos = Mathf.FloorToInt(eachCube.posOnCube.x);
                        break;
                    case Axis.Y:
                        pos = Mathf.FloorToInt(eachCube.posOnCube.y);
                        break;
                    case Axis.Z:
                        pos = Mathf.FloorToInt(eachCube.posOnCube.z);
                        break;
                }
                if( pos == rotatingPlaneAxisPos )
                {
                    eachCubesOnRotatingPlane.Add(eachCube);
                    eachCube.transform.parent = planeData.transform;
                }
            }
        }

        public void ReleaseEachCubesPlane(int rotationNum)
        {
            if ( rotationNum != 0)
            {
                Vector2 eachCubePosOnAxisPlane = Vector2.zero;
                float rotationNumRadian = -Mathf.PI / 2f * rotationNum;
                Vector3 rotatedEachCubeWorldPos = Vector3.zero;
                float centerMargin = (_cubeSize.Value-1) * 0.5f;
                foreach( EachCubeController eachCubeOnRotatingPlane in eachCubesOnRotatingPlane )
                {
                    // Debug.Log(eachCubeOnRotatingPlane.posOnCube);
                    // Debug.Log("↓");
                    switch(rotatingPlaneData.axis)
                    {
                        case Axis.X:
                            eachCubePosOnAxisPlane = new Vector2( eachCubeOnRotatingPlane.posOnCube.y, eachCubeOnRotatingPlane.posOnCube.z ) - new Vector2( centerMargin, centerMargin);
                            eachCubePosOnAxisPlane = MultipleRotationMatrix( eachCubePosOnAxisPlane, rotationNumRadian * rotatingPlaneData.plusMinus ) + new Vector2( centerMargin, centerMargin );
                            rotatedEachCubeWorldPos = new Vector3( eachCubeOnRotatingPlane.posOnCube.x, eachCubePosOnAxisPlane.x, eachCubePosOnAxisPlane.y );
                            break;
                        case Axis.Y:
                            eachCubePosOnAxisPlane = new Vector2( eachCubeOnRotatingPlane.posOnCube.z, eachCubeOnRotatingPlane.posOnCube.x ) - new Vector2( centerMargin, centerMargin );
                            eachCubePosOnAxisPlane = MultipleRotationMatrix( eachCubePosOnAxisPlane, rotationNumRadian ) + new Vector2( centerMargin, centerMargin );
                            rotatedEachCubeWorldPos = new Vector3( eachCubePosOnAxisPlane.y, eachCubeOnRotatingPlane.posOnCube.y, eachCubePosOnAxisPlane.x );
                            break;
                        case Axis.Z:
                            eachCubePosOnAxisPlane = new Vector2( eachCubeOnRotatingPlane.posOnCube.x, eachCubeOnRotatingPlane.posOnCube.y ) - new Vector2( centerMargin, centerMargin );
                            eachCubePosOnAxisPlane = MultipleRotationMatrix( eachCubePosOnAxisPlane, rotationNumRadian * rotatingPlaneData.plusMinus ) + new Vector2( centerMargin, centerMargin );
                            rotatedEachCubeWorldPos = new Vector3( eachCubePosOnAxisPlane.x, eachCubePosOnAxisPlane.y, eachCubeOnRotatingPlane.posOnCube.z );
                            break;
                    }
                    _eachCubes
                        .ToList()
                        .Find( eachCube => eachCube == eachCubeOnRotatingPlane )
                        .posOnCube = rotatedEachCubeWorldPos.Round();
                    // Debug.Log(eachCubeOnRotatingPlane.posOnCube);
                }
            }
            foreach( EachCubeController eachCube in _eachCubes )
            {
                eachCube.transform.parent = parentCubeTransform;
                eachCube.transform.position.Round();
            }
        }
        private Vector2 MultipleRotationMatrix( Vector2 matrix, float radian )
        {
            Vector2 rotatedMatrix = new Vector2
            (
                matrix.x * Mathf.Cos(radian) + matrix.y * Mathf.Sin(radian),
                matrix.x * -Mathf.Sin(radian) + matrix.y * Mathf.Cos(radian)
            );
            return rotatedMatrix;
        }

    }
}
