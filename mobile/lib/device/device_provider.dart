import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/core/api_client.dart';
import 'package:package_info_plus/package_info_plus.dart';

import '../core/api_provider.dart';
import 'device_storage.dart';
import 'registration_result.dart';

final deviceProvider =
    StateNotifierProvider<DeviceNotifier, AsyncValue<RegistrationResult?>>(
        (ref) {
  final storage = ref.read(deviceStorageProvider);
  final api = ref.read(apiClientProvider);
  return DeviceNotifier(storage, api);
});

class DeviceNotifier extends StateNotifier<AsyncValue<RegistrationResult?>> {
  final DeviceStorage _storage;
  final ApiClient _api;

  DeviceNotifier(this._storage, this._api) : super(const AsyncValue.data(null));

  Future<void> ensureRegistered() async {
    if (await _storage.hasValidRegistration()) return;
    await register();
  }

  Future<void> register() async {
    state = const AsyncValue.loading();
    try {
      final packageInfo = await PackageInfo.fromPlatform();

      final result = await _api.post<RegistrationResult>(
        '/devices/register',
        body: {'appVersion': packageInfo.version},
        skipDeviceHeaders: true,
      );

      await _storage.saveRegistration(result);
      state = AsyncValue.data(result);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
    }
  }
}
