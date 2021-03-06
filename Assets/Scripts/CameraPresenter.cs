
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class CameraPresenter : IInitializable
    {
        private readonly CameraView cameraView;
        private readonly CubeView cubeView;
        private readonly CubeModel cubeModel;
        private readonly ScreenView screenView;
        private readonly ScreenModel screenModel;

        public CameraPresenter( CameraView cameraView, CubeView cubeView, CubeModel cubeModel, ScreenView screenView, ScreenModel screenModel )
        {
            this.cameraView = cameraView;
            this.cubeModel = cubeModel;
            this.cubeView = cubeView;
            this.screenView = screenView;
            this.screenModel = screenModel;
        }
        
        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            cubeView.onSliderValueChangedTrigger()
                .Subscribe( size => cameraView.OnCubeSizeChanged(size) );

            cubeModel.onClearMagicCubeTrigger()
                .Subscribe( _ => cameraView.OnClearMagicCube() );

            screenView.onButtonDownScreenTrigger()
                .Subscribe( _ => cameraView.OnDrugScreen() );
            screenView.zoom
                .Subscribe( zoom => cameraView.Zoom(zoom) );
            
            screenModel.screenType
                .Where( screenType => screenType == ScreenType.ホーム画面 )
                .Subscribe( _ => cameraView.DisposeDrugScreen() );
        }

    }
}
