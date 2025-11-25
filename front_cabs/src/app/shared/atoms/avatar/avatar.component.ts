import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../../core/services/secure-auth.service';

@Component({
    selector: 'app-ui-avatar',
    standalone: true,
    imports: [CommonModule],
    template: `<div class="w-full h-full aspect-square rounded-full flex items-center justify-center font-semibold uppercase bg-zinc-800 text-white">{{ initials }}</div>`,
})
export class UiAvatarComponent {
    @Input() user: User | null = null;
    get initials(): string {
        if (!this.user) return 'U';
        if (this.user.nombre && this.user.apellido) {
            return `${this.user.nombre.charAt(0)}${this.user.apellido.charAt(0)}`.toUpperCase();
    }
    if (this.user.nombreCompleto) {
        const parts = this.user.nombreCompleto.split(' ');
        if (parts.length >= 2) {
            return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
        }
        return parts[0].substring(0, 2).toUpperCase();
    }
    if (this.user.name) {
        const parts = this.user.name.split(' ');
        return parts.map(p => p.charAt(0)).join('').substring(0, 2).toUpperCase();
    }
    return 'U';
    }
}
