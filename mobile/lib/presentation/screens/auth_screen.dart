import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/presentation/screens/home_screen.dart';
import 'package:mobile/presentation/widgets/social_auth_button.dart';

class AuthScreen extends ConsumerWidget {
  const AuthScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              'Welcome to Ba7besh',
              style: Theme.of(context).textTheme.headlineMedium,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 40),
            SocialAuthButton(
              icon: Icons.flutter_dash,
              label: 'Continue with Google',
              onPressed: () async {
                // TODO: Implement Google auth
                Navigator.of(context).pushReplacement(
                  MaterialPageRoute(builder: (_) => const HomeScreen()),
                );
              },
            ),
            const SizedBox(height: 16),
            SocialAuthButton(
              icon: Icons.facebook,
              label: 'Continue with Facebook',
              onPressed: () async {
                // TODO: Implement Facebook auth
              },
            ),
            const SizedBox(height: 16),
            SocialAuthButton(
              icon: Icons.camera_alt,
              label: 'Continue with Instagram',
              onPressed: () async {
                // TODO: Implement Instagram auth
              },
            ),
          ],
        ),
      ),
    );
  }
}