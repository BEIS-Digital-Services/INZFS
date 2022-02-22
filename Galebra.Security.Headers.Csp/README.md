# Content Security Policy in ASP.NET 6

This library allows you to configure Content Security Policy headers in ASP.NET 6 for Razor Pages and MVC.

The purpose of this Readme is to focus on the pragmatic aspect of implementing CSP and outline why this library has been
built as such and not otherwise.

* [Get Started](#get-started)
* [Design Philosophy and introduction](#design-philosophy-and-introduction)
* [The CspPolicyGroup class](#the-csppolicygroup-class)
* [The CspPolicy class and Nonce TagHelper](#the-csppolicy-class-and-nonce-taghelper)
* [Multiple Policies, Attributes, Filters and default CspPolicyGroup](#multiple-policies-attributes-filters-and-default-csppolicygroup)
* [IsDisabled global boolean](#isdisabled-global-boolean)
* [Dependency Injection](#dependency-injection)
* [Browser Link and Hot Reload](#browser-link-and-hot-reload)
* [Debug and DisplayTagHelper](#debug-and-displaytaghelper)
* [Additional Resources](#additional-resources)

## Get Started

All terminology is explained in the next sections. You may check the MVC and Razor Pages sample and navigate through the pages.

The library does not use `Endpoint` routing, so you can invoke the `UseContentSecurityPolicy`
middleware before or after `UseRouting`.

````csharp
app.UseStaticFiles();

app.UseContentSecurityPolicy();

app.UseRouting();
````

It can be placed before `UseStaticFiles` if you need CSP headers to be delivered with peculiar content such as `SVG`.


You configure CSP via *appsettings.json* or via an `Action` in *Program.cs*.

When using *appsetting.json*:

````csharp
using Galebra.Security.Headers.Csp;

builder.Services.AddContentSecurityPolicy(builder.Configuration.GetSection("Csp"));
````

In the following, three policy groups are registered:

````json
  "Csp": {
    "IsDisabled": false,//default: Will apply the default policy everywhere until overriden by attributes or filters
    "PolicyGroups": {
      "PolicyGroup1": {
        "Csp": {
          "Fixed": "default-src 'none' 'sha256-RFWPLDbv2BY+rCkDzsE+0fr8ylGr2R2faWMhq4lfEQc=';script-src 'self'"
        },
        "IsDefault": false,
        "NumberOfNonceBytes": 16//default
      },
      "PolicyGroup2": {
        "Csp": {
          "Fixed": "default-src 'self';base-uri 'self';form-action 'self';object-src;frame-ancestors;connect-src ws://localhost:65412",
          "Nonceable": [
            "style-src 'self'"
          ]
        },
        "CspReportOnly": {
          "Fixed": "default-src;form-action 'self';base-uri;object-src;frame-ancestors;sandbox",
          "Nonceable": [
            "style-src",
            "script-src"
          ]
        },
        "IsDefault": true,//default
        "NumberOfNonceBytes": 8
      },
      "PolicyGroup3": {
        "Csp": {
          "Nonceable": [
            "style-src"
          ]
        },
        "IsDefault": false,
        "NumberOfNonceBytes": 3
      }
    }
  },
````

The first policy group does not require nonces and enforces CSP.
The second policy group configures the two headers, CSP and CSP-Report-Only, and requires nonces for each of these headers.

This policy is the default policy. Beware that, by default, `IsDefault` is set to `true` and the library will throw during service
registration if the number of default policies is not one.

The `IsDisabled` property in *Line 1* is set to `false` (default),
which means that the default policy named *PolicyGroup2* will be applied globally unless overridden by attributes or filters.
The third policy uses only nonces, for styles. The default value for nonce generation is 16 bytes.
We used `connect-src ws://localhost:65412` in this example to allow `/_framework/aspnetcore-browser-refresh.js` to work properly.
Also, we disabled CSS Hot Reload in Visual Studio, see https://github.com/dotnet/aspnetcore/issues/36085, to avoid
a weak CSP configuration just for development. It is not clear how this port is generated, apparently randomly.
These policies will also disable Visual Studio tracking features when they occur.

Alternatively, you can configure everything in code:

````csharp
builder.Services.AddContentSecurityPolicy(c =>
{
    c.IsDisabled = false;
    c.Add("Policy1", g =>
    {
        g.Csp.Fixed = "default-src 'self';connect-src ws://localhost:65412";
        g.Csp.Nonceable.Add("style-src 'self'");
        g.CspReportOnly.Nonceable.Add("script-src");
        g.IsDefault = true;
        g.NumberOfNonceBytes = 32;
    });
    c.Add("Policy2", g =>
    {
        g.Csp.Fixed = "default-src 'self';connect-src ws://localhost:65412";
        g.CspReportOnly.Fixed="default-src";
        g.IsDefault = false;
    });
});
````

Add nonces to the body by importing the TagHelpers

````cshtml
@addTagHelper *, Galebra.Security.Headers.Csp
````

And add to styles, scripts or link tags the TagHelper:

````cshtml
nonce-add=true
````

For example, for `PolicyGroup3`, that restricts you to use only nonced styles,
you would allow loading bootstrap like so:

````cshtml
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" nonce-add=true/>
````

A sample project is provided and exposes the usage of the library.

![Display CSP policy groups](../../samples/_static/displaypolicygroups.png)

## Design Philosophy and introduction

The following design principles, detailed below, have been followed:

* CSP configuration is well suited in *appsettings.json* because you need to store long strings
* Configuration via `Action` should also be possible in *Program.cs*
* Support multiple policies to use in different parts of the website
* Enable and Disable attributes in pages or actions or controllers
* Enable and Disable Filters for folders in razor pages
* Developper provides a default, global policy, and can override
* Possibility to apply the default policy everywhere, and override for parts of the website,
or have no policy applied globally and set them on specific parts only
* Support CSP and a CSP-Report-Only headers simultaneously, each with a different set of policy directives
* Optimize nonce generation for `script`, `style`, and `link` tags
* Use the latest recommendations for [random number generations](https://github.com/dotnet/runtime/issues/40579)
* Optimize string generation for common CSP string lengths
* Add CSP to headers by using the key already existing in `HttpContext` to avoid memory allocation
* Each policy is, at the end, a long string.
This string can be either static (fixed) or generated newly on each request if nonces are required.

Consider the following arguments. CSP is a set of two headers, whose names can be:

* Content-Security-Policy
* Content-Security-Policy-Report-Only

Each policy in those headers is defined by an ordered set of directives.
Each directive is a name/value pair; name is non-empty, but value may be empty.
For example, `script-src` is a directive's *name* and the value can be, for instance, `'self'` or a url or a set of these.
Another directive can be `default-src 'none'`, where `default-src` is another directive name.
The directives forming a policy are separated by semicolons.
For this example, the Content Security Policy would be the header:

````
Content-Security-Policy: default-src 'none';script-src 'self'
````

which is equivalent to the shorter version

````
Content-Security-Policy: default-src;script-src 'self'
````

In theory, you can have [multiple CSP policies](https://w3c.github.io/webappsec-csp/#multiple-policies) **for the same header name**
in the same response; however, we believe **this should not be recommended** because it makes your policy more convoluted
and rather highlights a design flaw, notwithstanding that you send more bytes to the user's browser. Consequently, the library
outputs only one CSP header for a given header name.
In practice, we assign the value for the header key (name) rather than adding a new item in the dictionary.
This also has the consequence of efficient write.

So, with this library, **you cannot have** the following:

````
Content-Security-Policy: Policy1
Content-Security-Policy: Policy2
````

## The CspPolicyGroup class

However, you can have both a CSP and CSP-Report-Only policy, and usually **this is recommended**.
Some libraries do not support this configuration. This library does. For example **you can do** something like this:

````
Content-Security-Policy: default-src 'self'
Content-Security-Policy-Report-Only: default-src;style-src 'self'
````

In this scenario, the website would run without issues if, loosely speaking, all styles and scripts are loaded from the server
(`'self'`), but the browser will report to you what would break if you disable scripts ('none') and other fetch directives,
except for styles which use `'self'`.
Thus, this allows you to enforce a policy and at the same time fine-tune another one which ultimately will become
the enforcing policy when you are ready.

Allowing for both CSP and CSP-Report-Only headers to coexist introduces the `CspPolicyGroup` class.
It is this group class that you use to configure your policies. This class contains two properties, `Csp` and `CspReportOnly`, each of
which is a `CspPolicy` class. You can leave one of these properties empty; for example if you want to have only browser reports,
you would build only the property `CspReportOnly`.

## The CspPolicy class and Nonce TagHelper

The usual route to library design is to use the so-called fluent-api. This gives elegant code,
but with CSP this is unnecessary complication and makes your *Program.cs* (or *Startup.cs*) rather long.
In addition, CSP configuration ultimately boils down to outputting one or two strings in the headers,
plus the possibility of nonces in those headers and body.
A developer will ultimately look at the CSP output in the browser's tools
to see if the formatting is as expected and nonces properly generated and positioned.
There is already a lot of resources and services in the pipeline, so we decided to keep it simple and focus on performance.

This led us to the following observation. A CSP header value is **always** divided into two groups.
The first group is a fixed, possibly empty, string, i.e. a string that *does not change upon requests on a given page*,
and another string that is dynamically generated upon each request to include one or more nonces.
Therefore the `CspPolicy` class has two public properties, a string `Fixed` and a list of strings called `Nonceable`.

For example, the policy:

````
default-src;style-src 'self'
````

would be set as a `CspPolicy.Fixed` string because a nonce is not required. In *appsettings.json*, this would be:

````json
"Fixed": "default-src;style-src 'self'",
````

A `sha256-myshacode`, if needed, would be included in the `Fixed` string.

If, now, you want to have this policy, but in addition produce a nonce for styles, then you would need to split your string
and populate the `CspPolicy.Nonceable` list. In *appsettings.json*, the split would be like this:

````json
"Fixed": "default-src",
"Nonceable": [
"style-src 'self'"
]
````

The `style-src` is here short, but generally is longer to include for example urls from a Bootstrap CDN. The most important is to
identify the directive's name, here `style-src`.

To use the nonce on the style in this example, you would invoke the Tag Helper `nonce-add`, e.g.

````cshtml
<style nonce-add=true>
.myclass{
    background:lightgreen;
}
</style>
<h4 class="myclass">I am lightgreen, thank you nonce!</h4>
````

A nonce will be automatically generated in the response header when `Nonceable` is not an empty list,
and in the body as soon as you invoke the Tag Helper.
To view the nonces, view the page source as browsers hide the nonce when you use *inspect element*. 

In `_ViewImports.cshtml`, you will need to add

````cshtml
@addTagHelper *, Galebra.Security.Headers.Csp
````

When you use a `nonce`, common libraries are confined to the `script` and `style` tags, but `link` tags are also possible.
Nonce generation defaults to the spec-recommended 16 bytes, which gives 256^16=2^128 possibilities,
or a 22 long web-encoded base64 string. You can override this with the property `CspPolicyGroup.NumberOfNonceBytes`,
e.g. in *appsettings.json*, and the nonce will apply to the entire group.

````json
"NumberOfNonceBytes": 8
````

## Multiple Policies, Attributes, Filters and default CspPolicyGroup

When you implement CSP on a website, often you need several `CspPolicyGroup` objects depending on the page where the user lands.
For example, you would have a global CSP policy on all pages, but when processing a payment on a page or Razor Pages folder,
or a Controller, you will want another CSP policy (for example to accept connections to a payment API such as PayPal or Stripe).
This library allows you to configure many policies and invoke them when needed, through attributes and filters.

When you use the library, unless you override the default described below, the default `CspPolicyGroup`
is applied to all pages as soon as you inject the Middleware in the pipeline.

````csharp
app.UseStaticFiles();

app.UseContentSecurityPolicy();
````

If you want your CSP header to be applied also on static files in `wwwroot`, then call the middleware before

````csharp
app.UseContentSecurityPolicy();

app.UseStaticFiles();
````

The default `CspPolicyGroup` that you define with `CspPolicyGroup.IsDefault=true` can be overriden globally, e.g. for Razor Pages:

````csharp
//Apply globally
builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new EnableCspPageFilter { PolicyGroupName = "MyNonDefaultPolicyGroup" }));
    });
````

However, this makes little sense since you could just as well define it as the default policy.

More interestingly, you can apply a given policy to an entire Folder in Razor Pages, e.g. in the *Movies* folder:

````csharp
//Apply on specific folders
options.Conventions.AddFolderApplicationModelConvention(
    "/Movies",
    model => model.Filters.Add(new EnableCspPageFilter { PolicyGroupName = "PolicyGroup1" }));
````

And carry on with another policy group for subfolders

````csharp
options.Conventions.AddFolderApplicationModelConvention(
    "/Movies/Adventure",
    model => model.Filters.Add(new EnableCspPageFilter { PolicyGroupName = "PolicyGroup3" }));
````

You can also disable CSP in a folder:

````chsarp
 model => model.Filters.Add(new DisableCspPageFilter { EnforceMode = false }));
````

Where `EnforceMode` is `true` by default and is discussed below.

In areas, you could do something like this:

````csharp
//Could be used in, e.g., ASP.NET Identity
options.Conventions.AddAreaFolderApplicationModelConvention("Identity", "/Account",
    model => model.Filters.Add(new EnableCspPageFilter { PolicyGroupName = "PolicyGroup1" }));

options.Conventions.AddAreaPageApplicationModelConvention("Identity", "/Account/Manage/ChangePassword",
    model => model.Filters.Add(new EnableCspPageFilter { PolicyGroupName = "PolicyGroup3" }));
````

In a Razor Page or Action or Controller, you can override the default `CspPolicyGroup` with an attribute:

````csharp
[EnableCsp(PolicyGroupName="PolicyGroup1")]
````

````csharp
[DisableCsp]
````

You can also use the attribute in a Razor Page, e.g.

````cshtml
@page
@using Galebra.Security.Headers.Csp
@model IndexModel
@attribute [EnableCsp("PolicyGroup1")]
````

or

````cshtml
@page
@using Galebra.Security.Headers.Csp
@model IndexModel
@attribute [DisableCsp]
````

The `[DisableCsp]` always wins unless you set the `init` property `DisableCsp.EnforceMode` to false.
For example, on a page, with

````csharp
[DisableCsp]
[EnableCsp(PolicyGroupName ="PolicyGroup3")]
````

CSP will be disabled regardless of the (here useless) presence of the enable attribute, whereas if you set

````csharp
[DisableCsp(EnforceMode = false)]
[EnableCsp(PolicyGroupName = "PolicyGroup3")]
````

the *PolicyGroup3* will be applied.
If the enable attribute was absent and if there is no other enable filters for this route,
`EnforceMode` will have no effect, and CSP will be disabled.

The enforcement rule is useful in a scenario where an entire folder or a controller needs to have CSP disabled on all routes
except for those where the enable attribute is present. For example, in the following we use the `DisableCspPageFilter`
to disable CSP in the *Movies* folder:

````csharp
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention(
        "/Movies",
        model => model.Filters.Add(new DisableCspPageFilter { EnforceMode = false }));
});
````
Because we have set `EnforceMode = false`, we can set CSP on a page inside the *Movies* folder with an attribute,
e.g. in *Movies/index* `[EnableCsp(PolicyGroupName = "PolicyGroup3")]`. The same applies for a controller; you would disable CSP
with `EnforceMode=false` and use the `EnableCsp` attribute on an action:

````csharp
[DisableCsp(EnforceMode = false)]
public class BooksController : Controller
{
    public ActionResult Index()
    {
        //CSP is disabled here
        return View();
    }

    // GET: BooksController/Details/5
    [EnableCsp(PolicyGroupName ="PolicyGroup3")]
    public ActionResult Details(int id)
    {
        //CSP works here owing to EnforceMode=false
        return View();
    }
}
````

## IsDisabled global boolean

By default, the library applies the default `CspPolicyGroup` to all delivered pages until overwritten by attributes or filters.
You can override this global behaviour in *Program.cs* or in *appsettings.json* where you can set at the top level
`"IsDisabled": true,`. This will result in CSP not being applied globally, at all, until you invoke it via attributes or filters.

## Dependency Injection

Even though you should not need it in production, you can inject the `ICspOptions` that have been configured as a Singleton, e.g. on a Page:

````csharp
private readonly ICspOptions _cspOptions;

public IndexModel(ICspOptions cspOptions)
{
    _cspOptions = cspOptions;
}
````

or in a Page View

````cshtml
@using Galebra.Security.Headers.Csp
@inject ICspOptions CspOptions
````

Similarly, you can inject the `ICspNonce`, configured as a Scoped service, with the using
`@using Galebra.Security.Headers.Csp.Infrastructure`.
Run and check out the index page of the sample project.

## Browser Link and Hot Reload

Check your browser's dev tools and check which ports are used for connections,
see https://github.com/dotnet/aspnetcore/issues/36085.
Use this to configure your CSP such that you allow Visual Studio to use Hot reloads and browser link. See example below.

## Debug and DisplayTagHelper

The library will throw at build time when you misconfigure your policies but does only some basic checks and string parsing.
This check is limited on purpose because your eyes are better at parsing such kind of strings. In addition,
we found that the browser tools are good at complaining about misconfigurations in your directives
and we do not expect a developer working on CSP to go to production without
paying care to output headers.

For debugging, you can use the toy `DisplayCspGroupTagHelper` to display the name of the `CspPolicyGroup`
that has been used in the response. If the disabled attribute or filter is applied, then a disabled or disabled global strings
will be displayed instead.

````cshtml
<display-csp-group/>
````

## Additional Resources

* [CSP, W3C](https://w3c.github.io/webappsec-csp)
* [Scott Helme Content Security Policy - An Introduction](https://scotthelme.co.uk/content-security-policy-an-introduction/)
* [Github's CSP journey](https://github.blog/2016-04-12-githubs-csp-journey/)
* [Github's post-CSP journey](https://github.blog/2017-01-19-githubs-post-csp-journey/)
* [content-security-policy.com](https://content-security-policy.com/)
* [Security Headers](https://securityheaders.com/)
* [Csp with Google](https://csp.withgoogle.com/docs/index.html)
* [Dangers of white listing in CSP (Google)](https://storage.googleapis.com/pub-tools-public-publication-data/pdf/45542.pdf)
* [Csp Hash Calculator](https://strict-csp-codelab.glitch.me/csp_sha256_util.html)
* [Base64 encode calculator](http://string-functions.com/base64encode.aspx)
* [Content Security Policy (Google)](https://developers.google.com/web/fundamentals/security/csp/)
* [CSP evaluator (Google)](https://csp-evaluator.withgoogle.com/)
* [Csplite](https://csplite.com/csp/test187/)
* [cspisawesome.com](https://www.cspisawesome.com/)
* [Report Uri](https://report-uri.com/home/generate)
* [CspScanner](https://cspscanner.com/)
* [Postcards from the post-XSS world](https://lcamtuf.coredump.cx/postxss/)
* [Content-Security-Policy-CSP-Bypass-Techniques by bhaveshk90](https://github.com/bhaveshk90/Content-Security-Policy-CSP-Bypass-Techniques)
* [Video: A successful mess between hardening and mitigation - Spagnuolo/Weichselbaum](https://www.youtube.com/watch?v=_L06HetskC4&t=902s)
* [AppSecEU 16 - Michele Spagnuolo, Lukas Weichselbaum - Making CSP great again](https://www.youtube.com/watch?v=uf12a-0AluI)
* [Video: Let's break stuff Matt Brunt](https://www.youtube.com/watch?v=mr230uotw-Y)
* [Video Troy Hunt: Understanding CSP](https://www.troyhunt.com/understanding-csp-the-video-tutorial-edition/)
* [When is content security policy (CSP) not appropriate?](https://security.stackexchange.com/questions/249237/when-is-content-security-policy-csp-not-appropriate)
* [On what types of web content is a Content-Security-Policy useful?](https://security.stackexchange.com/questions/249907/on-what-types-of-web-content-is-a-content-security-policy-useful)

* [NWebsec](https://docs.nwebsec.com/en/latest/nwebsec/Configuring-csp.html)
* [Andrew Lock library](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders)
* [Joonas W library](https://joonasw.net/view/csp-in-aspnet-core)
