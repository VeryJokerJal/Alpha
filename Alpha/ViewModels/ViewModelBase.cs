using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Windows;
using ReactiveUI;

namespace Alpha.ViewModels
{
    public class ViewModelBase : ReactiveObject, IScheduler
    {
        public DateTimeOffset Now => DateTimeOffset.Now;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return Application.Current.Dispatcher.CheckAccess()
                ? action(this, state)
                : Application.Current.Dispatcher.Invoke(() => action(this, state));
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return new SingleAssignmentDisposable
            {
                Disposable = new Timer(_ => action(this, state), null, dueTime, TimeSpan.FromMilliseconds(-1))
            };
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            TimeSpan delay = dueTime - DateTimeOffset.Now;
            return Schedule(state, delay, action);
        }
    }
}
