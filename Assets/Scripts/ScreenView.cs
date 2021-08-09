
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
        [SerializeField] private GameObject timerGameObject;
        [SerializeField] private Text timerText;
        [SerializeField] private MenuController menu;
        [SerializeField] private Button menuButton;
        [SerializeField] private Toggle timerToggle;
        [SerializeField] private GameObject undoButtonGameObject;
        [SerializeField] private Canvas endingCanvas;
        [SerializeField] private Text endingTitle;
        [SerializeField] private Text leftTimeText;
        [SerializeField] private ConfirmDialogController confirmDialogController;

        private readonly Subject<Unit> _onStartScreenViewTrigger = new Subject<Unit>();
        public IObservable<Unit> onStartScreenViewTrigger() => _onStartScreenViewTrigger;

        private readonly Subject<ScreenType> _onApplyChangeScreenTypeTrigger = new Subject<ScreenType>();
        public IObservable<ScreenType> onApplyChangeScreenTypeTrigger() => _onApplyChangeScreenTypeTrigger;
        private readonly Subject<RaycastHit> _onRayCastHitSurfaceTrigger = new Subject<RaycastHit>();
        public IObservable<RaycastHit> onRayCastHitSurfaceTrigger() => _onRayCastHitSurfaceTrigger;
        private readonly Subject<Unit> _onButtonDownScreenTrigger = new Subject<Unit>();
        public IObservable<Unit> onButtonDownScreenTrigger() => _onButtonDownScreenTrigger;
        
        private ReactiveProperty<float> _zoom = new ReactiveProperty<float>();
        public IObservable<float> zoom { get => _zoom; }

        private CompositeDisposable disposables;
        private int timeLimit;
        private bool isRotatingPlane;
        private bool isClearMagicCube;

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
                    confirmDialogController.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(false);
                    mainCanvas.gameObject.SetActive(false);
                    endingCanvas.gameObject.SetActive(false);
                    menuButton.gameObject.SetActive(false);
                    undoButtonGameObject.SetActive(false);
                    
                    timerGameObject.SetActive(false);
                    isClearMagicCube = false;
                    ResetData();
                    break;
                case ScreenType.キューブサイズ調整画面:
                    homeCanvas.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(true);
                    mainCanvas.gameObject.SetActive(false);
                    endingCanvas.gameObject.SetActive(false);
                    menuButton.gameObject.SetActive(false);
                    confirmDialogController.gameObject.SetActive(false);
                    undoButtonGameObject.SetActive(false);
                    ResetData();
                    break;
                case ScreenType.メイン画面:
                    homeCanvas.gameObject.SetActive(false);
                    sizeAdjustmentCanvas.gameObject.SetActive(false);
                    mainCanvas.gameObject.SetActive(true);
                    menuButton.gameObject.SetActive(true);
                    undoButtonGameObject.SetActive(true);
                    break;
                case ScreenType.エンディング画面:
                    mainCanvas.gameObject.SetActive(false);
                    endingCanvas.gameObject.SetActive(true);
                    menuButton.gameObject.SetActive(false);
                    SetEndingTexts();
                    break;
            }
        }

        public void ToggleActiveTimerGameObject()
        {
            timerGameObject.SetActive(timerToggle.isOn);
        }
        public void OnClickMenuButton()
        {
            menuButton.gameObject.SetActive(false);
            menu.OpenMenu();
        }
        public void OnClickCloseMenuButton()
        {
            menuButton.gameObject.SetActive(true);
            menu.CloseMenu();
        }
        public void OnClickStartButton()
        {
            _onApplyChangeScreenTypeTrigger.OnNext(ScreenType.キューブサイズ調整画面);
        }
        public void OnClickStartGameButton()
        {
            _onApplyChangeScreenTypeTrigger.OnNext(ScreenType.メイン画面);
        }
        public void OnClickBackTitleButton()
        {
            _onApplyChangeScreenTypeTrigger.OnNext(ScreenType.ホーム画面);
        }
        public void OnClickRestartButtonOnMenu()
        {
            confirmDialogController.gameObject.SetActive(true);
            confirmDialogController.SetUI
            ( 
                title: "ゲームをリスタートしますか?",
                yesButtonAction: () =>
                {
                    menu.CloseMenu();
                    OnClickStartButton();
                },
                yesButtonText: "はい",
                noButtonAction: () => confirmDialogController.gameObject.SetActive(false),
                noButtonText: "いいえ"
            );
        }
        public void OnClickBackTitleButtonOnMenu()
        {
            confirmDialogController.gameObject.SetActive(true);
            confirmDialogController.SetUI
            ( 
                title: "タイトル画面に戻りますか?",
                yesButtonAction: () =>
                {
                    menu.CloseMenu();
                    OnClickBackTitleButton();
                },
                yesButtonText: "はい",
                noButtonAction: () => confirmDialogController.gameObject.SetActive(false),
                noButtonText: "いいえ"
            );
        }
        public void OnClickConfirmDialogBackground()
        {
            confirmDialogController.gameObject.SetActive(false);
        }

        public void RestartGameFromEnding()
        {
            _onApplyChangeScreenTypeTrigger.OnNext(ScreenType.キューブサイズ調整画面);
        }

        public void OnClickCloseEndingButton()
        {
            endingCanvas.gameObject.SetActive(false);
        }

        private void ResetData()
        {
            if ( disposables != null )
            {
                disposables.Dispose();
            }
            timeLimit = 0;
        }

        public void StartGame(int cubeSize)
        {
            timeLimit = (cubeSize*cubeSize*cubeSize) * 200;
            disposables = new CompositeDisposable();
            ObserveMouseButtonDown();
            ObserveMouseScrollWheel();
            StartTimerCountDown();
        }

        public void OnClearMagicCube()
        {
            isClearMagicCube = true;
        }

        private void SetEndingTexts()
        {
            if( isClearMagicCube == true )
            {
                endingTitle.text = "おめでとう!!";
                leftTimeText.text = "残り時間" + timerText.text;
            }
            else
            {
                endingTitle.text = "残念、時間切れだ";
                leftTimeText.text = "";
            }
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
                .AddTo(disposables);
        }

        private void ObserveMouseScrollWheel()
        {
            Observable.EveryUpdate()
                .Subscribe( _ =>
                {
                    _zoom.Value = Input.GetAxis("Mouse ScrollWheel");
                })
                .AddTo(disposables);
        }

        private void StartTimerCountDown()
        {
            timerGameObject.SetActive(true);
            
            int countDownTime = timeLimit;
            int hour;
            int minutes;
            int seconds;

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .TakeWhile( _ => countDownTime > 0 )
                .Subscribe( _ =>
                {
                    hour = countDownTime / 3600;
                    minutes = (countDownTime - hour * 3600) / 60;
                    seconds = countDownTime - hour * 3600 - minutes * 60;
                    timerText.text = string.Format( "{0}:{1:00}:{2:00}", hour, minutes, seconds );
                    countDownTime --;
                }, () =>
                {
                    _onApplyChangeScreenTypeTrigger.OnNext(ScreenType.エンディング画面);
                })
                .AddTo(disposables);
        }

    }
}
