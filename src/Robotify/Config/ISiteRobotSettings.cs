using System.Collections.Generic;

namespace Robotify.Config
{
	public interface ISiteRobotSettings
	{
		bool Enabled { get; }

		IEnumerable<string> DisallowPaths { get; }

		IEnumerable<string> AllowPaths { get; }

		IEnumerable<string> UserAgents { get; }

		int? CrawlDelay { get; }

        string SitemapUrl { get; }
	}
}