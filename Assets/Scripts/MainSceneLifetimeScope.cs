using VContainer;
using VContainer.Unity;

namespace MagicCube
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CubePresenter>(Lifetime.Singleton);
            builder.Register<CubeModel>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<CubeView>();

            builder.RegisterEntryPoint<ScreenPresenter>(Lifetime.Singleton);
            builder.Register<ScreenModel>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<ScreenView>();

            builder.RegisterEntryPoint<CameraPresenter>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<CameraView>();
        }
    }

}
