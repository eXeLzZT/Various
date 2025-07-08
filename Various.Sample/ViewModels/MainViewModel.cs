using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Various.Sample.Services.Interfaces;
using Various.Utils;
using Various.Wpf.Controls;

namespace Various.Sample.ViewModels;

public class MainViewModel : ReactiveObject, IUseReactiveModal
{
    private readonly ISampleService? _sampleService;
    private readonly IDialogService? _dialogService;

    private ReadOnlyObservableCollection<Item> _structItems;
    //private SourceList<int> _list1;

    [Reactive] public ReactiveObject ModalContent { get; set; }

    private SourceList<SampleItem> _defaultItems;

    [Reactive] public bool IsModalOpen { get; set; }
    public ReadOnlyObservableCollection<Item> StructItems => _structItems;
    public ObservableCollection<FlowItemViewModel> FlowItemViewModels { get; }

    public ICommand CommandCloseDialog { get; }
    public ICommand CommandOpenDialog { get; }
    public ICommand CommandOpenAppBar { get; }
    public ICommand CommandUpdateSampleService { get; }

    internal MainViewModel(ISampleService? sampleService = null, IDialogService? dialogService = null)
    {
        _sampleService = sampleService ?? Locator.Current.GetService<ISampleService>();
        _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

        ModalContent = new NotificationViewModel("Hello this is a text.");

        _defaultItems = new SourceList<SampleItem>();
        _defaultItems.AddRange(new List<SampleItem> { new SampleItem("", "All"), new SampleItem("OWN", "Own") });

        _sampleService?.Connect()
            .Merge(_defaultItems.Connect())
            .Transform(x => new Item(x.Id, x.Name))
            .Sort(SortExpressionComparer<Item>.Ascending(x => x.Name))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _structItems)
            .DisposeMany()
            .Subscribe();

        FlowItemViewModels = new ObservableCollection<FlowItemViewModel>();

        for (var i = 0; i < 15; i++)
        {
            var rdm = new Random().Next(1, 3);
            FlowItemViewModels.Add(new FlowItemViewModel
            {
                Name = $"Test Card ({i})",
                Info = $"Test info ({i})",
                Group = rdm switch
                {
                    1 => "Grp1",
                    2 => "Grp2",
                    _ => "Grp3"
                },
            });
        }

        //_list1 = new SourceList<int>();
        //var list2 = new SourceList<string>();

        //_list1.Add(1);
        //_list1.Add(2);
        //_list1.Add(3);

        //list2.Add("a");
        //list2.Add("b");

        // Füllen Sie die beiden Listen mit Werten

        //var transformedList1 = _list1.Connect().Transform(x => new Item(x, $"test{x}"));
        //var merged = transformedList1.Merge(list2.Connect()).ObserveOn(RxApp.MainThreadScheduler).Sort(SortExpressionComparer<Item>.Ascending(x => x.Name)).Bind(out _structItems).Subscribe();

        //var transformedList1 = _list1.Connect().TransformMany(x => list2.Items.Select(y => x.ToString() + y));
        //var merged = transformedList1.Sort(SortExpressionComparer<string>.Ascending(x => x)).Bind(out _structItems).Subscribe();

        CommandCloseDialog = ReactiveCommand.Create(CloseDialog);
        CommandOpenDialog = ReactiveCommand.Create(OpenDialog);
        CommandOpenAppBar = ReactiveCommand.Create(OpenAppBar);
        CommandUpdateSampleService = ReactiveCommand.Create(UpdateSampleService);
    }

    private void CloseDialog()
    {
        IsModalOpen = false;
    }

    private void OpenDialog()
    {
        IsModalOpen = true;
    }

    private void OpenAppBar()
    {
        _dialogService.Show<AppBarViewModel>(
            window => AppBarUtils.Register(window),
            (window, e) => AppBarUtils.Unregister(window));
    }

    private void UpdateSampleService()
    {
        _sampleService?.Update();
        //_defaultItems.Clear();
        //var t = _defaultItems.Items.Single(x => x.Id.Equals(""));

        //var x = new SampleItem("", "Xll2");

        //_defaultItems.Remove(t);
        //_defaultItems.Add(new SampleItem("", "All2"));
        //_defaultItems.Add(x);
        //_defaultItems.Remove(x);
        //_list1.Edit(x =>
        //{
        //    x.Clear();
        //    x.Add(2);
        //    x.Add(4);
        //});
    }
}