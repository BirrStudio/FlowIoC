using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalSaveModule.Models;

namespace Modules.LocalSaveModule.Services
{
    public partial interface ILocalSaveService
    {
        public static partial class Commands
        {
            /// <summary>
            /// Puts every persisted asset back to what it shipped with and writes that as the whole
            /// save, so the file reads as a fresh install's and a later save on pause or quit
            /// writes the same values again.
            ///
            /// The assets are reset, the modules that read them are not: a Model that copied its
            /// values out at startup still holds the old ones. Reload the scene after this step, so
            /// every module rebuilds from the reset assets.
            /// </summary>
            public class ResetAll : Command
            {
                [Inject] private ILocalSaveModel _model { get; set; }

                public override void Execute() => _model.ResetAll();
            }
        }
    }
}
