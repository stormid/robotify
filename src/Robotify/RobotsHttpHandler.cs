using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Robotify.Config;

namespace Robotify
{
    public class RobotsHttpHandler : IHttpHandler
	{
		private static string _robotsContent;

		public void ProcessRequest(HttpContext context)
		{
            if (!SiteRobotSettings.IsConfigured)
            {
                SiteRobotSettings.Set(new FromConfigSiteRobotSettings());
            }

		    if (RobotsTxtFileExists(context))
            {
                Trace.TraceInformation("[Robotify] Sending physical robots.txt file");
                context.Response.ContentType = "text/plain";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.TransmitFile("robots.txt");
            }
            else
		    {
		        if (RobotifyIsConfigured())
		        {
                    Trace.TraceInformation("[Robotify] Sending dynamic robots.txt file");
                    context.Response.Buffer = true;
		            context.Response.BufferOutput = true;
		            context.Response.ContentType = "text/plain";
		            context.Response.ContentEncoding = Encoding.UTF8;
		            context.Response.Write(GetRobotsContent(SiteRobotSettings.Current, context));
		        }
		        else
		        {
                    context.Response.StatusCode = 404;
                }
            }
		}

        private static bool RobotsTxtFileExists(HttpContext context)
        {
            return File.Exists(context.Server.MapPath("~/robots.txt"));
        }

        private bool RobotifyIsConfigured()
        {
            return SiteRobotSettings.Current != null && SiteRobotSettings.Current.Enabled;
        }

        private static string GetRobotsContent(ISiteRobotSettings settings, HttpContext context)
		{
			if (string.IsNullOrWhiteSpace(_robotsContent))
			{
				_robotsContent = MakeRobots(settings, context);
			}
			return _robotsContent;
		}

		private static string MakeRobots(ISiteRobotSettings settings, HttpContext context)
		{
			var wtr = new StringBuilder();

			if (settings.UserAgents != null && settings.UserAgents.Any())
			{
			    foreach (var agent in settings.UserAgents.Where(agent => !string.IsNullOrWhiteSpace(agent)))
			    {
			        wtr.AppendFormat("User-agent: {0}\n", agent);
			    }
			}
			else
			{
				wtr.AppendLine("User-agent: *");
			}

			if (settings.DisallowPaths != null && settings.DisallowPaths.Any())
			{
				foreach(var path in settings.DisallowPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
			    {
			        wtr.AppendFormat("Disallow: {0}\n", path);
			    }
			}

			if (settings.AllowPaths != null && settings.AllowPaths.Any())
			{
			    foreach (var path in settings.AllowPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
			    {
			        wtr.AppendFormat("Allow: {0}\n", path);
			    }
			}

			if (settings.CrawlDelay.HasValue)
			{
				wtr.AppendFormat("Crawl-delay: {0}\n", settings.CrawlDelay);
			}

            if (!string.IsNullOrWhiteSpace(settings.SitemapUrl))
            {
                string sitemapUri = null;
                if (Regex.IsMatch(settings.SitemapUrl, "^http(?:s|)://.*", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    sitemapUri = settings.SitemapUrl;
                }
                else
                {
                    var ub = new UriBuilder(context.Request.Url) {Path = settings.SitemapUrl};
                    sitemapUri = ub.ToString();
                }
                if(!string.IsNullOrWhiteSpace(sitemapUri)) wtr.AppendFormat("Sitemap: {0}\n", sitemapUri);
            }
            return wtr.ToString();
		}
		
		public bool IsReusable => true;
	}
}
