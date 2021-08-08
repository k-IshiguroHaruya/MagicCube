
using UnityEngine;
using DG.Tweening;

namespace MagicCube
{
    public class EachCubeController : MonoBehaviour
    {   
        private int _order;
        public int order { get => _order; }

        public void SetOrder(int order)
        {
            if (order == 1)
            {
                order = 2;
            }
            _order = order;
        }

        public void SetScale( Vector3 targetScale, float duration )
        {
            if ( this.transform.localScale == targetScale )
            {
                return;
            }
            this.transform
                .DOScale( targetScale, duration )
                .SetEase( Ease.OutSine );
            this.transform
                .DOLocalRotate( new Vector3(360,360,360), duration, RotateMode.FastBeyond360 )
                .SetEase( Ease.OutSine );
        }

    }
}
