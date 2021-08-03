
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;

namespace MagicCube
{
    public class CubeView : MonoBehaviour
    {
        [SerializeField] private Slider cubeSizeSlider;
        [SerializeField] private GameObject eachCubePrefab;

        // private readonly ReactiveProperty<int> _cubeSize = new ReactiveProperty<int>();
        // public IReadOnlyReactiveProperty<int> cubeSize { get => _cubeSize; }
        private readonly Subject<int> _onSliderValueChangedTrigger = new Subject<int>();
        public IObservable<int> onSliderValueChangedTrigger() => _onSliderValueChangedTrigger;

        private float eachCubeMargin;
        private EachCubeController[,,] eachCubeControllers;

        void Start()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            cubeSizeSlider
                .OnValueChangedAsObservable()
                .Subscribe( size => _onSliderValueChangedTrigger.OnNext( (int)size ) )
                .AddTo(this);
        }

        public void InitCube()
        {
            int max = (int)cubeSizeSlider.maxValue;
            eachCubeControllers = new EachCubeController[max,max,max];
            eachCubeMargin = eachCubePrefab.transform.localScale.x;
            for(int i=0; i<max; i++)
            {
                for(int j=0; j<max; j++)
                {
                    for(int k=0; k<max; k++)
                    {
                        GameObject eachCube = Instantiate(eachCubePrefab);
                        eachCube.transform.parent = this.transform;
                        eachCube.transform.localPosition += new Vector3( i*eachCubeMargin, j*eachCubeMargin, k*eachCubeMargin );
                        eachCube.transform.localScale = Vector3.zero;
                        EachCubeController eachCubeController = eachCube.GetComponent<EachCubeController>();
                        eachCubeController.SetOrder( Math.Max( Math.Max(i,j), k) + 1 );
                        eachCubeControllers[i,j,k] = eachCubeController;
                    }
                }
            }
            // this.transform.localPosition += new Vector3( (-max+eachCubeMargin)/2f, (-max+eachCubeMargin)/2f, (-max+eachCubeMargin)/2f );
        }

        public void ChangeCubeSize(int cubeSize)
        {
            this.transform.position = new Vector3( (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f, (-cubeSize+eachCubeMargin)/2f );
            int max = (int)cubeSizeSlider.maxValue;
            for(int i=0; i<max; i++)
            {
                for(int j=0; j<max; j++)
                {
                    for(int k=0; k<max; k++)
                    {
                        if ( eachCubeControllers[i,j,k].order == (int)cubeSizeSlider.value )
                        {
                            eachCubeControllers[i,j,k].Expand();
                        }
                        else if (eachCubeControllers[i,j,k].order == (int)cubeSizeSlider.value + 1 )
                        {
                            eachCubeControllers[i,j,k].Contract();
                        }

                    }
                }
            }
        }

    }
}
