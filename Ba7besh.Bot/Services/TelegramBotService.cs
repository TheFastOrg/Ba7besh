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
    ConversationService conversationService,
    TelegramUserAuthProvider authProvider)
    : IHostedService
{
    private readonly TelegramBotClient _botClient = new(botOptions.Value.BotToken);
    private readonly CancellationTokenSource _cts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Telegram bot service");
    
        try {
            // Test connection to Telegram API
            var me = _botClient.GetMe(cancellationToken).GetAwaiter().GetResult();
            logger.LogInformation("Successfully connected to Telegram API. Bot username: {Username}", me.Username);
        
            // Start receiving updates
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions,
                _cts.Token
            );
        
            logger.LogInformation("Bot receiver started successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed to start Telegram bot service");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram bot service");
        _cts.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
    CancellationToken cancellationToken)
{
    try
    {
        // Handle location messages
        if (update.Message?.Location != null)
        {
            await HandleLocationBasedSearchAsync(update.Message, cancellationToken);
            return;
        }

        // Process only messages with text
        if (update.Message is not { } message || message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        var user = message.From;
        
        // Get the current conversation state
        var conversation = conversationService.GetOrCreate(chatId);
        
        logger.LogInformation("Received message from {ChatId}: {Text}", chatId, messageText);

        // Authenticate user automatically
        if (user != null)
        {
            var authResult = await authProvider.AuthenticateUserAsync(
                user.Id, 
                user.FirstName, 
                user.LastName ?? "", 
                user.Username);

            if (!authResult.Success)
            {
                await botClient.SendMessage(
                    chatId,
                    "عذراً، حدث خطأ في التحقق من الهوية. الرجاء المحاولة مرة أخرى.",
                    cancellationToken: cancellationToken);
                return;
            }

            // Store auth info in conversation state
            conversation.UserId = authResult.UserId;
            conversation.BackendToken = authResult.BackendToken;
    
            // Log successful authentication
            logger.LogInformation("User {UserId} authenticated successfully", authResult.UserId);
        }

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
                    chatId,
                    "مرحباً بك في بحبش! 👋\n\n" +
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
                    conversationService.UpdateState(chatId,
                        state => { state.Stage = ConversationStage.SearchingRestaurant; });

                    // Create keyboard with location button
                    var locationKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { KeyboardButton.WithRequestLocation("مشاركة موقعي الحالي") }
                    })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    await _botClient.SendMessage(
                        chatId,
                        "ما هو اسم المطعم أو المنطقة التي تبحث عنها؟ أو يمكنك مشاركة موقعك لإيجاد المطاعم القريبة منك",
                        replyMarkup: locationKeyboard,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await HandleSearchQueryAsync(message, searchQuery, cancellationToken);
                }

                break;

            case "/review":
                conversationService.UpdateState(chatId,
                    state => { state.Stage = ConversationStage.AwaitingRestaurantName; });

                await _botClient.SendMessage(
                    chatId,
                    "ما هو اسم المطعم الذي تريد تقييمه؟",
                    cancellationToken: cancellationToken);
                break;

            case "/recommend":
                await SendRecommendationsAsync(chatId, cancellationToken);
                break;

            case "/help":
                await _botClient.SendMessage(
                    chatId,
                    "كيف يمكنني مساعدتك؟\n\n" +
                    "• للبحث عن مطعم، ما عليك سوى كتابة اسمه أو استخدام /search\n" +
                    "• لتقييم مطعم، استخدم /review\n" +
                    "• للحصول على اقتراحات المطاعم الأفضل تقييماً، استخدم /recommend",
                    cancellationToken: cancellationToken);
                break;

            default:
                await _botClient.SendMessage(
                    chatId,
                    "عذراً، لم أفهم هذا الأمر. استخدم /help للحصول على قائمة بالأوامر المتاحة.",
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
                chatId,
                "الرجاء إدخال اسم المطعم أو المنطقة للبحث",
                cancellationToken: cancellationToken);
            return;
        }

        // Reset the conversation for a new search
        conversationService.Reset(chatId);

        await _botClient.SendMessage(
            chatId,
            "🔍 جاري البحث...",
            cancellationToken: cancellationToken);

        try
        {
            var searchResult = await apiClient.SearchBusinessesAsync(new SearchBusinessesQuery{ SearchTerm = searchQuery}, cancellationToken);

            if (searchResult.Businesses.Count == 0)
            {
                await _botClient.SendMessage(
                    chatId,
                    "لم يتم العثور على نتائج لـ \"" + searchQuery + "\".\n\nحاول البحث باستخدام كلمات أخرى.",
                    cancellationToken: cancellationToken);
                return;
            }

            await SendBusinessListAsync(chatId, searchResult.Businesses, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching for businesses");

            await _botClient.SendMessage(
                chatId,
                "عذراً، حدث خطأ أثناء البحث. الرجاء المحاولة مرة أخرى لاحقاً.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleLocationBasedSearchAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        if (message.Location == null)
        {
            await _botClient.SendMessage(
                chatId,
                "الرجاء مشاركة موقعك الحالي لإيجاد المطاعم القريبة منك",
                cancellationToken: cancellationToken);
            return;
        }

        var location = new Application.BusinessDiscovery.Location
        {
            Latitude = message.Location.Latitude,
            Longitude = message.Location.Longitude
        };

        await _botClient.SendMessage(
            chatId,
            "🔍 جاري البحث عن المطاعم القريبة منك...",
            cancellationToken: cancellationToken);

        try
        {
            // Search for nearby restaurants with a 5km radius
            var searchQuery = new SearchBusinessesQuery
            {
                CenterLocation = location,
                RadiusKm = 5
            };

            var searchResult = await apiClient.SearchBusinessesAsync(searchQuery, cancellationToken);

            if (searchResult.Businesses.Count == 0)
            {
                await _botClient.SendMessage(
                    chatId,
                    "لم يتم العثور على مطاعم بالقرب منك.",
                    cancellationToken: cancellationToken);
                return;
            }

            // Sort results by distance
            var sortedBusinesses = searchResult.Businesses
                .OrderBy(b => b.DistanceInKm)
                .ToList();

            await SendBusinessListAsync(chatId, sortedBusinesses, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching for nearby restaurants");

            await _botClient.SendMessage(
                chatId,
                "عذراً، حدث خطأ أثناء البحث. الرجاء المحاولة مرة أخرى لاحقاً.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task SendBusinessListAsync(long chatId, IReadOnlyList<BusinessSummary> businesses,
        CancellationToken cancellationToken)
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
                messageText += $"التقييم: {GetStarRating(stats.AverageRating)} ({stats.ReviewCount} تقييم)\n";

            if (business.DistanceInKm.HasValue) messageText += $"المسافة: {business.DistanceInKm:F1} كم\n";

            messageText += "\n";
        }

        messageText += "للتقييم، استخدم الأمر /review";

        await _botClient.SendMessage(
            chatId,
            messageText,
            ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task SendRecommendationsAsync(long chatId, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(
            chatId,
            "🔍 جاري البحث عن أفضل المطاعم...",
            cancellationToken: cancellationToken);

        try
        {
            var recommendations = await apiClient.GetTopRatedBusinessesAsync(cancellationToken);

            if (recommendations.Count == 0)
            {
                await _botClient.SendMessage(
                    chatId,
                    "لم يتم العثور على توصيات في الوقت الحالي.",
                    cancellationToken: cancellationToken);
                return;
            }

            await SendBusinessListAsync(chatId, recommendations, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting recommendations");

            await _botClient.SendMessage(
                chatId,
                "عذراً، حدث خطأ أثناء البحث عن التوصيات. الرجاء المحاولة مرة أخرى لاحقاً.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleRestaurantNameInputAsync(Message message, string restaurantName,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        // First try to find the business in the database
        var business = await apiClient.FindBusinessByNameAsync(restaurantName, cancellationToken);

        if (business == null)
        {
            // If business not found, we can still proceed but note that we'll use a temp ID
            conversationService.UpdateState(chatId, state =>
            {
                state.RestaurantName = restaurantName;
                state.RestaurantId = null; // We don't have an ID
                state.Stage = ConversationStage.AwaitingRating;
            });

            await _botClient.SendMessage(
                chatId,
                $"لم نتمكن من العثور على '{restaurantName}' في قاعدة البيانات، ولكن سنقوم بحفظ تقييمك.",
                cancellationToken: cancellationToken);
        }
        else
        {
            // If business found, store its ID
            conversationService.UpdateState(chatId, state =>
            {
                state.RestaurantName = business.ArName;
                state.RestaurantId = business.Id;
                state.Stage = ConversationStage.AwaitingRating;
            });
        }

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
            chatId,
            $"كم تقيم {restaurantName} من 5 نجوم؟",
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
                chatId,
                "الرجاء إدخال رقم من 1 إلى 5 أو استخدام الأزرار المتاحة.",
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
            chatId,
            "اكتب تعليقك عن المطعم (اختياري - يمكنك الضغط على 'تخطي' للتخطي)",
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

    private async Task HandleReviewTextInputAsync(Message message, string reviewText,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        if (reviewText == "تخطي") reviewText = string.Empty;

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

        if (!string.IsNullOrEmpty(state.ReviewText)) confirmationMessage += $"التعليق: {state.ReviewText}\n";

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
            chatId,
            confirmationMessage,
            ParseMode.Markdown,
            replyMarkup: confirmationKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleConfirmationInputAsync(Message message, string confirmationInput,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var state = conversationService.GetOrCreate(chatId);

        var removeKeyboard = new ReplyKeyboardRemove();

        if (confirmationInput.StartsWith("نعم"))
        {
            await _botClient.SendMessage(
                chatId,
                "جاري إرسال تقييمك...",
                replyMarkup: removeKeyboard,
                cancellationToken: cancellationToken);

            try
            {
                // Create the review command
                var reviewCommand = new SubmitReviewCommand
                {
                    BusinessId = state.RestaurantId ?? "suggested_business",
                    UserId = state.UserId ?? throw new InvalidOperationException("User not authenticated"), // FIXED!
                    OverallRating = state.Rating ?? 5,
                    Content = state.ReviewText
                };

                var success = false;

                if (state.RestaurantId != null)
                {
                    logger.LogInformation("Submitting review with Firebase token: {HasToken}", !string.IsNullOrEmpty(state.BackendToken));

                    // If we have a business ID, submit through API
                    success = await apiClient.SubmitReviewAsync(reviewCommand, state.BackendToken, cancellationToken);
                }
                else
                {
                    // If no business ID, we can suggest the business along with the review
                    // This is a simplification - you might want to add a proper business suggestion flow
                    logger.LogInformation(
                        "Received review for unknown business: {BusinessName}. Storing locally.",
                        state.RestaurantName);
                    success = true;
                }

                if (success)
                    await _botClient.SendMessage(
                        chatId,
                        "✅ تم إرسال تقييمك بنجاح! شكراً لمشاركتك.",
                        cancellationToken: cancellationToken);
                else
                    await _botClient.SendMessage(
                        chatId,
                        "عذراً، حدث خطأ أثناء إرسال التقييم. الرجاء المحاولة مرة أخرى لاحقاً.",
                        cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error submitting review");

                await _botClient.SendMessage(
                    chatId,
                    "عذراً، حدث خطأ أثناء إرسال التقييم. الرجاء المحاولة مرة أخرى لاحقاً.",
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            await _botClient.SendMessage(
                chatId,
                "تم إلغاء التقييم.",
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
        if (halfStar) stars += "½";

        return $"{stars} ({rating:F1})";
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}