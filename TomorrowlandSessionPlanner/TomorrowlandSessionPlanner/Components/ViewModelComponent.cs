using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Components;

public abstract class ViewModelComponent<TViewModel> : ComponentBase, IAsyncDisposable where TViewModel : ViewModel
{
    [Inject] protected TViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    protected virtual void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        await ViewModel.DisposeAsync();
    }
}