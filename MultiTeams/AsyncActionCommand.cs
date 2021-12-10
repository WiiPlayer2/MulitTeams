namespace MultiTeams
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    class AsyncActionCommand : ICommand
    {
        private readonly Func<object?, Task> execute;

        private readonly Func<object?, bool>? canExecute;

        public AsyncActionCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = default)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => this.canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter) => await this.execute(parameter);

        public event EventHandler? CanExecuteChanged;
    }
}