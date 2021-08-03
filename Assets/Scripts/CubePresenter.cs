
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class CubePresenter : IInitializable
    {
        private readonly CubeModel cubeModel;
        private readonly CubeView cubeView;

        public CubePresenter( CubeModel cubeModel, CubeView cubeView )
        {
            this.cubeModel = cubeModel;
            this.cubeView = cubeView;
        }

        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            cubeModel.cubeSize
                .SkipLatestValueOnSubscribe()
                .Subscribe( size => cubeView.ChangeCubeSize(size) );
            
            cubeView.onSliderValueChangedTrigger()
                .Subscribe( size => cubeModel.SetCubeSize(size) );
        }
    }
}
