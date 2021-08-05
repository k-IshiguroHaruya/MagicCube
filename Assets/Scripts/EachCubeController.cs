
using UnityEngine;
using DG.Tweening;

namespace MagicCube
{
    public class EachCubeController : MonoBehaviour
    {
        private Vector3 _posOnCube;
        public Vector3 posOnCube { get => _posOnCube; }
        
        private int _order;
        public int order { get => _order; }

        public void SetPosOnCube(Vector3 posOnCube)
        {
            _posOnCube = posOnCube;
        }

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
                .SetEase( Ease.OutQuint );
        }

    }
}
