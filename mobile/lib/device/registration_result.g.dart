// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'registration_result.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$RegistrationResultImpl _$$RegistrationResultImplFromJson(
        Map<String, dynamic> json) =>
    _$RegistrationResultImpl(
      deviceId: json['deviceId'] as String,
      signatureKey: json['signatureKey'] as String,
      expiresAt: DateTime.parse(json['expiresAt'] as String),
    );

Map<String, dynamic> _$$RegistrationResultImplToJson(
        _$RegistrationResultImpl instance) =>
    <String, dynamic>{
      'deviceId': instance.deviceId,
      'signatureKey': instance.signatureKey,
      'expiresAt': instance.expiresAt.toIso8601String(),
    };
