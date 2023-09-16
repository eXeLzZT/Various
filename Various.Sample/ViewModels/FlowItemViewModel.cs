using ReactiveUI;

namespace Various.Sample.ViewModels;

public class FlowItemViewModel : ReactiveObject
{
    public string Name { get; set; }
    public string Info { get; set; }
}