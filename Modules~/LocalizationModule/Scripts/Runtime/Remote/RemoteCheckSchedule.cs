using System;
using Modules.LocalizationModule.Enums;

namespace Modules.LocalizationModule.Remote
{
    /// <summary>
    /// Whether a launch looks at the sheet. A failed look counts as one, so a player offline is
    /// not retried on every launch of the day; a last look in the future - a clock set back - is
    /// treated as none.
    /// </summary>
    internal class RemoteCheckSchedule
    {
        public bool IsDue(RemoteFrequency frequency, long? lastCheckUtcTicks, long nowUtcTicks)
        {
            switch (frequency)
            {
                case RemoteFrequency.Always:
                    return true;

                case RemoteFrequency.Daily:
                    return HasPassed(TimeSpan.FromDays(1), lastCheckUtcTicks, nowUtcTicks);

                case RemoteFrequency.Weekly:
                    return HasPassed(TimeSpan.FromDays(7), lastCheckUtcTicks, nowUtcTicks);

                default:
                    return false;
            }
        }

        private bool HasPassed(TimeSpan span, long? last, long now) =>
            last == null || last.Value > now || now - last.Value >= span.Ticks;
    }
}
