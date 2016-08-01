# ![Robotify](https://raw.githubusercontent.com/stormid/robotify/master/docs/img/robot.png) Robotify

Robotify - robots.txt handler for ASP.NET (Webforms and MVC)

## Installation

Install package via NuGet:

```
Install-Package Robotify
```

## Configuration

Once installed your web.config will be updated to include a http handler that will respond to all GET requests for ```robots.txt```.  By default if you already have a ```robots.txt``` file in the root of your website this will be used instead of generating one.

To configure the dynamic ```robots.txt``` handler you can set options via the ```<appSettings />``` configuration section, below is the default configuration used when values are missing:

```xml
  <appSettings>
    <add key="Robotify:Enabled" value="true" />
    <add key="Robotify:AllowPaths" value="" /> <!-- multiple values are separated with ; "/catalog/;/products" -->
    <add key="Robotify:DisallowPaths" value="/" /> <!-- multiple values are separated with ; "/admin/;/login/;/secure.aspx" -->
    <add key="Robotify:UserAgent" value="*" /> <!-- multiple values are separated with ; "googlebot;bingbot;anotherbot" -->
    <add key="Robotify:CrawlDelay" value="" /> <!-- only rendered if value supplied -->
    <add key="Robotify:SitemapUrl" value="" /> <!-- only rendered if url supplied -->
  </appSettings>
```

The default configuration will produce a ```robots.txt``` that denies all robots.

```txt
User-agent: *
Disallow: /
```

If you are installing Robotify into a MVC application, remember to update your route config to ignore the robots.txt path:

```c#
  public static void RegisterRoutes(RouteCollection routes)
  {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.IgnoreRoute("robots.txt");

      routes.MapRoute(
          name: "Default",
          url: "{controller}/{action}/{id}",
          defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
      );
  }
```