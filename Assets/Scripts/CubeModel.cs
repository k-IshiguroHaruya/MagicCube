
using UniRx;
using UnityEngine;

namespace MagicCube
{
    public struct PlaneData
    {
        public Transform transform;
        public Axis axis;
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

        public void SetCubeSize(int cubeSize)
        {
            _cubeSize.Value = cubeSize;

            foreach( EachCubeController eachCube in _eachCubes )
            {
                if ( eachCube.order <= cubeSize )
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

        public void SetDruggingEachCube(EachCubeController eachCube)
        {
            _druggingEachCube.Value = eachCube;
        }

        public void SetEachCubesPlane(PlaneData planeData)
        {
            foreach( EachCubeController eachCube in _eachCubes )
            {
                float pos = 0;
                float druggingPlanePos = 0;
                switch(planeData.axis)
                {
                    case Axis.X:
                        pos = eachCube.posOnCube.x;
                        druggingPlanePos = _druggingEachCube.Value.posOnCube.x;
                        break;
                    case Axis.Y:
                        pos = eachCube.posOnCube.y;
                        druggingPlanePos = _druggingEachCube.Value.posOnCube.y;
                        break;
                    case Axis.Z:
                        pos = eachCube.posOnCube.z;
                        druggingPlanePos = _druggingEachCube.Value.posOnCube.z;
                        break;
                }
                if ( pos == druggingPlanePos )
                {
                    eachCube.transform.parent = planeData.transform;
                }
            }
        }

        public void ReleaseEachCubesPlane(Transform parent)
        {
            foreach( EachCubeController eachCube in _eachCubes )
            {
                eachCube.transform.parent = parent;
            }
        }

    }
}
