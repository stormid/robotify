using System;
using System.Collections.Generic;
using System.Linq;

namespace Robotify.Config
{
    public class FromConfigSiteRobotSettings : ISiteRobotSettings
    {
        private const string Prefix = "Robotify:";
        public bool Enabled { get; } = nameof(Enabled).FromAppSettingsWithPrefix(Prefix, true);
        public IEnumerable<string> DisallowPaths { get; } = ParseFromString(nameof(DisallowPaths).FromAppSettingsWithPrefix(Prefix, "/"));
        public IEnumerable<string> AllowPaths { get; } = ParseFromString(nameof(AllowPaths).FromAppSettingsWithPrefix(Prefix, ""));
        public IEnumerable<string> UserAgents { get; } = ParseFromString(nameof(UserAgents).FromAppSettingsWithPrefix(Prefix, "*"));
        public int? CrawlDelay { get; } = nameof(CrawlDelay).FromAppSettingsWithPrefix<int?>(Prefix, null);
        public string SitemapUrl { get; } = nameof(SitemapUrl).FromAppSettingsWithPrefix<string>(Prefix, null);

        private static IEnumerable<string> ParseFromString(string input, string delimiter = ";")
        {
            return string.IsNullOrWhiteSpace(input) ? Enumerable.Empty<string>() : input.Split(new[] {delimiter}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}