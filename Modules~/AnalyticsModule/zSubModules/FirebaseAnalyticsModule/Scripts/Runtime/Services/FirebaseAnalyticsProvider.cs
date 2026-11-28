using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using FlowIoC.ConsoleModule;
using Modules.AnalyticsModule.Data.ValueObjects;
using Modules.AnalyticsModule.Enums;
using Modules.AnalyticsModule.Services;

namespace Modules.AnalyticsModule.FirebaseAnalyticsModule.Services
{
    /// <summary>
    /// Firebase Analytics behind the socket. The bring-up is the SDK's dependency check, answered
    /// on the main thread; every event is cut to Firebase's limits first and the cut is reported,
    /// so a name the SDK would silently reject is fixed by the developer instead. Consent is
    /// Google Consent Mode's four flags, one to one. In the Editor the SDK's desktop stub answers.
    /// </summary>
    public class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        private const int DEPENDENCY_CHECK_ATTEMPTS = 20;
        private const int DEPENDENCY_CHECK_RETRY_MILLISECONDS = 500;

        private readonly AnalyticsEventTrimmer _trimmer = new();
        private readonly AnalyticsLimitsVO _limits = new(40, 40, 100, 25);

        public string Name => "Firebase";

        public void Initialize(Action<bool> ready) => CheckDependencies(ready, 1);

        // Another Firebase SDK in the game (Crashlytics, Messaging) may be running the same check at
        // boot. Firebase does not wait for it: a second, concurrent call comes back faulted with
        // "Don't call other Firebase functions while CheckDependencies is running". A faulted check
        // is therefore asked again after a short wait, a bounded number of times; once the other
        // check is over, the next call answers from Firebase's cached result at once. The queue in
        // the core holds the events meanwhile, and ready is called exactly once, with the outcome.
        // The refusal is not always a faulted task: while the other check runs, the call itself can
        // throw. That throw is turned into a faulted task so the same retry answers it, rather than
        // escaping to the caller as a failed initialize.
        private void CheckDependencies(Action<bool> ready, int attempt)
        {
            Task<DependencyStatus> check;

            try
            {
                check = FirebaseApp.CheckAndFixDependenciesAsync();
            }
            catch (Exception exception)
            {
                check = Task.FromException<DependencyStatus>(exception);
            }

            check.ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully && attempt < DEPENDENCY_CHECK_ATTEMPTS)
                {
                    FlowLogger.Log($"Initialize - the dependency check faulted, attempt {attempt}; asking again: " +
                                   (task.Exception?.GetBaseException().Message ?? "no message"));
                    Task.Delay(DEPENDENCY_CHECK_RETRY_MILLISECONDS)
                        .ContinueWithOnMainThread(_ => CheckDependencies(ready, attempt + 1));
                    return;
                }

                bool available = task.IsCompletedSuccessfully && task.Result == DependencyStatus.Available;

                if (available)
                {
                    _ = FirebaseApp.DefaultInstance;
                    FlowLogger.Log("Initialize - Firebase is available.");
                }
                else
                {
                    string reason = task.IsCompletedSuccessfully
                        ? task.Result.ToString()
                        : task.Exception?.GetBaseException().Message ?? "the dependency check did not complete";
                    FlowLogger.LogError($"Initialize - Firebase is not available after {attempt} attempt(s): {reason}");
                }

                ready(available);
            });
        }

        public void Log(AnalyticsEventVO analyticsEvent)
        {
            AnalyticsEventVO trimmed = _trimmer.Trim(analyticsEvent, _limits, out string report);

            if (report.Length > 0)
                FlowLogger.LogWarning($"Log - '{analyticsEvent.Name}' cut for Firebase: {report}");

            if (trimmed.Parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(trimmed.Name);
                return;
            }

            var parameters = new Parameter[trimmed.Parameters.Count];

            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = ToParameter(trimmed.Parameters[i]);

            FirebaseAnalytics.LogEvent(trimmed.Name, parameters);
        }

        public void SetUserProperty(string name, string value) => FirebaseAnalytics.SetUserProperty(name, value);

        public void SetUserId(string userId) => FirebaseAnalytics.SetUserId(userId);

        public void SetConsent(AnalyticsConsentVO consent) =>
            FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, ConsentStatus>
            {
                {ConsentType.AnalyticsStorage, Status(consent.AnalyticsStorage)},
                {ConsentType.AdStorage, Status(consent.AdStorage)},
                {ConsentType.AdUserData, Status(consent.AdUserData)},
                {ConsentType.AdPersonalization, Status(consent.AdPersonalization)}
            });

        private Parameter ToParameter(AnalyticsParameterVO parameter) => parameter.Kind switch
        {
            AnalyticsParameterKind.Long => new Parameter(parameter.Name, parameter.LongValue),
            AnalyticsParameterKind.Double => new Parameter(parameter.Name, parameter.DoubleValue),
            AnalyticsParameterKind.Bool => new Parameter(parameter.Name, parameter.BoolValue ? 1L : 0L),
            _ => new Parameter(parameter.Name, parameter.StringValue ?? string.Empty)
        };

        private ConsentStatus Status(bool granted) => granted ? ConsentStatus.Granted : ConsentStatus.Denied;
    }
}
