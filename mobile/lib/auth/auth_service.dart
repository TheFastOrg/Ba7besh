import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:mobile/core/api_client.dart';

class AuthResult {
  final bool success;
  final User? user;
  final String? errorMessage;

  AuthResult({
    required this.success,
    this.user,
    this.errorMessage,
  });
}

class AuthService {
  final FirebaseAuth _auth = FirebaseAuth.instance;
  final GoogleSignIn _googleSignIn = GoogleSignIn();
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  final ApiClient _api;

  static const String _tokenKey = 'auth_token';
  static const String _userIdKey = 'user_id';

  AuthService(this._api);

  Stream<User?> get authStateChanges => _auth.authStateChanges();

  Future<String?> getToken() async {
    return await _storage.read(key: _tokenKey);
  }

  Future<String?> getUserId() async {
    return await _storage.read(key: _userIdKey);
  }

  Future<void> _persistUserData(User user) async {
    final token = await user.getIdToken();
    await _storage.write(key: _tokenKey, value: token);
    await _storage.write(key: _userIdKey, value: user.uid);
  }

  Future<AuthResult> signInWithGoogle() async {
    try {
      // Start the Google sign-in process
      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();
      if (googleUser == null) {
        return AuthResult(
          success: false,
          errorMessage: 'Google sign in was aborted',
        );
      }

      // Get authentication details from Google
      final GoogleSignInAuthentication googleAuth = await googleUser.authentication;
      final credential = GoogleAuthProvider.credential(
        accessToken: googleAuth.accessToken,
        idToken: googleAuth.idToken,
      );

      // Sign in to Firebase with Google credentials
      final userCredential = await _auth.signInWithCredential(credential);
      final user = userCredential.user;

      if (user == null) {
        return AuthResult(
          success: false,
          errorMessage: 'Failed to sign in with Google',
        );
      }

      // Register with backend
      final idToken = await user.getIdToken();
      if (idToken != null) {
        try {
          await _api.post('/auth/google', body: {'idToken': idToken});
          await _persistUserData(user);
        } catch (e) {
          // If backend registration fails, we still have a Firebase user
          // so we'll consider this a partial success
          return AuthResult(
            success: true,
            user: user,
            errorMessage: 'Authenticated with Google but failed to register with server: ${e.toString()}',
          );
        }
      }

      return AuthResult(success: true, user: user);
    } catch (e) {
      return AuthResult(
        success: false,
        errorMessage: 'Authentication error: ${e.toString()}',
      );
    }
  }

  Future<void> signOut() async {
    await _googleSignIn.signOut();
    await _auth.signOut();
    await _storage.delete(key: _tokenKey);
    await _storage.delete(key: _userIdKey);
  }
}