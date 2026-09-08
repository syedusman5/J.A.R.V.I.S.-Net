using Jarvis.Cli;
using Jarvis.Cli.Service;
using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using Jarvis.Core.Skill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection(WeatherOptions.SectionName));
builder.Services.Configure<FileSearchOptions>(builder.Configuration.GetSection(FileSearchOptions.SectionName));

// Filter out the Microsoft.Hosting.Lifetime logs to reduce noise in the console output
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

// Register dependencies
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ReminderStore>();

// Register HttpClient for skills that need it
builder.Services.AddHttpClient("jarvis", c =>
{
    c.Timeout = TimeSpan.FromSeconds(5);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("JarvisAssistant/0.1");
});

// Voice off by default; flip Jarvis:VoiceEnabled in appsettings.json to turn it on.
var voiceEnabled = true;
if (voiceEnabled)
    builder.Services.AddSingleton<ISpeechOutput, SystemSpeechOutput>();
else
    builder.Services.AddSingleton<ISpeechOutput, NullSpeechOutput>();

// Register SkillRouter
builder.Services.AddSingleton<SkillRouter>();

// Register SkillRouter as a singleton and ensure it's lazily initialized
builder.Services.AddSingleton(sp => new Lazy<SkillRouter>(sp.GetRequiredService<SkillRouter>));

// Register skills
builder.Services.AddSingleton<ISkill, GreetingSkill>();
builder.Services.AddSingleton<ISkill, TimeSkill>();
builder.Services.AddSingleton<ISkill, HelpSkill>();
builder.Services.AddSingleton<ISkill, WeatherSkill>();
builder.Services.AddSingleton<ISkill, FileSearchSkill>();
builder.Services.AddSingleton<ISkill, ReminderSkill>();

// Register services
builder.Services.AddHostedService<ReminderService>();
builder.Services.AddHostedService<ConsoleLoop>();

// Run the application
await builder.Build().RunAsync();