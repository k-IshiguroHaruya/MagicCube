
using UnityEngine;
using UniRx;
using System;

namespace MagicCube
{
    public class ScreenView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private readonly Subject<ScreenType> _onClickStartGameButtonTrigger = new Subject<ScreenType>();
        public IObservable<ScreenType> onClickStartGameButtonTrigger() => _onClickStartGameButtonTrigger;

        public void ChangeScreen(ScreenType screenType)
        {
            switch(screenType)
            {
                case ScreenType.ホーム画面:
                    break;
                case ScreenType.キューブサイズ調整画面:
                    break;
                case ScreenType.メイン画面:
                    canvas.enabled = false;
                    break;
                case ScreenType.エンディング画面:
                    break;
            }
        }
        public void OnClickStartGameButton()
        {
            _onClickStartGameButtonTrigger.OnNext(ScreenType.メイン画面);
        }

    }
}
