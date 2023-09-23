using ReactiveUI;

namespace Various.Wpf.Controls;

public interface IUseReactiveModal
{
    ReactiveObject ModalContent { get; set; }
    bool IsModalOpen { get; set; }
}
