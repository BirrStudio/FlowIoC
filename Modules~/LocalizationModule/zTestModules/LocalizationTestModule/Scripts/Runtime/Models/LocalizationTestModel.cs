#if UNITY_EDITOR

namespace Modules.LocalizationModule.LocalizationTestModule.Models
{
    /// <summary>The number the sample's variable line counts, so a plural form can be seen to change.</summary>
    internal class LocalizationTestModel
    {
        public int Levels { get; private set; } = 1;

        public void AddLevel() => Levels++;
    }
}

#endif
