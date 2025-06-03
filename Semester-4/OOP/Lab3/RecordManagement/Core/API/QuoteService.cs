using System.Text.Json;
using RecordManagement.Core.DTOs;

namespace RecordManagement.Core.API
{
    /// <summary>
    /// Provides motivational quote services from external API with fallback support.
    /// </summary>
    public sealed class QuoteService : IQuoteService
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random = new();

        private readonly List<string> _fallbackQuotes = [
            "The only way to do great work is to love what you do. - Steve Jobs",
            "Don't watch the clock; do what it does. Keep going. - Sam Levenson",
            "Believe you can and you're halfway there. - Theodore Roosevelt",
            "It always seems impossible until it's done. - Nelson Mandela",
            "Success is not final, failure is not fatal: It is the courage to continue that counts. - Winston Churchill",
            "The future belongs to those who believe in the beauty of their dreams. - Eleanor Roosevelt",
            "You are never too old to set another goal or to dream a new dream. - C.S. Lewis",
            "The secret of getting ahead is getting started. - Mark Twain",
            "It does not matter how slowly you go as long as you do not stop. - Confucius",
            "Act as if what you do makes a difference. It does. - William James"
        ];

        /// <summary>
        /// Initializes a new instance of QuoteService.
        /// </summary>
        /// <param name="httpClient">HTTP client for API requests.</param>
        public QuoteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Retrieves a random motivational quote.
        /// </summary>
        /// <returns>
        /// Quote DTO if successful, fallback quote if API fails, <c>null</c> in case of error.
        /// </returns>
        /// <remarks>
        /// Attempts to fetch from <c>quotable.io</c> API first, uses local fallback if unavailable.
        /// </remarks>
        public async Task<QuoteDTO?> GetRandomQuoteAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.quotable.io/random");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var quoteData = JsonSerializer.Deserialize<QuoteData>(content);

                return new QuoteDTO
                {
                    Content = quoteData?.Content,
                    Author = quoteData?.Author
                };
            }
            catch
            {
                string fallbackQuote = _fallbackQuotes[_random.Next(_fallbackQuotes.Count)];
                string[] parts = fallbackQuote.Split([" - "], 2, StringSplitOptions.None);

                return new QuoteDTO
                {
                    Content = parts[0],
                    Author = parts.Length > 1 ? parts[1] : "Unknown"
                };
            }
        }

        private sealed class QuoteData
        {
            public string? Content { get; init; }
            public string? Author { get; init; }
        }
    }
}
