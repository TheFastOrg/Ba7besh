import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_phone_auth_handler/firebase_phone_auth_handler.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:lottie/lottie.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/presentation/screens/home_screen.dart';
import 'package:pinput/pinput.dart';

class OtpVerificationScreen extends ConsumerWidget {
  final String phoneNumber;

  const OtpVerificationScreen({
    Key? key,
    required this.phoneNumber,
  }) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return FirebasePhoneAuthHandler(
      phoneNumber: phoneNumber,
      signOutOnSuccessfulVerification: false,
      linkWithExistingUser: false,
      autoRetrievalTimeOutDuration: const Duration(seconds: 60),
      otpExpirationDuration: const Duration(seconds: 60),
      onCodeSent: () {
        // Code sent callback
      },
      onLoginSuccess: (userCredential, autoVerified) async {
        // Handle successful verification
        await ref.read(authProvider.notifier).completePhoneAuth(userCredential);

        if (context.mounted) {
          Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(builder: (_) => const HomeScreen()),
                (route) => false,
          );
        }
      },
      onLoginFailed: (authException, stackTrace) {
        // Handle failed verification
        ref.read(authProvider.notifier).setAuthError(
            authException.message ?? 'Verification failed');
      },
      onError: (error, stackTrace) {
        // Handle general errors
        ref.read(authProvider.notifier).setAuthError(error.toString());
      },
      builder: (context, controller) {
        return Scaffold(
          appBar: AppBar(
            title: const Text('Verify Phone'),
          ),
          body: controller.isSendingCode
              ? const Center(child: CircularProgressIndicator())
              : _buildVerificationUI(context, controller, ref),
        );
      },
    );
  }

  Widget _buildVerificationUI(
      BuildContext context,
      FirebasePhoneAuthController controller,
      WidgetRef ref,
      ) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Text(
              'Verification Code',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Text(
              'Enter the code sent to $phoneNumber',
              style: const TextStyle(fontSize: 14, color: Colors.grey),
            ),
            const SizedBox(height: 24),

            // PIN input
            Pinput(
              length: 6,
              defaultPinTheme: PinTheme(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey.shade300),
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              focusedPinTheme: PinTheme(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.blue),
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              onCompleted: (pin) {
                controller.verifyOtp(pin);
              },
              enabled: !controller.isSendingCode,
            ),

            const SizedBox(height: 24),

            // Verification status
            if (controller.isSendingCode)
              Center(
                child: Lottie.asset(
                  'assets/animations/loading.json',
                  width: 100,
                  height: 100,
                ),
              ),

            // Resend code option
            if (controller.codeSent) ...[
              const SizedBox(height: 24),
              if (controller.isOtpExpired)
                OutlinedButton(
                  onPressed: controller.isOtpExpired
                      ? () => controller.sendOTP()
                      : null,
                  child: const Text('Resend Code'),
                )
              else
                Text(
                  'Resend code in ${controller.otpExpirationTimeLeft.inSeconds}s',
                  style: const TextStyle(fontSize: 14),
                  textAlign: TextAlign.center,
                ),
            ],

            // Auto verification status
            if (controller.autoRetrievalTimeLeft != null) ...[
              const SizedBox(height: 16),
              Text(
                'Auto verifying in ${controller.autoRetrievalTimeLeft?.inSeconds}s',
                style: const TextStyle(fontSize: 14),
                textAlign: TextAlign.center,
              ),
            ],

            // Error message
            if (ref.watch(authProvider).errorMessage != null) ...[
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.red.shade200),
                ),
                child: Text(
                  ref.watch(authProvider).errorMessage!,
                  style: TextStyle(color: Colors.red.shade800),
                  textAlign: TextAlign.center,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}