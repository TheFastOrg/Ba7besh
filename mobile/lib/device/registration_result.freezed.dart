// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'registration_result.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models');

RegistrationResult _$RegistrationResultFromJson(Map<String, dynamic> json) {
  return _RegistrationResult.fromJson(json);
}

/// @nodoc
mixin _$RegistrationResult {
  String get deviceId => throw _privateConstructorUsedError;
  String get signatureKey => throw _privateConstructorUsedError;
  DateTime get expiresAt => throw _privateConstructorUsedError;

  /// Serializes this RegistrationResult to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of RegistrationResult
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $RegistrationResultCopyWith<RegistrationResult> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $RegistrationResultCopyWith<$Res> {
  factory $RegistrationResultCopyWith(
          RegistrationResult value, $Res Function(RegistrationResult) then) =
      _$RegistrationResultCopyWithImpl<$Res, RegistrationResult>;
  @useResult
  $Res call({String deviceId, String signatureKey, DateTime expiresAt});
}

/// @nodoc
class _$RegistrationResultCopyWithImpl<$Res, $Val extends RegistrationResult>
    implements $RegistrationResultCopyWith<$Res> {
  _$RegistrationResultCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of RegistrationResult
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? deviceId = null,
    Object? signatureKey = null,
    Object? expiresAt = null,
  }) {
    return _then(_value.copyWith(
      deviceId: null == deviceId
          ? _value.deviceId
          : deviceId // ignore: cast_nullable_to_non_nullable
              as String,
      signatureKey: null == signatureKey
          ? _value.signatureKey
          : signatureKey // ignore: cast_nullable_to_non_nullable
              as String,
      expiresAt: null == expiresAt
          ? _value.expiresAt
          : expiresAt // ignore: cast_nullable_to_non_nullable
              as DateTime,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$RegistrationResultImplCopyWith<$Res>
    implements $RegistrationResultCopyWith<$Res> {
  factory _$$RegistrationResultImplCopyWith(_$RegistrationResultImpl value,
          $Res Function(_$RegistrationResultImpl) then) =
      __$$RegistrationResultImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String deviceId, String signatureKey, DateTime expiresAt});
}

/// @nodoc
class __$$RegistrationResultImplCopyWithImpl<$Res>
    extends _$RegistrationResultCopyWithImpl<$Res, _$RegistrationResultImpl>
    implements _$$RegistrationResultImplCopyWith<$Res> {
  __$$RegistrationResultImplCopyWithImpl(_$RegistrationResultImpl _value,
      $Res Function(_$RegistrationResultImpl) _then)
      : super(_value, _then);

  /// Create a copy of RegistrationResult
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? deviceId = null,
    Object? signatureKey = null,
    Object? expiresAt = null,
  }) {
    return _then(_$RegistrationResultImpl(
      deviceId: null == deviceId
          ? _value.deviceId
          : deviceId // ignore: cast_nullable_to_non_nullable
              as String,
      signatureKey: null == signatureKey
          ? _value.signatureKey
          : signatureKey // ignore: cast_nullable_to_non_nullable
              as String,
      expiresAt: null == expiresAt
          ? _value.expiresAt
          : expiresAt // ignore: cast_nullable_to_non_nullable
              as DateTime,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$RegistrationResultImpl
    with DiagnosticableTreeMixin
    implements _RegistrationResult {
  const _$RegistrationResultImpl(
      {required this.deviceId,
      required this.signatureKey,
      required this.expiresAt});

  factory _$RegistrationResultImpl.fromJson(Map<String, dynamic> json) =>
      _$$RegistrationResultImplFromJson(json);

  @override
  final String deviceId;
  @override
  final String signatureKey;
  @override
  final DateTime expiresAt;

  @override
  String toString({DiagnosticLevel minLevel = DiagnosticLevel.info}) {
    return 'RegistrationResult(deviceId: $deviceId, signatureKey: $signatureKey, expiresAt: $expiresAt)';
  }

  @override
  void debugFillProperties(DiagnosticPropertiesBuilder properties) {
    super.debugFillProperties(properties);
    properties
      ..add(DiagnosticsProperty('type', 'RegistrationResult'))
      ..add(DiagnosticsProperty('deviceId', deviceId))
      ..add(DiagnosticsProperty('signatureKey', signatureKey))
      ..add(DiagnosticsProperty('expiresAt', expiresAt));
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$RegistrationResultImpl &&
            (identical(other.deviceId, deviceId) ||
                other.deviceId == deviceId) &&
            (identical(other.signatureKey, signatureKey) ||
                other.signatureKey == signatureKey) &&
            (identical(other.expiresAt, expiresAt) ||
                other.expiresAt == expiresAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, deviceId, signatureKey, expiresAt);

  /// Create a copy of RegistrationResult
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$RegistrationResultImplCopyWith<_$RegistrationResultImpl> get copyWith =>
      __$$RegistrationResultImplCopyWithImpl<_$RegistrationResultImpl>(
          this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$RegistrationResultImplToJson(
      this,
    );
  }
}

abstract class _RegistrationResult implements RegistrationResult {
  const factory _RegistrationResult(
      {required final String deviceId,
      required final String signatureKey,
      required final DateTime expiresAt}) = _$RegistrationResultImpl;

  factory _RegistrationResult.fromJson(Map<String, dynamic> json) =
      _$RegistrationResultImpl.fromJson;

  @override
  String get deviceId;
  @override
  String get signatureKey;
  @override
  DateTime get expiresAt;

  /// Create a copy of RegistrationResult
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$RegistrationResultImplCopyWith<_$RegistrationResultImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
