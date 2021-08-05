
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class ScreenPresenter : IInitializable
    {
        private readonly ScreenModel screenModel;
        private readonly ScreenView screenView;

        public ScreenPresenter( ScreenModel screenModel, ScreenView screenView )
        {
            this.screenModel = screenModel;
            this.screenView = screenView;
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
        }

    }
}
