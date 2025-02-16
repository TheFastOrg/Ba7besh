import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/onboarding/onboarding_page.dart';
import 'package:mobile/onboarding/onboarding_provider.dart';
import 'package:mobile/presentation/widgets/page_indicator.dart';
import 'auth_screen.dart';

class OnboardingScreen extends ConsumerStatefulWidget {
  const OnboardingScreen({super.key});

  @override
  ConsumerState<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends ConsumerState<OnboardingScreen> {
  final PageController _pageController = PageController();
  int _currentPage = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          PageView.builder(
            controller: _pageController,
            itemCount: OnboardingPage.pages.length,
            onPageChanged: (int page) {
              setState(() {
                _currentPage = page;
              });
            },
            itemBuilder: (context, index) {
              final page = OnboardingPage.pages[index];
              return _buildPage(page);
            },
          ),
          Positioned(
            bottom: 80,
            left: 0,
            right: 0,
            child: Column(
              children: [
                PageIndicator(
                  count: OnboardingPage.pages.length,
                  currentIndex: _currentPage,
                ),
                const SizedBox(height: 32),
                _buildButton(),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPage(OnboardingPage page) {
    return Padding(
      padding: const EdgeInsets.all(40),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Spacer(),
          Text(
            page.title,
            style: Theme.of(context).textTheme.headlineMedium,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 20),
          Text(
            page.description,
            style: Theme.of(context).textTheme.bodyLarge,
            textAlign: TextAlign.center,
          ),
          const Spacer(),
        ],
      ),
    );
  }

  Widget _buildButton() {
    final isLastPage = _currentPage == OnboardingPage.pages.length - 1;
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 40),
      child: ElevatedButton(
        onPressed: () {
          if (isLastPage) {
            ref.read(onboardingProvider.notifier).completeOnboarding();
            Navigator.of(context).pushReplacement(
              MaterialPageRoute(builder: (_) => const AuthScreen()),
            );
          } else {
            _pageController.nextPage(
              duration: const Duration(milliseconds: 300),
              curve: Curves.easeInOut,
            );
          }
        },
        style: ElevatedButton.styleFrom(
          minimumSize: const Size.fromHeight(50),
        ),
        child: Text(isLastPage ? 'Get Started' : 'Next'),
      ),
    );
  }
}