import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/presentation/screens/home_screen.dart';
import 'package:mobile/presentation/widgets/social_auth_button.dart';

class AuthScreen extends ConsumerWidget {
  const AuthScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref.listen(authProvider, (previous, next) {
      next.whenOrNull(
        data: (user) {
          if (user != null) {
            Navigator.of(context).pushReplacement(
              MaterialPageRoute(builder: (_) => const HomeScreen()),
            );
          }
        },
        error: (error, _) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Authentication failed: $error')),
          );
        },
      );
    });

    final authState = ref.watch(authProvider);

    return Scaffold(
      body: Stack(
        children: [
          Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Lottie.asset(
                  'assets/animations/welcome.json',
                  height: 200,
                ),
                const SizedBox(height: 48),
                Text(
                  'Welcome to Ba7besh',
                  style: Theme.of(context).textTheme.headlineMedium,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                Text(
                  'Discover and share your favorite local restaurants',
                  style: Theme.of(context).textTheme.bodyLarge,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 48),
                SocialAuthButton(
                  icon: Icons.g_mobiledata,
                  label: 'Continue with Google',
                  onPressed: () => ref.read(authProvider.notifier).signInWithGoogle(),
                ),
              ],
            ),
          ),
          if (authState.isLoading)
            Container(
              color: Colors.black54,
              child: Center(
                child: Lottie.asset(
                  'assets/animations/loading.json',
                  width: 200,
                  height: 200,
                ),
              ),
            ),
        ],
      ),
    );
  }
}