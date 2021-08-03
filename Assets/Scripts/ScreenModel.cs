
using UniRx;

namespace MagicCube
{
    public enum ScreenType
    {
        ホーム画面,
        キューブサイズ調整画面,
        メイン画面,
        エンディング画面,
    }

    public class ScreenModel
    {
        private readonly ReactiveProperty<ScreenType> _screenType = new ReactiveProperty<ScreenType>( ScreenType.キューブサイズ調整画面 );
        public IReadOnlyReactiveProperty<ScreenType> screenType { get => _screenType; }

        public void SetScreenType(ScreenType screenType)
        {
            _screenType.Value = screenType;
        }

    }
}
