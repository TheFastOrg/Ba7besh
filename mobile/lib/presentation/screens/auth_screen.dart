import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/presentation/screens/home_screen.dart';
import 'package:mobile/presentation/screens/phone_auth_screen.dart';
import 'package:mobile/presentation/widgets/social_auth_button.dart';

class AuthScreen extends ConsumerWidget {
  const AuthScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);

    // Navigate to home screen when user is authenticated
    ref.listen(authProvider, (previous, next) {
      if (next.user != null && !next.isLoading) {
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (_) => const HomeScreen()),
        );
      }
    });

    return Scaffold(
      body: SafeArea(
        child: Stack(
          children: [
            Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // App logo or welcome animation
                  Lottie.asset(
                    'assets/animations/welcome.json',
                    height: 200,
                  ),
                  const SizedBox(height: 48),

                  // Title
                  Text(
                    'Welcome to Ba7besh',
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),

                  // Subtitle
                  Text(
                    'Discover and share your favorite local restaurants in Syria',
                    style: Theme.of(context).textTheme.bodyLarge,
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 48),

                  // Error message (if any)
                  if (authState.errorMessage != null) ...[
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.red.shade50,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.red.shade200),
                      ),
                      child: Text(
                        authState.errorMessage!,
                        style: TextStyle(color: Colors.red.shade800),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    const SizedBox(height: 24),
                  ],

                  // Phone Sign-in button
                  SocialAuthButton(
                    icon: Icons.phone,
                    label: 'Continue with Phone',
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(builder: (_) => const PhoneAuthScreen()),
                      );
                    },
                  ),

                  const SizedBox(height: 16),

                  // Google Sign-in button
                  SocialAuthButton(
                    icon: Icons.g_mobiledata,
                    label: 'Continue with Google',
                    onPressed: () => ref.read(authProvider.notifier).signInWithGoogle(),
                  ),

                  const SizedBox(height: 16),

                  // Additional option if needed
                  TextButton(
                    onPressed: () {
                      // Skip for now or continue as guest feature could be implemented here
                    },
                    child: const Text('Skip for now'),
                  ),
                ],
              ),
            ),

            // Loading overlay
            if (authState.isLoading)
              Container(
                color: Colors.black45,
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
      ),
    );
  }
}