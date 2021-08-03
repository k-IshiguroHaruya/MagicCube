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
        }
    }

}