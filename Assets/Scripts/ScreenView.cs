
using UnityEngine;
using UniRx;
using System;

namespace MagicCube
{
    public class ScreenView : MonoBehaviour
    {
        [SerializeField] private Canvas sizeAdjustmentCanvas;
        [SerializeField] private Canvas mainCanvas;

        private readonly Subject<ScreenType> _onClickStartGameButtonTrigger = new Subject<ScreenType>();
        public IObservable<ScreenType> onClickStartGameButtonTrigger() => _onClickStartGameButtonTrigger;

        public void ChangeScreen(ScreenType screenType)
        {
            switch(screenType)
            {
                case ScreenType.ホーム画面:
                    break;
                case ScreenType.キューブサイズ調整画面:
                    sizeAdjustmentCanvas.enabled = true;
                    mainCanvas.enabled = false;
                    break;
                case ScreenType.メイン画面:
                    sizeAdjustmentCanvas.enabled = false;
                    mainCanvas.enabled = true;
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
