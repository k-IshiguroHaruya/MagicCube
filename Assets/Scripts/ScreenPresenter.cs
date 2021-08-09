
using VContainer.Unity;
using UniRx;
using System;

namespace MagicCube
{
    public class ScreenPresenter : IInitializable
    {
        private readonly ScreenModel screenModel;
        private readonly ScreenView screenView;
        private readonly CubeModel cubeModel;
        private readonly CameraView cameraView;

        public ScreenPresenter( ScreenModel screenModel, ScreenView screenView, CubeModel cubeModel, CameraView cameraView )
        {
            this.screenModel = screenModel;
            this.screenView = screenView;
            this.cubeModel = cubeModel;
            this.cameraView = cameraView;
        }

        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            screenModel.screenType
                .Subscribe( screenType => screenView.ChangeScreen(screenType) );
            
            screenView.onStartScreenViewTrigger()
                .Subscribe( _ => screenView.SetActiveHomeContinueButton( screenModel.GetIsLastGameData() ) );
            screenView.onApplyChangeScreenTypeTrigger()
                .Subscribe( screenType => screenModel.SetScreenType(screenType) );

            cubeModel.isRotatingPlane
                .Subscribe( flag => screenView.SetIsRotatingPlane(flag) );
            cubeModel.onFinishScranblePlaneTrigger()
                .Delay(TimeSpan.FromSeconds(1))
                .Subscribe( _ => screenView.StartGame(cubeModel.cubeSize.Value) );
            cubeModel.onClearMagicCubeTrigger()
                .Subscribe( _ => screenView.OnClearMagicCube() );

            cameraView.onFinishedClearedCameraAnimationTrigger()
                .Subscribe( _ => screenModel.SetScreenType(ScreenType.エンディング画面) );
        }

    }
}
