import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final onboardingStorageProvider = Provider((ref) => OnboardingStorage());

class OnboardingStorage {
  final _storage = const FlutterSecureStorage();
  static const _key = 'onboarding_complete';

  Future<bool> hasCompletedOnboarding() async {
    return await _storage.read(key: _key) == 'true';
  }

  Future<void> setOnboardingComplete() async {
    await _storage.write(key: _key, value: 'true');
  }
}