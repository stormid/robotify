using System.Configuration;

namespace Robotify.Config
{
    public static class SiteRobotSettings
    {
        private static ISiteRobotSettings _current;

        public static ISiteRobotSettings Current
        {
            get
            {
                if (_current == null)
                    throw new ConfigurationErrorsException(
                        "Robot site settings have not been configured, please ensure Set() is called before accessing this configuration");
                return _current;
            }
        }

        public static bool IsConfigured => _current != null;

        public static void Set(ISiteRobotSettings settings)
        {
            _current = settings;
        }
    }
}