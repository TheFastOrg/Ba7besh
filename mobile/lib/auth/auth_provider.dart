import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/auth/auth_service.dart';
import 'package:mobile/core/api_provider.dart';

class AuthState {
  final User? user;
  final bool isLoading;
  final String? errorMessage;

  AuthState({
    this.user,
    this.isLoading = false,
    this.errorMessage,
  });

  AuthState copyWith({
    User? user,
    bool? isLoading,
    String? errorMessage,
  }) {
    return AuthState(
      user: user ?? this.user,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

final authServiceProvider = Provider<AuthService>((ref) {
  final api = ref.watch(apiClientProvider);
  return AuthService(api);
});

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  final authService = ref.watch(authServiceProvider);
  return AuthNotifier(authService);
});

class AuthNotifier extends StateNotifier<AuthState> {
  final AuthService _authService;

  AuthNotifier(this._authService) : super(AuthState()) {
    _init();
  }

  void _init() {
    _authService.authStateChanges.listen((user) {
      state = state.copyWith(
        user: user,
        isLoading: false,
      );
    });
  }

  // For phone authentication
  Future<void> startPhoneAuth(String phoneNumber) async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    // The actual verification is handled by firebase_phone_auth_handler
    state = state.copyWith(isLoading: false);
  }

  Future<void> completePhoneAuth(UserCredential credential) async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    final result = await _authService.signInWithPhone(credential);

    state = state.copyWith(
      user: result.user,
      isLoading: false,
      errorMessage: result.success ? null : result.errorMessage,
    );
  }

  void setAuthError(String message) {
    state = state.copyWith(errorMessage: message, isLoading: false);
  }

  Future<void> signInWithGoogle() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    final result = await _authService.signInWithGoogle();

    state = state.copyWith(
      user: result.user,
      isLoading: false,
      errorMessage: result.success ? null : result.errorMessage,
    );
  }

  Future<void> signOut() async {
    state = state.copyWith(isLoading: true);
    await _authService.signOut();
    state = state.copyWith(
      user: null,
      isLoading: false,
    );
  }
}