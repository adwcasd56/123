#define DEFAULT  // DEFAULT RBAC LDAP HTS IMP
#if NEVER
#elif DEFAULT
// <snippet1>
// <snippet11>
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();

var app = builder.Build();
// </snippet11>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
// </snippet1>
#elif RBAC
// <snippet_rbac>
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate(options =>
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            options.EnableLdap("contoso.com");
        }
    });
// </snippet_rbac>

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
#elif LDAP
// <snippet_ldap>
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate(options =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.EnableLdap(settings =>
                {
                    settings.Domain = "contoso.com";
                    settings.MachineAccountName = "machineName";
                    settings.MachineAccountPassword =
                                      builder.Configuration["Password"];
                });
            }
        });

builder.Services.AddRazorPages();
// </snippet_ldap>

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
#elif HTS
// <snippet_hts>
using Microsoft.AspNetCore.Server.HttpSys;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.WebHost.UseHttpSys(options =>
        {
            options.Authentication.Schemes =
                AuthenticationSchemes.NTLM |
                AuthenticationSchemes.Negotiate;
            options.Authentication.AllowAnonymous = false;
        });
}
// </snippet_hts>

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
#elif IMP
#pragma warning disable CA1416 // Validate platform compatibility
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Security.Principal;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
// <snippet_imp>
app.Run(async (context) =>
{
    try
    {
        var user = (WindowsIdentity)context.User.Identity!;

        await context.Response
            .WriteAsync($"User: {user.Name}\tState: {user.ImpersonationLevel}\n");

        await WindowsIdentity.RunImpersonatedAsync(user.AccessToken, async () =>
        {
            var impersonatedUser = WindowsIdentity.GetCurrent();
            var message =
                $"User: {impersonatedUser.Name}\t" +
                $"State: {impersonatedUser.ImpersonationLevel}";

            var bytes = Encoding.UTF8.GetBytes(message);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        });
    }
    catch (Exception e)
    {
        await context.Response.WriteAsync(e.ToString());
    }
});

// </snippet_imp>
#pragma warning restore CA1416 // Validate platform compatibility
#endif
