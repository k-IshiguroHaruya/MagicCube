
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;

namespace MagicCube
{
    public class CubeView : SingletonMonoBehaviour<CubeView>
    {
        [SerializeField] private Slider cubeSizeSlider;

        // private readonly ReactiveProperty<int> _cubeSize = new ReactiveProperty<int>();
        // public IReadOnlyReactiveProperty<int> cubeSize { get => _cubeSize; }
        private readonly Subject<int> _onSliderValueChangedTrigger = new Subject<int>();
        public IObservable<int> onSliderValueChangedTrigger() => _onSliderValueChangedTrigger;

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

        public void ChangeCubeSize(int cubeSize)
        {
            Debug.Log("きてるきてる" + cubeSize);
        }

    }
}
