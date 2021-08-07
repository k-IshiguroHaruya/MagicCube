
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
                .Subscribe( planeData => cubeView.AutoRotatePlane(planeData) );
            
            cubeView.onStartCubeViewTrigger()
                .Subscribe( transform => cubeModel.SetParentCubeTransform(transform) );
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

            screenModel.screenType
                .Subscribe( screenType =>
                {
                    switch(screenType)
                    {
                        case ScreenType.キューブサイズ調整画面:
                            cubeView.InitCube();
                            break;
                        case ScreenType.メイン画面:
                            cubeModel.DeleteUnnecessaryCubes();
                            break;
                    }
                });
        }
    }
}
