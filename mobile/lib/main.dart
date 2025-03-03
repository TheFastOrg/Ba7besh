import 'package:firebase_phone_auth_handler/firebase_phone_auth_handler.dart';
import 'package:flutter/material.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'device/device_provider.dart';
import 'firebase_options.dart';
import 'presentation/screens/loading_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize Firebase
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );

  // Create provider container for state management
  final container = ProviderContainer();

  // Ensure device is registered with the backend
  await container.read(deviceProvider.notifier).ensureRegistered();

  runApp(
    UncontrolledProviderScope(
      container: container,
      child: const Ba7beshApp(),
    ),
  );
}

class Ba7beshApp extends StatelessWidget {
  const Ba7beshApp({super.key});

  @override
  Widget build(BuildContext context) {
    return FirebasePhoneAuthProvider(child: MaterialApp(
      title: 'Ba7besh',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.deepPurple,
          brightness: Brightness.light,
        ),
        useMaterial3: true,
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            padding: const EdgeInsets.symmetric(
              horizontal: 24,
              vertical: 12,
            ),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        ),
      ),
      home: const LoadingScreen(),
    ),);
  }
}