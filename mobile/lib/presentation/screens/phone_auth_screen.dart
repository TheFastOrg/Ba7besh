import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/presentation/screens/otp_verification_screen.dart';

class PhoneAuthScreen extends ConsumerStatefulWidget {
  const PhoneAuthScreen({super.key});

  @override
  ConsumerState<PhoneAuthScreen> createState() => _PhoneAuthScreenState();
}

class _PhoneAuthScreenState extends ConsumerState<PhoneAuthScreen> {
  final TextEditingController _phoneController = TextEditingController();
  bool _isValid = false;
  String _errorText = '';

  @override
  void dispose() {
    _phoneController.dispose();
    super.dispose();
  }

  void _validatePhoneNumber(String value) {
    // Syrian mobile number validation - should start with 9 and have 10 digits total
    final syrianPhoneRegex = RegExp(r'^9\d{8}$');
    setState(() {
      if (value.isEmpty) {
        _errorText = 'Phone number is required';
        _isValid = false;
      } else if (!syrianPhoneRegex.hasMatch(value)) {
        _errorText = 'Enter a valid Syrian phone number (9XXXXXXXX)';
        _isValid = false;
      } else {
        _errorText = '';
        _isValid = true;
      }
      _errorText = '';
      _isValid = true;
    });
  }

  void _continueWithPhone() {
    if (!_isValid) return;

    // Format the phone number with Syria's country code
    final phoneNumber = '${_phoneController.text}';

    ref.read(authProvider.notifier).startPhoneAuth(phoneNumber);
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => OtpVerificationScreen(phoneNumber: phoneNumber),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Phone Login'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'Enter your phone number',
                style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              const Text(
                'We\'ll send a verification code to your phone',
                style: TextStyle(fontSize: 14, color: Colors.grey),
              ),
              const SizedBox(height: 24),
              TextField(
                controller: _phoneController,
                keyboardType: TextInputType.phone,
                onChanged: _validatePhoneNumber,
                decoration: InputDecoration(
                  labelText: 'Phone Number',
                  hintText: '9XXXXXXXX',
                  prefixText: '+963 ',
                  errorText: _errorText.isNotEmpty ? _errorText : null,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: _isValid ? _continueWithPhone : null,
                child: const Text('Continue'),
              ),

              if (authState.errorMessage != null) ...[
                const SizedBox(height: 16),
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
              ],
            ],
          ),
        ),
      ),
    );
  }
}