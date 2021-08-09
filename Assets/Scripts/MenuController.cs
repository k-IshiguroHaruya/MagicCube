
using UnityEngine;
using DG.Tweening;

namespace MagicCube
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform backgroundRectTransform;

        private float firstPosX;

        void Start()
        {
            firstPosX = backgroundRectTransform.localPosition.x;
            backgroundRectTransform.localPosition = new Vector2( Screen.width, backgroundRectTransform.localPosition.y );
        }

        public void OpenMenu()
        {
            backgroundRectTransform
                .DOLocalMoveX( firstPosX, 0.5f )
                .SetEase( Ease.OutQuint );
        }
        public void CloseMenu()
        {
            backgroundRectTransform
                .DOLocalMoveX( Screen.width, 0.5f )
                .SetEase( Ease.OutQuint );
        }

    }
}
