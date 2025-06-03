using RecordManagement.Core.DTOs;

namespace RecordManagement.Core.API;

public interface IQuoteService
{
    Task<QuoteDTO?> GetRandomQuoteAsync();
}
