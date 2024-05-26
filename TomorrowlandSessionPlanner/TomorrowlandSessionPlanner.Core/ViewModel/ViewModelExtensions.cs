using CommunityToolkit.Mvvm.Input;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public static class ViewModelExtensions
{
    public static Task ExecuteAsync(this IAsyncRelayCommand relayCommand)
    {
        return relayCommand.ExecuteAsync(null);
    }
}