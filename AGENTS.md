# H.Recognizers Agent Notes

## Project Role

- This repo contains speech-to-text recognizer modules for H-Core/HomeCenter.
- It includes Yandex Cloud, Wit.ai, and Microsoft Speech recognizer implementations, with streaming paths.
- For career/portfolio analysis, this repo supports the voice-command side of the Raycast-style Windows automation story.

## Layout

- `H.Recognizers.sln` is the solution entry point.
- `src/libs/H.Recognizers.YandexRecognizer` contains Yandex recognition implementations.
- `src/libs/H.Recognizers.WitAiRecognizer` contains Wit.ai recognition implementations.
- `src/libs/H.Recognizers.SystemSpeechRecognizer` contains Microsoft Speech recognition integrations and a local `Microsoft.Speech.dll` dependency.
- `src/libs/H.Yandex.Cloud.SDK` contains vendored/generated Yandex Cloud SDK and proto assets.
- `src/apps/H.Recognizers.App.WPF` is a small WPF host app.
- `src/tests/H.Recognizers.IntegrationTests` contains integration tests and sample WAV data.

## Build And Test

- Historical CI used Windows with .NET 5.
- From the repo root, the intended validation path is:

```bash
dotnet test H.Recognizers.sln --configuration Release --nologo
```

- Full validation is Windows-oriented because of WPF, Microsoft Speech, `net4.x`, and audio/speech runtime assumptions.
- Integration tests may require network access, valid provider credentials, service availability, and stable test audio.

## Maintenance Notes

- `src/Directory.Build.props` enables `LangVersion=preview` and nullable reference types.
- Library package metadata and versions live under `src/libs/Directory.Build.props`.
- Treat `src/libs/H.Yandex.Cloud.SDK/cloudapi` as vendored/generated provider material. Avoid broad manual edits unless regenerating or updating the provider SDK deliberately.
- Keep streaming recognizer behavior compatible with `H.Core` recognition abstractions and `H.Services.RecognitionService`.
- Do not commit OAuth tokens, API keys, folder IDs, private audio, generated credentials, or provider-specific secrets.
