using Jarvis.Cli;
using Jarvis.Cli.Service;
using Jarvis.Core.Interface;
using Jarvis.Core.Skill;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton(sp => new Lazy<SkillRouter>(sp.GetRequiredService<SkillRouter>));

builder.Services.AddSingleton<ISkill, GreetingSkill>();
builder.Services.AddSingleton<ISkill, TimeSkill>();
builder.Services.AddSingleton<ISkill, HelpSkill>();

builder.Services.AddSingleton<SkillRouter>();
builder.Services.AddHostedService<ConsoleLoop>();
await builder.Build().RunAsync();