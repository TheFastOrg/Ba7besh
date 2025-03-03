import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/device/device_provider.dart';
import 'package:mobile/onboarding/onboarding_provider.dart';

import 'auth_screen.dart';
import 'home_screen.dart';
import 'onboarding_screen.dart';

class LoadingScreen extends ConsumerWidget {
  const LoadingScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final deviceState = ref.watch(deviceProvider);
    final hasCompletedOnboarding = ref.watch(onboardingProvider);
    final authState = ref.watch(authProvider);

    // Wait for device registration
    return deviceState.when(
      data: (registration) {
        if (registration == null) {
          return const _LoadingIndicator();
        }

        // If user hasn't completed onboarding, show onboarding screen
        if (!hasCompletedOnboarding) {
          return const OnboardingScreen();
        }

        // Handle authentication state
        if (authState.isLoading) {
          return const _LoadingIndicator();
        }

        // Navigate based on auth state
        if (authState.user != null) {
          return const HomeScreen();
        } else {
          return const AuthScreen();
        }
      },
      loading: () => const _LoadingIndicator(),
      error: (error, stackTrace) => Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.error_outline,
                size: 48,
                color: Colors.red.shade700,
              ),
              const SizedBox(height: 16),
              Text(
                'Failed to initialize app',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: Colors.grey.shade800,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                error.toString(),
                style: TextStyle(
                  color: Colors.grey.shade600,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
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

class _LoadingIndicator extends StatelessWidget {
  const _LoadingIndicator();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Lottie.asset(
              'assets/animations/loading.json',
              width: 200,
              height: 200,
            ),
            const SizedBox(height: 16),
            Text(
              'Loading Ba7besh...',
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey.shade600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}