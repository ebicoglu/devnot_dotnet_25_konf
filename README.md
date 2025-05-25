# DEVNOT .NET KONFERANSI 2025 – Anatomy of AI

🗓️ **Tarih:** 24 Mayıs 2025 Cumartesi  
📍 **Yer:** Sheraton Grand İstanbul Hotel  
🌐 [Konferans Web Sitesi](https://dotnet.devnot.com/)

## 🎤 Sunum: Anatomy of AI

Bu repo, DEVNOT .NET Konferansı 2025'te gerçekleştirdiğim **"Anatomy of AI"** sunumuna ait dökümanları ve demoları içermektedir.

### 📖 İçerik

- 📑 Sunum slayt ➡ [anatomy-of-ai.pptx](https://github.com/ebicoglu/devnot_dotnet_25_konf/blob/main/anatomy-of-ai.pptx)
- 💻 Demo projeler ➡ [Demo](https://github.com/ebicoglu/devnot_dotnet_25_konf/tree/main/Demo)


### ℹ Demo Çalıştırmak İçin
1. [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) kurun.
2. OpenAI API key environment variable ekleyin. Powershell ile aşağıdaki gibi ekleyebilirsiniz:
     ```bash
     setx OPENAI_API_KEY "your-api-key"
     ```

⚠ Eğer OpenAI aboneliğiniz yoksa başka bir ücretli/ücretsiz API kullanabilirsiniz. Bunun için projelerdeki [Microsoft.Extensions.AI.OpenAI](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI/) kütüphanesini AzureAI için [Azure.AI.OpenAI](https://www.nuget.org/packages/Azure.AI.OpenAI) veya local'de çalıştırmak için makinanıza Ollama kurup [Microsoft.Extensions.AI.Ollama](https://www.nuget.org/packages/Microsoft.Extensions.AI.Ollama/) veya [OllamaSharp](https://www.nuget.org/packages/OllamaSharp) paketiyle bağlantı kurabilirsiniz. Microsoft çözümü dışında LangChain kütüphanesi de oldukça iyi ve zengin entegrasyonları ile çok iyi bir alternatif [nuget.org/packages/LangChain](https://www.nuget.org/packages/LangChain/).
