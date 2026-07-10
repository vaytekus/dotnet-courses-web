using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CoursesApp.Web.Services;
    
public class EmailDomainValidator(
    ILookupClient lookupClient,
    IMemoryCache cache,
    ILogger<EmailDomainValidator> logger,
    IOptions<EmailValidationOptions> options) : IEmailDomainValidator
{
    private const string _cacheKeyPrefix = "mx:";
    private static readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);

    private readonly HashSet<string> _blackListDomains = new(options.Value.BlacklistedDomains, StringComparer.OrdinalIgnoreCase);
    
    public async Task<bool> HasMxRecordAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var atIndex = email.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return false;
        }

        var domain = email[(atIndex + 1)..].ToLowerInvariant();

        if (_blackListDomains.Contains(domain))
        {
            logger.LogInformation("Domain {Domain} rejected by blacklist", domain);
            return false;
        }

        var cacheKey = _cacheKeyPrefix + domain;
        if (cache.TryGetValue<bool>(cacheKey, out var cached))
        {
            return cached;
        }

        try
        {
            var result = await lookupClient.QueryAsync(domain, QueryType.MX, cancellationToken: ct);
            var hasMx = result.Answers.OfType<MxRecord>().Any();
            cache.Set(cacheKey, hasMx, _cacheTtl);
            return hasMx;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "MX lookup failed for domain {Domain}", domain);
            return false;
        }
    }
}
