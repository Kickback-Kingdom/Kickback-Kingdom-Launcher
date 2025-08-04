using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Helpers
{
    public static class ReactiveCommandExtensions
    {
        public static T WithGlobalErrorHandling<T>(this T command) where T : IReactiveCommand
        {
            command.ThrownExceptions.Subscribe(ex =>
            {
                Console.Error.WriteLine($"[ReactiveCommand Error] {ex}");
                _ = App.ShowErrorFromExceptionAsync(ex);
            });
            return command;
        }
    }
}
