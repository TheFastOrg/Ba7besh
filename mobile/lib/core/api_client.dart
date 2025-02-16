import 'dart:convert';

import 'package:mobile/device/request_signer.dart';
import 'package:http/http.dart' as http;

import '../device/registration_result.dart';

class ApiException implements Exception {
  final String message;
  final int statusCode;
  final dynamic details;

  ApiException(this.message, this.statusCode, [this.details]);

  factory ApiException.fromResponse(http.Response response) {
    try {
      final body = jsonDecode(response.body);
      return ApiException(
        body['error'] ?? 'Unknown error',
        response.statusCode,
        body['details'],
      );
    } catch (e) {
      return ApiException(
        'Failed to parse error response',
        response.statusCode,
      );
    }
  }

  @override
  String toString() => 'ApiException: $message (Status: $statusCode)';
}

class ApiClient {
  final RequestSigner _signer;
  final String _baseUrl;

  ApiClient(this._signer, this._baseUrl);

  Future<T> post<T>(
      String path, {
        required Map<String, dynamic> body,
        bool skipDeviceHeaders = false,
      }) async {
    final Map<String, String> headers = {
      'Content-Type': 'application/json',
    };

    if (!skipDeviceHeaders) {
      headers.addAll(await _signer.signRequest(path, jsonEncode(body)));
    }

    final response = await http.post(
      Uri.parse('$_baseUrl$path'),
      headers: headers,
      body: jsonEncode(body),
    );

    if (response.statusCode != 200) {
      throw ApiException.fromResponse(response);
    }

    return _decodeResponse<T>(response);
  }

  T _decodeResponse<T>(http.Response response) {
    final body = jsonDecode(response.body);
    if (T == RegistrationResult) {
      return RegistrationResult.fromJson(body) as T;
    }
    throw UnimplementedError('Decoder not implemented for type $T');
  }
}