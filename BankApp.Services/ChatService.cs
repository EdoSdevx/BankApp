using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace BankApp.BankApp.Services;

public class ChatService
{
    private readonly ICustomerPortalService _customerPortal;
    private readonly ICustomerLoanService _customerLoan;
    private readonly ILoanService _loan;
    private readonly HttpClient _http;
    private readonly GroqOptions _options;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        ICustomerPortalService customerPortal,
        ICustomerLoanService customerLoan,
        ILoanService loan,
        IOptions<GroqOptions> options,
        ILogger<ChatService> logger)
    {
        _customerPortal = customerPortal;
        _customerLoan = customerLoan;
        _loan = loan;
        _options = options.Value;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        _logger = logger;
    }

    public async Task<string> ChatAsync(int customerId, string question, CancellationToken ct)
    {
        var messages = new List<ChatMessage>();
        messages.Add(new ChatMessage { Role = "system", Content = BuildSystemPrompt(customerId) });
        messages.Add(new ChatMessage { Role = "user", Content = question });

        for (int round = 0; round < 5; round++)
        {
            var request = new GroqRequest
            {
                Model = _options.Model,
                Messages = messages,
                Tools = GetToolDefinitions(),
                Temperature = 0.3m,
                MaxTokens = 800
            };

            var json = JsonSerializer.Serialize(request);
            var httpResponse = await _http.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"), ct);

            var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
            var response = JsonSerializer.Deserialize<GroqResponse>(responseJson);

            if (response?.Choices is null || response.Choices.Count == 0)
                return "I'm having trouble connecting. Please try again.";

            var choice = response.Choices[0];
            var toolCalls = choice.Message?.ToolCalls;

            if (toolCalls is null || toolCalls.Count == 0)
            {
                var content = choice.Message?.Content ?? "I couldn't determine the answer.";
                _logger.LogInformation("Chat round {Round}: final answer", round + 1);
                return content;
            }

            messages.Add(new ChatMessage
            {
                Role = "assistant",
                ToolCalls = toolCalls
            });

            foreach (var call in toolCalls)
            {
                var result = await ExecuteTool(call.Function.Name, call.Function.Arguments, customerId);
                messages.Add(new ChatMessage
                {
                    Role = "tool",
                    ToolCallId = call.Id,
                    Content = result
                });
                _logger.LogInformation("Chat round {Round}: executed tool {Tool}", round + 1, call.Function.Name);
            }
        }

        return "I couldn't find the answer. Try asking about your accounts, loans, or exchange rates.";
    }

    private string BuildSystemPrompt(int customerId)
    {
        return """
            You are a BankApp banking assistant. You help customers with their accounts,
            loans, exchange rates, and transactions. Follow these rules:

            1. ONLY use the tools provided. Never invent data.
            2. For questions about features we don't have, say: "We don't offer that yet."
            3. For investment or financial advice, say: "I provide information, not advice."
            4. Keep answers concise — 2-4 sentences when possible.
            5. Always mention specific numbers when you have them from tools.
            6. Be helpful but never say things like "I think" or "usually" — use exact data.
            """;
    }

    private List<ToolDefinition> GetToolDefinitions()
    {
        return new()
        {
            new()
            {
                Type = "function",
                Function = new ToolFunction
                {
                    Name = "get_accounts",
                    Description = "Get the customer's bank accounts with balances and currencies",
                    Parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Type = "function",
                Function = new ToolFunction
                {
                    Name = "get_loans",
                    Description = "Get the customer's active and past loans",
                    Parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Type = "function",
                Function = new ToolFunction
                {
                    Name = "get_loan_types",
                    Description = "Get available loan products with interest rates and limits",
                    Parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Type = "function",
                Function = new ToolFunction
                {
                    Name = "get_exchange_rates",
                    Description = "Get current exchange rates for USD, EUR, GBP against TRY",
                    Parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            },
            new()
            {
                Type = "function",
                Function = new ToolFunction
                {
                    Name = "calculate_emi",
                    Description = "Calculate monthly payment for a loan. Returns monthly payment amount.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            amount = new { type = "number", description = "Loan amount in TRY" },
                            termMonths = new { type = "integer", description = "Loan term in months" },
                            annualRate = new { type = "number", description = "Annual interest rate as decimal (0.15 = 15%)" }
                        },
                        required = new[] { "amount", "termMonths", "annualRate" }
                    }
                }
            }
        };
    }

    private async Task<string> ExecuteTool(string name, string arguments, int customerId)
    {
        try
        {
            switch (name)
            {
                case "get_accounts":
                    var accResult = await _customerPortal.GetAccountsAsync(customerId);
                    if (accResult.Success && accResult.Data is not null)
                        return JsonSerializer.Serialize(accResult.Data.Select(a => new
                        {
                            accountId = a.AccountId, currency = a.CurrencyCode,
                            balance = a.Balance, isActive = a.IsActive
                        }));
                    return "No accounts found.";

                case "get_loans":
                    var loanResult = await _customerLoan.GetMyLoansAsync(customerId);
                    if (loanResult.Success && loanResult.Data is not null)
                        return JsonSerializer.Serialize(loanResult.Data.Select(l => new
                        {
                            loanId = l.LoanId, type = l.LoanTypeName,
                            amount = l.Amount, status = l.Status,
                            remaining = l.RemainingPrincipal,
                            monthly = l.MonthlyPayment,
                            rate = l.AnnualInterestRate,
                            paymentsMade = l.PaymentsMade, termMonths = l.TermMonths
                        }));
                    return "No loans found.";

                case "get_loan_types":
                    var ltResult = await _loan.GetLoanTypesAsync();
                    if (ltResult.Success && ltResult.Data is not null)
                        return JsonSerializer.Serialize(ltResult.Data.Select(t => new
                        {
                            name = t.Name, annualRate = t.AnnualInterestRate,
                            minAmount = t.MinAmount, maxAmount = t.MaxAmount,
                            minTerm = t.MinTermMonths, maxTerm = t.MaxTermMonths
                        }));
                    return "No loan types available.";

                case "get_exchange_rates":
                    var erResult = await _customerPortal.GetExchangeRatesAsync();
                    if (erResult.Success && erResult.Data is not null)
                        return JsonSerializer.Serialize(erResult.Data.Select(r => new
                        {
                            from = "TRY", to = r.CurrencyCode, rate = r.Rate
                        }));
                    return "No rates available.";

                case "calculate_emi":
                    var args = JsonSerializer.Deserialize<EmiArgs>(arguments);
                    if (args is null || args.Amount <= 0 || args.TermMonths <= 0)
                        return "Invalid EMI parameters.";
                    var schedule = LoanService.GenerateSchedule(args.Amount, args.AnnualRate, args.TermMonths);
                    var monthly = schedule.Count > 0 ? schedule[0].TotalDue : 0;
                    var totalRepay = monthly * args.TermMonths;
                    return JsonSerializer.Serialize(new
                    {
                        monthlyPayment = monthly,
                        totalRepayment = totalRepay,
                        totalInterest = totalRepay - args.Amount
                    });

                default:
                    return "Unknown tool.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tool execution failed: {Tool}", name);
            return $"Error: {ex.Message}";
        }
    }

    private class EmiArgs
    {
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        [JsonPropertyName("annualRate")]
        public decimal AnnualRate { get; set; }
    }
}

public class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
}

public class GroqRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();
    [JsonPropertyName("tools")]
    public List<ToolDefinition>? Tools { get; set; }
    [JsonPropertyName("temperature")]
    public decimal Temperature { get; set; } = 0.3m;
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 800;
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolCall>? ToolCalls { get; set; }
    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }
}

public class ToolDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";
    [JsonPropertyName("function")]
    public ToolFunction Function { get; set; } = new();
}

public class ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("parameters")]
    public object Parameters { get; set; } = new { type = "object", properties = new { } };
}

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";
    [JsonPropertyName("function")]
    public ToolCallFunction Function { get; set; } = new();
}

public class ToolCallFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

public class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public AssistantMessage? Message { get; set; }
}

public class AssistantMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}
