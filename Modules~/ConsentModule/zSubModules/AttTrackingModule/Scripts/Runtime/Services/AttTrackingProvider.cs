using System;
using System.Threading;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.Shared.Enums;
using Unity.Advertisement.IosSupport;
using UnityEngine;
using AttStatus = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus;

namespace Modules.ConsentModule.AttTrackingModule.Services
{
    /// <summary>
    /// Apple's App Tracking Transparency behind ITrackingProvider. On an iPhone the prompt is shown
    /// once - a status already decided is answered without it - and the answer is returned on the
    /// main thread it was asked from. Anywhere else there is no prompt: NotApplicable.
    /// </summary>
    public class AttTrackingProvider : ITrackingProvider
    {
        public string Name => "ATT";

        public void RequestTracking(Action<TrackingStatus> done)
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                done(TrackingStatus.NotApplicable);
                return;
            }

            AttStatus current = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            if (current != AttStatus.NOT_DETERMINED)
            {
                done(Map(current));
                return;
            }

            SynchronizationContext main = SynchronizationContext.Current;

            ATTrackingStatusBinding.RequestAuthorizationTracking(status =>
            {
                TrackingStatus answer = Map((AttStatus) status);

                if (main == null || SynchronizationContext.Current == main)
                    done(answer);
                else
                    main.Post(_ => done(answer), null);
            });
        }

        private static TrackingStatus Map(AttStatus status)
        {
            switch (status)
            {
                case AttStatus.AUTHORIZED:
                    return TrackingStatus.Authorized;
                case AttStatus.DENIED:
                    return TrackingStatus.Denied;
                case AttStatus.RESTRICTED:
                    return TrackingStatus.Restricted;
                default:
                    return TrackingStatus.NotDetermined;
            }
        }
    }
}
