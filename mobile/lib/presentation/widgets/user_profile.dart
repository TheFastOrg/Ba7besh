import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/auth/auth_provider.dart';
import 'package:mobile/presentation/screens/auth_screen.dart';

class UserProfile extends ConsumerWidget {
  const UserProfile({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);
    final user = authState.user;

    if (user == null) {
      return const SizedBox.shrink();
    }

    return Column(
      children: [
        ListTile(
          leading: _buildProfileAvatar(user),
          title: Text(user.displayName ?? 'User'),
          subtitle: Text(user.email ?? ''),
        ),
        const Divider(),
        ListTile(
          leading: const Icon(Icons.logout),
          title: const Text('Sign Out'),
          onTap: () async {
            await ref.read(authProvider.notifier).signOut();
            if (context.mounted) {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const AuthScreen()),
              );
            }
          },
        ),
      ],
    );
  }

  Widget _buildProfileAvatar(User user) {
    if (user.photoURL != null) {
      return CircleAvatar(
        backgroundImage: NetworkImage(user.photoURL!),
      );
    } else {
      return CircleAvatar(
        backgroundColor: Colors.deepPurple.shade100,
        child: Text(
          _getInitials(user.displayName),
          style: TextStyle(color: Colors.deepPurple.shade900),
        ),
      );
    }
  }

  String _getInitials(String? displayName) {
    if (displayName == null || displayName.isEmpty) {
      return 'U';
    }
    final names = displayName.split(' ');
    if (names.length == 1) {
      return names[0][0].toUpperCase();
    }
    return '${names[0][0]}${names[1][0]}'.toUpperCase();
  }
}