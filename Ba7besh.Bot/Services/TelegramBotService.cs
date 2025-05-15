using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Ba7besh.Application.BusinessDiscovery;
using Ba7besh.Application.ReviewManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ba7besh.Bot.Services;

public class TelegramBotService(
    ILogger<TelegramBotService> logger,
    IOptions<BotConfiguration> botOptions,
    IBa7beshApiClient apiClient,
    ConversationService conversationService)
    : IHostedService
{
    private readonly TelegramBotClient _botClient = new(botOptions.Value.BotToken);
    private readonly CancellationTokenSource _cts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Telegram bot service");
        
        // Start receiving updates
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [],
            DropPendingUpdates = true
        };
        
        _botClient.StartReceiving(
             HandleUpdateAsync,
             HandlePollingErrorAsync,
            receiverOptions,
            cancellationToken: _cts.Token
        );
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram bot service");
        _cts.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Process only messages with text
            if (update.Message is not { } message || message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            logger.LogInformation("Received message from {ChatId}: {Text}", chatId, messageText);
            
            // Get the current conversation state
            var conversation = conversationService.GetOrCreate(chatId);
            
            // Handle commands
            if (messageText.StartsWith("/"))
            {
                await HandleCommandAsync(message, messageText, cancellationToken);
                return;
            }
            
            // Handle ongoing conversations based on current stage
            switch (conversation.Stage)
            {
                case ConversationStage.SearchingRestaurant:
                    await HandleSearchQueryAsync(message, messageText, cancellationToken);
                    break;
                
                case ConversationStage.AwaitingRestaurantName:
                    await HandleRestaurantNameInputAsync(message, messageText, cancellationToken);
                    break;
                
                case ConversationStage.AwaitingRating:
                    await HandleRatingInputAsync(message, messageText, cancellationToken);
                    break;
                
                case ConversationStage.AwaitingReviewText:
                    await HandleReviewTextInputAsync(message, messageText, cancellationToken);
                    break;
                
                case ConversationStage.AwaitingConfirmation:
                    await HandleConfirmationInputAsync(message, messageText, cancellationToken);
                    break;
                
                default:
                    // Not in a conversation flow, treat as a search query
                    await HandleSearchQueryAsync(message, messageText, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update");
        }
    }

    private async Task HandleCommandAsync(Message message, string commandText, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var command = commandText.Split(' ')[0].ToLower();
        
        conversationService.Reset(chatId);
        switch (command)
        {
            case "/start":
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "مرحباً بك في بحبش! 👋\n\n" +
                          "يمكنني مساعدتك في العثور على المطاعم وتقييمها. يمكنك استخدام الأوامر التالية:\n\n" +
                          "/search - ابحث عن مطعم\n" +
                          "/review - اضف تقييم\n" +
                          "/recommend - اقتراحات مطاعم\n" +
                          "/help - مساعدة",
                    cancellationToken: cancellationToken);
                break;
            
            case "/search":
                var searchQuery = commandText.Replace("/search", "").Trim();
                if (string.IsNullOrEmpty(searchQuery))
                {
                    conversationService.UpdateState(chatId, state => 
                    {
                        state.Stage = ConversationStage.SearchingRestaurant;
                    });
                    
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "ما هو اسم المطعم أو المنطقة التي تبحث عنها؟",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await HandleSearchQueryAsync(message, searchQuery, cancellationToken);
                }
                break;
            
            case "/review":
                conversationService.UpdateState(chatId, state => 
                {
                    state.Stage = ConversationStage.AwaitingRestaurantName;
                });
                
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "ما هو اسم المطعم الذي تريد تقييمه؟",
                    cancellationToken: cancellationToken);
                break;
            
            case "/recommend":
                await SendRecommendationsAsync(chatId, cancellationToken);
                break;
            
            case "/help":
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "كيف يمكنني مساعدتك؟\n\n" +
                          "• للبحث عن مطعم، ما عليك سوى كتابة اسمه أو استخدام /search\n" +
                          "• لتقييم مطعم، استخدم /review\n" +
                          "• للحصول على اقتراحات المطاعم الأفضل تقييماً، استخدم /recommend",
                    cancellationToken: cancellationToken);
                break;
            
            default:
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "عذراً، لم أفهم هذا الأمر. استخدم /help للحصول على قائمة بالأوامر المتاحة.",
                    cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task HandleSearchQueryAsync(Message message, string searchQuery, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "الرجاء إدخال اسم المطعم أو المنطقة للبحث",
                cancellationToken: cancellationToken);
            return;
        }
        
        // Reset the conversation for a new search
        conversationService.Reset(chatId);
        
        await _botClient.SendMessage(
            chatId: chatId,
            text: "🔍 جاري البحث...",
            cancellationToken: cancellationToken);
        
        try
        {
            var searchResult = await apiClient.SearchBusinessesAsync(searchQuery, cancellationToken);
            
            if (searchResult.Businesses.Count == 0)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "لم يتم العثور على نتائج لـ \"" + searchQuery + "\".\n\nحاول البحث باستخدام كلمات أخرى.",
                    cancellationToken: cancellationToken);
                return;
            }
            
            await SendBusinessListAsync(chatId, searchResult.Businesses, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching for businesses");
            
            await _botClient.SendMessage(
                chatId: chatId,
                text: "عذراً، حدث خطأ أثناء البحث. الرجاء المحاولة مرة أخرى لاحقاً.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task SendBusinessListAsync(long chatId, IReadOnlyList<BusinessSummary> businesses, CancellationToken cancellationToken)
    {
        var messageText = $"تم العثور على {businesses.Count} مطعم:\n\n";
        
        for (var i = 0; i < businesses.Count; i++)
        {
            var business = businesses[i];
            
            // Format each business
            messageText += $"*{i + 1}. {business.ArName}*\n";
            
            if (business.Categories.Count > 0)
            {
                var categories = string.Join(", ", business.Categories.Select(c => c.ArName));
                messageText += $"التصنيف: {categories}\n";
            }
            
            if (business is BusinessSummaryWithStats stats)
            {
                messageText += $"التقييم: {GetStarRating(stats.AverageRating)} ({stats.ReviewCount} تقييم)\n";
            }
            
            if (business.DistanceInKm.HasValue)
            {
                messageText += $"المسافة: {business.DistanceInKm:F1} كم\n";
            }
            
            messageText += "\n";
        }
        
        messageText += "للتقييم، استخدم الأمر /review";
        
        await _botClient.SendMessage(
            chatId: chatId,
            text: messageText,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task SendRecommendationsAsync(long chatId, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(
            chatId: chatId,
            text: "🔍 جاري البحث عن أفضل المطاعم...",
            cancellationToken: cancellationToken);
        
        try
        {
            var recommendations = await apiClient.GetTopRatedBusinessesAsync(cancellationToken);
            
            if (recommendations.Count == 0)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "لم يتم العثور على توصيات في الوقت الحالي.",
                    cancellationToken: cancellationToken);
                return;
            }
            
            await SendBusinessListAsync(chatId, recommendations, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting recommendations");
            
            await _botClient.SendMessage(
                chatId: chatId,
                text: "عذراً، حدث خطأ أثناء البحث عن التوصيات. الرجاء المحاولة مرة أخرى لاحقاً.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleRestaurantNameInputAsync(Message message, string restaurantName, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        
        conversationService.UpdateState(chatId, state => 
        {
            state.RestaurantName = restaurantName;
            state.Stage = ConversationStage.AwaitingRating;
        });
        
        // Create rating keyboard
        var ratingKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "⭐⭐⭐⭐⭐", "⭐⭐⭐⭐" },
            new KeyboardButton[] { "⭐⭐⭐", "⭐⭐", "⭐" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        
        await _botClient.SendMessage(
            chatId: chatId,
            text: $"كم تقيم {restaurantName} من 5 نجوم؟",
            replyMarkup: ratingKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleRatingInputAsync(Message message, string ratingInput, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        decimal rating;
        
        // Parse rating from star emojis or number
        if (ratingInput.Contains("⭐"))
        {
            rating = ratingInput.Count(c => c == '⭐');
        }
        else if (!decimal.TryParse(ratingInput, out rating) || rating < 1 || rating > 5)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "الرجاء إدخال رقم من 1 إلى 5 أو استخدام الأزرار المتاحة.",
                cancellationToken: cancellationToken);
            return;
        }
        
        conversationService.UpdateState(chatId, state => 
        {
            state.Rating = rating;
            state.Stage = ConversationStage.AwaitingReviewText;
        });
        
        // Remove keyboard and ask for review text
        var removeKeyboard = new ReplyKeyboardRemove();
        
        await _botClient.SendMessage(
            chatId: chatId,
            text: "اكتب تعليقك عن المطعم (اختياري - يمكنك الضغط على 'تخطي' للتخطي)",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "تخطي" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            },
            cancellationToken: cancellationToken);
    }

    private async Task HandleReviewTextInputAsync(Message message, string reviewText, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        
        if (reviewText == "تخطي")
        {
            reviewText = string.Empty;
        }
        
        conversationService.UpdateState(chatId, state => 
        {
            state.ReviewText = reviewText;
            state.Stage = ConversationStage.AwaitingConfirmation;
        });
        
        var state = conversationService.GetOrCreate(chatId);
        
        // Build confirmation message
        var confirmationMessage = $"*مراجعة تقييمك:*\n\n" +
                                 $"المطعم: {state.RestaurantName}\n" +
                                 $"التقييم: {GetStarRating(state.Rating ?? 0)}\n";
        
        if (!string.IsNullOrEmpty(state.ReviewText))
        {
            confirmationMessage += $"التعليق: {state.ReviewText}\n";
        }
        
        confirmationMessage += "\nهل تريد إرسال هذا التقييم؟";
        
        // Create confirmation keyboard
        var confirmationKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "نعم، أرسل التقييم" },
            new KeyboardButton[] { "لا، إلغاء" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        
        await _botClient.SendMessage(
            chatId: chatId,
            text: confirmationMessage,
            parseMode: ParseMode.Markdown,
            replyMarkup: confirmationKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleConfirmationInputAsync(Message message, string confirmationInput, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var state = conversationService.GetOrCreate(chatId);
        
        var removeKeyboard = new ReplyKeyboardRemove();
        
        if (confirmationInput.StartsWith("نعم"))
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "جاري إرسال تقييمك...",
                replyMarkup: removeKeyboard,
                cancellationToken: cancellationToken);
            
            try
            {
                // In a real implementation, you'd need to find the restaurant ID
                // This is simplified - you'd need to search for the restaurant first
                
                // Simulating review submission
                var reviewCommand = new SubmitReviewCommand
                {
                    BusinessId = "temp_business_id", // This would need to be obtained from a search
                    UserId = chatId.ToString(), // Using chatId as a simple userId
                    OverallRating = state.Rating ?? 5,
                    Content = state.ReviewText
                };
                
                // In a real implementation, call the API
                // await _apiClient.SubmitReviewAsync(reviewCommand, cancellationToken);
                
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "✅ تم إرسال تقييمك بنجاح! شكراً لمشاركتك.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error submitting review");
                
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "عذراً، حدث خطأ أثناء إرسال التقييم. الرجاء المحاولة مرة أخرى لاحقاً.",
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "تم إلغاء التقييم.",
                replyMarkup: removeKeyboard,
                cancellationToken: cancellationToken);
        }
        
        // Reset conversation state
        conversationService.Reset(chatId);
    }

    private static string GetStarRating(decimal rating)
    {
        var fullStars = Math.Floor(rating);
        var halfStar = rating - fullStars >= 0.5m;
            
        var stars = string.Join("", Enumerable.Repeat("⭐", (int)fullStars));
        if (halfStar)
        {
            stars += "½";
        }
            
        return $"{stars} ({rating:F1})";
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}