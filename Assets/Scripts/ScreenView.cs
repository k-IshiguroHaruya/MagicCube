
using UnityEngine;

namespace MagicCube
{
    public class ScreenView : MonoBehaviour
    {

        public void ChangeScreen(ScreenType screenType)
        {
            switch(screenType)
            {
                case ScreenType.ホーム画面:
                    break;
                case ScreenType.キューブサイズ調整画面:
                    break;
                case ScreenType.メイン画面:
                    break;
                case ScreenType.エンディング画面:
                    break;
            }
        }

    }
}
