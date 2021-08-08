
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class CameraPresenter : IInitializable
    {
        private readonly CameraView cameraView;
        private readonly CubeView cubeView;
        private readonly ScreenView screenView;

        public CameraPresenter( CameraView cameraView, CubeView cubeView, ScreenView screenView )
        {
            this.cameraView = cameraView;
            this.cubeView = cubeView;
            this.screenView = screenView;
        }
        
        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            cubeView.onSliderValueChangedTrigger()
                .Subscribe( size => cameraView.OnCubeSizeChanged(size) );

            screenView.onButtonDownScreenTrigger()
                .Subscribe( _ => cameraView.OnDrugScreen() );
            screenView.zoom
                .Subscribe( zoom => cameraView.Zoom(zoom) );
        }

    }
}
