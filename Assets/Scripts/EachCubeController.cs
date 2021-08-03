
using UnityEngine;
using DG.Tweening;

namespace MagicCube
{
    public class EachCubeController : MonoBehaviour
    {
        public int order { get; private set;}

        public void SetOrder(int order)
        {
            if (order == 1)
            {
                order = 2;
            }
            this.order = order;
        }

        public void Expand()
        {
            this.transform
                .DOScale( Vector3.one, 0.5f );
        }
        public void Contract()
        {
            this.transform
                .DOScale( Vector3.zero, 0.5f );
        }

    }
}
