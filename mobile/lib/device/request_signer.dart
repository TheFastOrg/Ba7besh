import 'dart:convert';

import 'package:crypto/crypto.dart';

import 'device_storage.dart';

class RequestSigner {
  final DeviceStorage _storage;

  RequestSigner(this._storage);

  Future<Map<String, String>> signRequest(String path, [String? body]) async {
    final deviceId = await _storage.getValue('deviceId');
    final signatureKey = await _storage.getValue('signatureKey');
    final timestamp = DateTime.now().toUtc().toIso8601String();

    var content = '$timestamp|$path';
    if (body != null) content += '|$body';

    final key = base64Decode(signatureKey!);
    final hmac = Hmac(sha256, key);
    final signature = base64Encode(hmac.convert(utf8.encode(content)).bytes);

    return {
      'X-Device-ID': deviceId!,
      'X-Signature': signature,
      'X-Timestamp': timestamp,
    };
  }
}