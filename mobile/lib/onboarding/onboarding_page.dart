class OnboardingPage {
  final String title;
  final String description;
  final String animation;

  const OnboardingPage({
    required this.title,
    required this.description,
    required this.animation,
  });

  static const List<OnboardingPage> pages = [
    OnboardingPage(
      title: 'Discover Local Gems',
      description: 'Find and explore trusted local restaurants in your area',
      animation: 'assets/animations/discover.json',
    ),
    OnboardingPage(
      title: 'Real Reviews',
      description: 'Get authentic reviews and photos from food lovers like you',
      animation: 'assets/animations/reviews.json',
    ),
    OnboardingPage(
      title: 'Join the Community',
      description: 'Share your experiences and help others discover great food',
      animation: 'assets/animations/community.json',
    ),
  ];
}