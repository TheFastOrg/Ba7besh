namespace Ba7besh.Bot.Resources;

public static class ArabicMessages
{
    public static class Commands
    {
        public const string Start = "/start";
        public const string Search = "/search";
        public const string Review = "/review";
        public const string Recommend = "/recommend";
        public const string Help = "/help";
    }
    
    public static class Welcome
    {
        public const string Greeting = "مرحباً بك في بحبش! 👋";
        public const string Description = "يمكنني مساعدتك في العثور على المطاعم وتقييمها. يمكنك استخدام الأوامر التالية:";
        public const string CommandList = @"/search - ابحث عن مطعم
/review - اضف تقييم
/recommend - اقتراحات مطاعم
/help - مساعدة";
    }
    
    public static class Search
    {
        public const string Prompt = "ما هو اسم المطعم أو المنطقة التي تبحث عنها؟";
        public const string Searching = "🔍 جاري البحث...";
        public const string NoResults = "لم يتم العثور على نتائج. حاول البحث باستخدام كلمات أخرى.";
        public const string ResultsFormat = "تم العثور على {0} مطعم:";
        public const string ErrorMessage = "عذراً، حدث خطأ أثناء البحث. الرجاء المحاولة مرة أخرى لاحقاً.";
    }
    
    public static class Review
    {
        public const string RestaurantNamePrompt = "ما هو اسم المطعم الذي تريد تقييمه؟";
        public const string RatingPrompt = "كم تقيم {0} من 5 نجوم؟";
        public const string CommentPrompt = "اكتب تعليقك عن المطعم (اختياري - يمكنك الضغط على 'تخطي' للتخطي)";
        public const string Skip = "تخطي";
        public const string ConfirmationFormat = "*مراجعة تقييمك:*\n\nالمطعم: {0}\nالتقييم: {1}\n{2}\nهل تريد إرسال هذا التقييم؟";
        public const string CommentFormat = "التعليق: {0}\n";
        public const string Submitting = "جاري إرسال تقييمك...";
        public const string Success = "✅ تم إرسال تقييمك بنجاح! شكراً لمشاركتك.";
        public const string Canceled = "تم إلغاء التقييم.";
        public const string Error = "عذراً، حدث خطأ أثناء إرسال التقييم. الرجاء المحاولة مرة أخرى لاحقاً.";
        public const string InvalidRating = "الرجاء إدخال رقم من 1 إلى 5 أو استخدام الأزرار المتاحة.";
    }
    
    public static class Buttons
    {
        public const string Yes = "نعم، أرسل التقييم";
        public const string No = "لا، إلغاء";
    }
    
    public static class BusinessInfo
    {
        public const string Category = "التصنيف: {0}";
        public const string Rating = "التقييم: {0} ({1} تقييم)";
        public const string Distance = "المسافة: {0:F1} كم";
    }
}