import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import 'package:mobile/onboarding/onboarding_page.dart';
import 'package:mobile/onboarding/onboarding_provider.dart';
import 'package:mobile/presentation/widgets/page_indicator.dart';
import 'auth_screen.dart';

class OnboardingScreen extends ConsumerStatefulWidget {
  const OnboardingScreen({super.key});

  @override
  ConsumerState<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends ConsumerState<OnboardingScreen> with TickerProviderStateMixin {
  final PageController _pageController = PageController();
  late final AnimationController _lottieController;
  int _currentPage = 0;

  @override
  void initState() {
    super.initState();
    _lottieController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 2),
    );
  }

  @override
  void dispose() {
    _pageController.dispose();
    _lottieController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            Expanded(
              child: PageView.builder(
                controller: _pageController,
                itemCount: OnboardingPage.pages.length,
                onPageChanged: (int page) {
                  setState(() {
                    _currentPage = page;
                  });
                  _lottieController.reset();
                  _lottieController.forward();
                },
                itemBuilder: (context, index) {
                  final page = OnboardingPage.pages[index];
                  return _buildPage(page);
                },
              ),
            ),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
              child: Column(
                children: [
                  PageIndicator(
                    count: OnboardingPage.pages.length,
                    currentIndex: _currentPage,
                  ),
                  const SizedBox(height: 32),
                  _buildButton(),
                  const SizedBox(height: 16),
                  TextButton(
                    onPressed: () {
                      ref.read(onboardingProvider.notifier).completeOnboarding();
                      Navigator.of(context).pushReplacement(
                        MaterialPageRoute(builder: (_) => const AuthScreen()),
                      );
                    },
                    child: const Text('Skip'),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPage(OnboardingPage page) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Lottie.asset(
            page.animation,
            controller: _lottieController,
            onLoaded: (composition) {
              _lottieController
                ..duration = composition.duration
                ..forward();
            },
          ),
          const SizedBox(height: 48),
          Text(
            page.title,
            style: Theme.of(context).textTheme.headlineMedium,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          Text(
            page.description,
            style: Theme.of(context).textTheme.bodyLarge,
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildButton() {
    final isLastPage = _currentPage == OnboardingPage.pages.length - 1;
    return SizedBox(
      width: double.infinity,
      child: FilledButton(
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
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Text(isLastPage ? 'Get Started' : 'Next'),
        ),
      ),
    );
  }
}