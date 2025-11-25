import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { SecureAuthService, User } from '../../../core/services/secure-auth.service';

@Component({
    selector: 'app-ui-avatar',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="w-full h-full aspect-square rounded-full flex items-center justify-center font-semibold bg-zinc-800 text-white">
        {{ initials }}
    </div>
    `,
})
export class UiAvatarComponent implements OnInit, OnDestroy {
    private auth = inject(SecureAuthService);
    private cdr = inject(ChangeDetectorRef);
    private sub?: Subscription;

    initials = 'U';

    ngOnInit(): void {
        // Inicial
        this.initials = this.computeInitials(this.auth.getCurrentUser());

        // Reactivo
        this.sub = this.auth.currentUser$.subscribe((user) => {
        this.initials = this.computeInitials(user);
        this.cdr.markForCheck(); // asegura render en OnPush
        });
    }

    ngOnDestroy(): void {
        this.sub?.unsubscribe();
    }

    private computeInitials(user: User | null): string {
        if (!user) return 'U';

        const safe = (s?: string) => (s?.[0] || '').toUpperCase();

        if (user.nombre && user.apellido) return (safe(user.nombre) + safe(user.apellido)) || 'U';

        if (user.nombreCompleto) {
        const parts = user.nombreCompleto.split(' ').filter(Boolean);
        if (parts.length >= 2) return (safe(parts[0]) + safe(parts[1])) || 'U';
        return parts[0].substring(0, 2).toUpperCase();
        }

        if (user.name) {
        const parts = user.name.split(' ').filter(Boolean);
        return parts.map(p => safe(p)).join('').substring(0, 2) || 'U';
        }

        return 'U';
    }
}
