namespace MockApi.Localization
{
    public class LocalizationSource
    {
        public string RelativePathToFolder { get; private set; }

        public LocalizationSource(string relativePathToFolder)
        {
            RelativePathToFolder = relativePathToFolder;
        }
    }
}
