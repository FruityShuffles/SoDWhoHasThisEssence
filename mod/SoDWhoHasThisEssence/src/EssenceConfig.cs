namespace SoDWhoHasThisEssence
{
    // Shown in the mod manager's config screen (ModBehaviour.modConfigFields picks up ModConfig fields).
    public class EssenceConfig : ModConfig
    {
        [LabelText("Show duplicates between other players")]
        [Description("Also outline essences that two or more teammates share when you don't have them.")]
        public bool showOtherDuplicates = false;
    }
}
