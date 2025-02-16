import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:mobile/device/registration_result.dart';

class DeviceStorage {
  final _storage = const FlutterSecureStorage();

  Future<String?> getValue(String key) => _storage.read(key: key);

  Future<void> setValue(String key, String value) => _storage.write(key: key, value: value);

  Future<void> saveRegistration(RegistrationResult result) async {
    await setValue('deviceId', result.deviceId);
    await setValue('signatureKey', result.signatureKey);
    await setValue('expiresAt', result.expiresAt.toIso8601String());
  }

  Future<bool> hasValidRegistration() async {
    final expiresAtStr = await getValue('expiresAt');
    if (expiresAtStr == null) return false;

    final expiresAt = DateTime.parse(expiresAtStr);
    return expiresAt.isAfter(DateTime.now());
  }
}