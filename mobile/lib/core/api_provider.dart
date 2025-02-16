import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../device/device_storage.dart';
import '../device/request_signer.dart';
import 'api_client.dart';
import 'config/app_config.dart';

final apiClientProvider = Provider<ApiClient>((ref) {
  final signer = ref.watch(requestSignerProvider);
  return ApiClient(signer, AppConfig.apiBaseUrl);
});

final requestSignerProvider = Provider<RequestSigner>((ref) {
  final storage = ref.watch(deviceStorageProvider);
  return RequestSigner(storage);
});

final deviceStorageProvider = Provider<DeviceStorage>((ref) {
  return DeviceStorage();
});