
using VContainer.Unity;
using UniRx;

namespace MagicCube
{
    public class CubePresenter : IInitializable
    {
        private readonly CubeModel cubeModel;
        private readonly CubeView cubeView;
        private readonly ScreenModel screenModel;

        public CubePresenter( CubeModel cubeModel, CubeView cubeView, ScreenModel screenModel )
        {
            this.cubeModel = cubeModel;
            this.cubeView = cubeView;
            this.screenModel = screenModel;
        }

        public void Initialize()
        {
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            cubeModel.cubeSize
                .SkipLatestValueOnSubscribe()
                .Subscribe( size => cubeView.OnChangeCubeSize(size) );
            cubeModel.druggingEachCube
                .SkipLatestValueOnSubscribe()
                .Subscribe( eachCube => cubeView.OnSetDruggingEachCube(eachCube) );
            cubeModel.eachCubes
                .ObserveRemove()
                .Subscribe( eachCubes => cubeView.DestoryUnnecessaryCubes(eachCubes.Value) );
            cubeModel.undoRotatePlaneTrigger()
                .Subscribe( planeData => cubeView.RotatePlaneAnimation(planeData, 0.5f) );
            cubeModel.onRotatePlaneFromDataTrigger()
                .Subscribe( planeData => cubeView.RotatePlaneAnimation(planeData, 0.05f) );
            
            cubeView.onStartCubeViewTrigger()
                .Subscribe( transform => cubeModel.SetParentCubeTransform(transform) );
            cubeView.onInitForRotatePlaneTrigger()
                .Subscribe( transform => cubeModel.SetForRotatePlane(transform) );
            cubeView.onSliderValueChangedTrigger()
                .Subscribe( size => cubeModel.SetCubeSize(size) );
            cubeView.onInitEachCubeControllerTrigger()
                .Subscribe( eachCube => cubeModel.AddEachCubes(eachCube) );
            cubeView.onButtonDownOnEachCubeTrigger()
                .Subscribe( eachCube => cubeModel.SetDruggingEachCube(eachCube) );
            cubeView.onChangedForRotatePlaneTrigger()
                .Subscribe( planeData => cubeModel.SetEachCubesParentPlane(planeData) );
            cubeView.onFinishedPlaneRotateTrigger()
                .Subscribe( planeData => cubeModel.OnFinishedPlaneRotate(planeData) );
            cubeView.onClickUndoButtonTrigger()
                .Subscribe( _ => cubeModel.UndoRotatePlane() );
            cubeView.onClickRotateButtonTrigger()
                .Subscribe( _ => cubeModel.RotatePlaneFromData() );

            screenModel.screenType
                .Subscribe( screenType =>
                {
                    switch(screenType)
                    {
                        case ScreenType.キューブサイズ調整画面:
                            cubeView.InitCube();
                            break;
                        case ScreenType.メイン画面:
                            cubeView.InitOnMainScreen();
                            cubeModel.DeleteUnnecessaryCubes();
                            break;
                    }
                });
        }
    }
}
