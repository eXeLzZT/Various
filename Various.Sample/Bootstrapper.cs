using ReactiveUI;
using Splat;
using Various.Sample.Services;
using Various.Sample.Services.Interfaces;
using Various.Sample.ViewModels;
using Various.Sample.Views;

namespace Various.Sample;

internal class Bootstrapper
{
    private readonly IMutableDependencyResolver _mutableResolver;
    private readonly IReadonlyDependencyResolver _resolver;

    public ReactiveObject MainViewModel { get; }

    public Bootstrapper(IMutableDependencyResolver? mutableResolver = null, IReadonlyDependencyResolver? resolver = null)
    {
        _mutableResolver = mutableResolver ?? Locator.CurrentMutable;
        _resolver = resolver ?? Locator.Current;

        RegisterServices();
        RegisterViews();

        MainViewModel = new MainViewModel();
    }

    private void RegisterServices()
    {
        _mutableResolver.RegisterConstant<ISampleService>(new SampleService());
    }

    private void RegisterViews()
    {
        _mutableResolver.Register<IViewFor<MainViewModel>>(() => new MainView());
        _mutableResolver.Register<IViewFor<NotificationViewModel>>(() => new NotificationView());
        _mutableResolver.Register<IViewFor<FlowItemViewModel>>(() => new FlowItemView());
    }
}
