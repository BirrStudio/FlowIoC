#if UNITY_EDITOR

using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalizationModule.LocalizationTestModule.Models;

namespace Modules.LocalizationModule.LocalizationTestModule.Controllers
{
    internal class AddLevelCommand : Command
    {
        [Inject] private LocalizationTestModel _model { get; set; }

        public override void Execute() => _model.AddLevel();
    }
}

#endif
