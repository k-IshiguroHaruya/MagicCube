
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

namespace MagicCube
{
    public class ScreenView : MonoBehaviour
    {
        [SerializeField] private Canvas homeCanvas;
        [SerializeField] private Button continueButton;
        [SerializeField] private Canvas sizeAdjustmentCanvas;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Text timerText;
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private Canvas endingCanvas;
        [SerializeField] private Canvas confirmDialog;

        private readonly Subject<Unit> _onStartScreenViewTrigger = new Subject<Unit>();
        public IObservable<Unit> onStartScreenViewTrigger() => _onStartScreenViewTrigger;

        private readonly Subject<ScreenType> _onClickStartGameButtonTrigger = new Subject<ScreenType>();
        public IObservable<ScreenType> onClickStartGameButtonTrigger() => _onClickStartGameButtonTrigger;
        private readonly Subject<RaycastHit> _onRayCastHitSurfaceTrigger = new Subject<RaycastHit>();
        public IObservable<RaycastHit> onRayCastHitSurfaceTrigger() => _onRayCastHitSurfaceTrigger;
        private readonly Subject<Unit> _onButtonDownScreenTrigger = new Subject<Unit>();
        public IObservable<Unit> onButtonDownScreenTrigger() => _onButtonDownScreenTrigger;
        
        private ReactiveProperty<float> _zoom = new ReactiveProperty<float>();
        public IObservable<float> zoom { get => _zoom; }

        private bool isRotatingPlane;

        void Start()
        {
            _onStartScreenViewTrigger.OnNext(Unit.Default);
        }

        public void SetActiveHomeContinueButton(bool isActive)
        {
            continueButton.gameObject.SetActive(isActive);
        }

        public void SetIsRotatingPlane(bool isRotatingPlane)
        {
            this.isRotatingPlane = isRotatingPlane;
        }

        public void ChangeScreen(ScreenType screenType)
        {
            switch(screenType)
            {
                case ScreenType.ホーム画面:
                    homeCanvas.gameObject.SetActive(true);
                    confirmDialog.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(false);
                    mainCanvas.gameObject.SetActive(false);
                    menuCanvas.gameObject.SetActive(false);
                    endingCanvas.gameObject.SetActive(false);
                    break;
                case ScreenType.キューブサイズ調整画面:
                    homeCanvas.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(true);
                    mainCanvas.gameObject.SetActive(false);
                    menuCanvas.gameObject.SetActive(false);
                    endingCanvas.gameObject.SetActive(false);
                    break;
                case ScreenType.メイン画面:
                    homeCanvas.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(false);
                    mainCanvas.gameObject.SetActive(true);
                    break;
                case ScreenType.エンディング画面:
                    break;
            }
        }

        public void DisplayConfirmDialog()
        {
            confirmDialog.gameObject.SetActive(true);
        }

        public void OnClickStartButton()
        {
            _onClickStartGameButtonTrigger.OnNext(ScreenType.キューブサイズ調整画面);
        }
        public void OnClickStartGameButton()
        {
            _onClickStartGameButtonTrigger.OnNext(ScreenType.メイン画面);
        }
        public void OnClickBackTitleButton()
        {
            _onClickStartGameButtonTrigger.OnNext(ScreenType.ホーム画面);
        }

        public void StartGame()
        {
            ObserveMouseButtonDown();
            ObserveMouseScrollWheel();
        }

        private void ObserveMouseButtonDown()
        {
            Observable.EveryUpdate()
                .Where( _ => Input.GetMouseButtonDown(0) )
                .Subscribe( _ =>
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    float max_distance = 100f;

                    if ( Physics.Raycast(ray, out hit, max_distance) )
                    {
                        if (hit.transform.tag == "Surface" && isRotatingPlane == false)
                        {
                            _onRayCastHitSurfaceTrigger.OnNext(hit);
                        }
                    }
                    else
                    {
                        _onButtonDownScreenTrigger.OnNext(Unit.Default);
                    }
                })
                .AddTo(this);
        }

        private void ObserveMouseScrollWheel()
        {
            Observable.EveryUpdate()
                .Subscribe( _ =>
                {
                    _zoom.Value = Input.GetAxis("Mouse ScrollWheel");
                })
                .AddTo(this);
        }

    }
}
