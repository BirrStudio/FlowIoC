using System.Threading.Tasks;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.LoadingModule.Data.ValueObjects;
using Modules.LoadingModule.Models;
using Modules.LoadingModule.Shared.Enums;
using Modules.LoadingModule.Signals;
using UnityEngine;

namespace Modules.LoadingModule.Services
{
    /// <summary>
    /// The module's public surface. It validates what it is asked and hands the work to the
    /// Commands through the internal signals, so every change to a set is a step the Flow Console
    /// can show. Every entry point hides itself from the call stack: an error raised here opens
    /// the Command that reported, not this guard.
    /// </summary>
    public class LoadingService : ILoadingService
    {
        [Inject] private ILoadingModel _model { get; set; }
        [InjectSignal] private LoadingInternalSignals _internalSignals { get; set; }

        private bool _tornDownReported;

        [HideInCallstack]
        public void Begin(string set)
        {
            if (IsTornDown($"Begin('{set}')")) return;

            if (!_model.TryGetSet(set, out _))
            {
                FlowLogger.LogError($"Begin('{set}') - CD_LoadingSets declares no set named '{set}'.");
                return;
            }

            _internalSignals.BeginSet.Dispatch(set);
        }

        [HideInCallstack]
        public ILoadingStep Report(string step) => new LoadingStep(this, step);

        [HideInCallstack]
        public Task<bool> Await(string set)
        {
            if (IsTornDown($"Await('{set}')")) return Task.FromResult(false);

            if (!_model.TryGetSet(set, out LoadingSetRVO runtime))
            {
                FlowLogger.LogError($"Await('{set}') - CD_LoadingSets declares no set named '{set}'.");
                return Task.FromResult(false);
            }

            switch (runtime.State)
            {
                case LoadingSetState.Completed: return Task.FromResult(true);
                case LoadingSetState.Failed: return Task.FromResult(false);
                default: return _model.Awaiter(runtime).Task;
            }
        }

        [HideInCallstack]
        internal void Submit(LoadingStepReportVO report)
        {
            if (IsTornDown($"Report('{report.Step}')")) return;

            if (!_model.TryGetStep(report.Step, out _))
            {
                FlowLogger.LogError($"Report('{report.Step}') - CD_LoadingSets lists no step named '{report.Step}' in any set.");
                return;
            }

            report.Time = Time.realtimeSinceStartup;
            _internalSignals.StepReported.Dispatch(report);
        }

        /// <summary>
        /// The module's context is gone, but a step handle held by work that outlived its own module -
        /// a coroutine on the provider that survives every scene - still calls in. Said once and then
        /// ignored: the config is not what is wrong, and a line per report buries everything else.
        /// </summary>
        [HideInCallstack]
        private bool IsTornDown(string call)
        {
            if (!_model.IsDeconstructed) return false;
            if (_tornDownReported) return true;

            _tornDownReported = true;
            FlowLogger.LogWarning($"{call} - the Loading module has been torn down, so this call and every later one is ignored. The work making it outlived its module.");
            return true;
        }
    }
}
