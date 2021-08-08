
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

        public ScreenPresenter( ScreenModel screenModel, ScreenView screenView, CubeModel cubeModel )
        {
            this.screenModel = screenModel;
            this.screenView = screenView;
            this.cubeModel = cubeModel;
        }

        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            screenModel.screenType
                .Subscribe( screenType => screenView.ChangeScreen(screenType) );
            
            screenView.onClickStartGameButtonTrigger()
                .Subscribe( screenType => screenModel.SetScreenType(screenType) );

            cubeModel.isRotatingPlane
                .Subscribe( flag => screenView.SetIsRotatingPlane(flag) );
            cubeModel.onFinishScranblePlaneTrigger()
                .Delay(TimeSpan.FromSeconds(1))
                .Subscribe( _ => screenView.StartGame() );
        }

    }
}
