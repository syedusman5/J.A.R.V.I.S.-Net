# J.A.R.V.I.S.-Net

A Jarvis-inspired personal assistant built with **.NET 10**. It combines a skill-based architecture, voice interaction, live weather data, file search, reminders, and a local LLM fallback powered by Ollama.

> This is an independent learning project inspired by the idea of a personal assistant; it is not affiliated with Marvel or Iron Man.

## Why I built this

I often forget where I saved a work document. This project started as a way to ask an assistant to find files, then grew into an experiment with local AI, speech, APIs, and clean .NET architecture.

## Features

- Current time and date
- Live weather via the free [Open-Meteo API](https://open-meteo.com/)
- File-name search inside a configured sandbox folder
- Timed reminders
- Voice recording and local speech-to-text with Whisper.net
- Text-to-speech output using the operating system speech engine
- Local LLM fallback with Ollama and `llama3.2:3b` for requests that do not match a skill confidently
- Automatic `help` output generated from registered skills

## How it works

Each ability is a separate **skill**. For example, `WeatherSkill` handles weather, `FileSearchSkill` searches files, and `ReminderSkill` sets reminders.

`SkillRouter` asks every skill for a confidence score:

```text
User request
    |
    v
SkillRouter --> confident skill? --> run the skill
    |
    +--> no confident match --> Ollama classifies intent --> validate result --> run a registered skill
```

The LLM does not receive permission to run arbitrary commands. It can only choose an already-registered skill, and the selected skill still validates its input.

## Project structure

```text
src/
  Jarvis.Core/   Core skills, routing, reminders, weather, and LLM service
  Jarvis.Cli/    Console host, audio recording, Whisper, and speech output
tests/
  Jarvis.Tests/  Automated routing tests
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows for the current NAudio/WASAPI recording implementation
- [Ollama](https://ollama.com/) for natural-language intent fallback
- Internet access for weather lookups

## Getting started

1. Clone the repository and enter the project folder.

   ```powershell
   git clone https://github.com/YOUR-USERNAME/J.A.R.V.I.S.-Net.git
   cd J.A.R.V.I.S.-Net
   ```

2. Restore and build.

   ```powershell
   dotnet restore
   dotnet build
   ```

3. Install the local LLM model.

   ```powershell
   ollama pull llama3.2:3b
   ollama serve
   ```

4. Run Jarvis in a second terminal.

   ```powershell
   dotnet run --project src/Jarvis.Cli
   ```

5. Run tests.

   ```powershell
   dotnet test
   ```

## Configuration

Configuration is supplied through `appsettings.json`. Do not commit personal folders, tokens, or secrets.

```json
{
  "Jarvis": {
    "VoiceEnabled": false
  },
  "Weather": {
    "DefaultLocation": "Chennai"
  },
  "FileSearch": {
    "SandboxRoot": "C:\\Path\\To\\Your\\Safe\\Folder",
    "MaxResults": 10
  }
}
```

Keep `SandboxRoot` narrow, such as a dedicated documents folder. File search is intended to be read-only.

## Example requests

```text
what time is it
weather in Chennai
find file project proposal
remind me in 10 minutes to stretch
list reminders
do I need a jacket in London?
help
```

## Safety and privacy

- Ollama and Whisper run locally; your voice and requests do not need to leave your computer for LLM/speech recognition.
- Weather is the only current feature that uses a public internet API.
- File search should remain restricted to a configured sandbox.
- Never place API keys, personal paths, or downloaded models in source control.
- Model output is treated as untrusted input and cannot execute shell commands.

## Roadmap

- [ ] More natural voices with ElevenLabs or Piper
- [ ] Text-mode fallback alongside voice input
- [ ] Search document contents, not only file names
- [ ] SQLite-backed reminders
- [ ] Conversation memory for follow-up questions
- [ ] Calendar, notes, email, and music integrations
- [ ] Desktop or web dashboard
- [ ] Wake-word support

## Contributing

Ideas, issues, and pull requests are welcome. If you add a skill, include routing tests so it does not accidentally take over another skill's requests.
