class OnboardingPage {
  final String title;
  final String description;
  final String image;

  const OnboardingPage({
    required this.title,
    required this.description,
    required this.image,
  });

  static const List<OnboardingPage> pages = [
    OnboardingPage(
      title: 'Discover Local Gems',
      description: 'Find and explore trusted local restaurants in your area',
      image: 'assets/images/discover.png',
    ),
    OnboardingPage(
      title: 'Real Reviews',
      description: 'Get authentic reviews and photos from food lovers like you',
      image: 'assets/images/reviews.png',
    ),
    OnboardingPage(
      title: 'Join the Community',
      description: 'Share your experiences and help others discover great food',
      image: 'assets/images/community.png',
    ),
  ];
}