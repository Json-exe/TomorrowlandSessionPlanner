using CommunityToolkit.Mvvm.ComponentModel;

namespace TomorrowlandSessionPlanner.Core.Model;

public abstract class ViewModel : ObservableObject, IAsyncDisposable
{
    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}