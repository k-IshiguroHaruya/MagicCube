
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class CubePresenter : IInitializable
    {
        private readonly CubeModel cubeModel;
        private readonly CubeView cubeView;
        private readonly ScreenModel screenModel;

        public CubePresenter( CubeModel cubeModel, CubeView cubeView, ScreenModel screenModel )
        {
            this.cubeModel = cubeModel;
            this.cubeView = cubeView;
            this.screenModel = screenModel;
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

            screenModel.screenType
                .Where( screenType => screenType == ScreenType.キューブサイズ調整画面 )
                .Subscribe( _ => cubeView.InitCube() );
        }
    }
}
