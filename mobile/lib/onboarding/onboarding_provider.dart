import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/core/onboarding_storage.dart';

final onboardingProvider = StateNotifierProvider<OnboardingNotifier, bool>((ref) {
  return OnboardingNotifier(ref.read(onboardingStorageProvider));
});

class OnboardingNotifier extends StateNotifier<bool> {
  final OnboardingStorage _storage;

  OnboardingNotifier(this._storage) : super(false) {
    _init();
  }

  Future<void> _init() async {
    state = await _storage.hasCompletedOnboarding();
  }

  Future<void> completeOnboarding() async {
    await _storage.setOnboardingComplete();
    state = true;
  }
}