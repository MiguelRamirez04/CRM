import { Injectable, signal, effect, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'crm-theme';

  darkMode = signal<boolean>(this.getInitialTheme());

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    effect(() => {
      const isDark = this.darkMode();
      this.applyTheme(isDark);
    });
  }

  toggle() {
    this.darkMode.update(v => !v);
    this.persist(this.darkMode());
  }

  setDarkMode(enabled: boolean) {
    this.darkMode.set(enabled);
    this.persist(enabled);
  }

  private getInitialTheme(): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;
    const stored = localStorage.getItem(this.storageKey);
    if (stored !== null) return stored === 'dark';
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  private persist(enabled: boolean) {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.setItem(this.storageKey, enabled ? 'dark' : 'light');
  }

  private applyTheme(isDark: boolean) {
    if (!isPlatformBrowser(this.platformId)) return;
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
  }
}
