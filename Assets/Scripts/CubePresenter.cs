
using VContainer.Unity;
using UniRx;
using System;

namespace MagicCube
{
    public class CubePresenter : IInitializable
    {
        private readonly CubeModel cubeModel;
        private readonly CubeView cubeView;
        private readonly ScreenModel screenModel;
        private readonly ScreenView screenView;

        public CubePresenter( CubeModel cubeModel, CubeView cubeView, ScreenModel screenModel, ScreenView screenView )
        {
            this.cubeModel = cubeModel;
            this.cubeView = cubeView;
            this.screenModel = screenModel;
            this.screenView = screenView;
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
            cubeModel.isRotatingPlane
                .Subscribe( flag => cubeView.SetIsRotatingPlane(flag) );
            
            cubeView.onInitCubeTrigger()
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
            cubeView.onToggleIsRotatingPlaneTrigger()
                .Subscribe( isRotatingPlane => cubeModel.SetIsRotatingPlane(isRotatingPlane) );
            cubeView.onDestroyCubeChildrenTrigger()
                .Subscribe( _ => cubeModel.OnDestroyCubeChildren() );

            screenModel.screenType
                .Subscribe( screenType =>
                {
                    switch(screenType)
                    {
                        case ScreenType.ホーム画面:
                            cubeView.DestroyCubeChildren( cubeModel.eachCubes );
                            break;
                        case ScreenType.キューブサイズ調整画面:
                            cubeView.DestroyCubeChildren( cubeModel.eachCubes );
                            cubeView.InitCube();
                            break;
                        case ScreenType.メイン画面:
                            cubeView.InitOnMainScreen();
                            cubeModel.DeleteUnnecessaryCubes();
                            Observable
                                .Timer(TimeSpan.FromSeconds(1))
                                .Subscribe( _ => cubeModel.ScramblePlane() );
                            break;
                    }
                });
            
            screenView.onRayCastHitSurfaceTrigger()
                .Subscribe( rayCastHit => cubeView.OnDrugCube(rayCastHit) );
        }
    }
}
