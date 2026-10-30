namespace Modules.LocalizationModule.Models
{
    public class LocalizationModel : ILocalizationModel
    {
        public bool IsReady { get; private set; }

        public void SetReady() => IsReady = true;
    }
}
