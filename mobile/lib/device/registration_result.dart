import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:flutter/foundation.dart';
part 'registration_result.freezed.dart';
part 'registration_result.g.dart';

@freezed
class RegistrationResult with _$RegistrationResult {
  const factory RegistrationResult({
    required String deviceId,
    required String signatureKey,
    required DateTime expiresAt,
  }) = _RegistrationResult;

  factory RegistrationResult.fromJson(Map<String, dynamic> json) =>
      _$RegistrationResultFromJson(json);
}