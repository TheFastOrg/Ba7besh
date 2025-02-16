import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/material.dart';
import 'package:mobile/device/device_provider.dart';
import 'package:mobile/onboarding/onboarding_provider.dart';

import 'home_screen.dart';
import 'onboarding_screen.dart';
class LoadingScreen extends ConsumerWidget {
  const LoadingScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final deviceState = ref.watch(deviceProvider);
    final hasCompletedOnboarding = ref.watch(onboardingProvider);

    return deviceState.when(
      data: (registration) {
        if (registration == null) {
          return const Scaffold(
            body: Center(
              child: CircularProgressIndicator(),
            ),
          );
        }
        return hasCompletedOnboarding
            ? const HomeScreen()
            : const OnboardingScreen();
      },
      loading: () => const Scaffold(
        body: Center(
          child: CircularProgressIndicator(),
        ),
      ),
      error: (error, stackTrace) => Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Text('Failed to register device'),
              ElevatedButton(
                onPressed: () {
                  ref.read(deviceProvider.notifier).register();
                },
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}