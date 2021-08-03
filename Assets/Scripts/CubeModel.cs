
using UniRx;

namespace MagicCube
{
    public class CubeModel
    {
        private readonly ReactiveProperty<int> _cubeSize = new ReactiveProperty<int>();
        public IReadOnlyReactiveProperty<int> cubeSize { get => _cubeSize; }

        public void SetCubeSize(int cubeSize)
        {
            _cubeSize.Value = cubeSize;
        }

    }
}
