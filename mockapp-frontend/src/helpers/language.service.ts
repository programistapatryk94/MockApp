import { Injectable } from '@angular/core';
import { LanguageInfo, LocalizationConfigurationDto } from './auth.model';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app/shared/app-config.service';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private _locale: string;
  private _localization!: LocalizationConfigurationDto;
  private _defaultLanguage!: LanguageInfo;
  private _currentLanguage!: LanguageInfo;

  get apiUrl(): string {
    return `${this.config.remoteApiUrl}/api/auth`;
  }

  constructor(private http: HttpClient, private config: AppConfigService) {
    this._locale = this.detectInitialLocale();
  }

  get currentLocale(): string {
    return this._locale;
  }

  get currentLanguage(): LanguageInfo {
    return this._currentLanguage;
  }

  set localization(loc: LocalizationConfigurationDto) {
    this._localization = loc;
    this._defaultLanguage = loc.languages.find((p) => p.isDefault)!;
    this._currentLanguage = loc.languages.find(p => p.name == this._locale)!;
  }

  get localization(): LocalizationConfigurationDto {
    return this._localization;
  }

  set currentLocale(locale: string) {
    this._locale = locale;
    localStorage.setItem('app.locale', locale); // lub cookie
  }

  changeLanguage(languageName: string): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/changeLanguage`, {
        languageName: languageName,
      })
      .pipe(
        tap(() => {
          this.currentLocale = languageName;
          window.location.reload();
        })
      );
  }

  private detectInitialLocale(): string {
    // kolejność: localStorage > cookie > przeglądarka
    return localStorage.getItem('app.locale') || navigator.language || 'en-US';
  }

  shouldLoadLocale(): boolean {
    return this._locale !== 'en-US';
  }

  getAngularLocale(): string {
    return this._locale;
  }

  getTranslateLang(): string {
    // Mapowanie dla ngx-translate
    return this.mapToShortLang(this._locale);
  }

  getDefaultLanguageName(): string {
    return this.mapToShortLang(this._defaultLanguage.name);
  }

  getAvailableLanguageNames(): string[] {
    return this.localization.languages.map((e) => this.mapToShortLang(e.name));
  }

  private mapToShortLang(locale: string): string {
    if (!locale) return 'en';
    const normalized = locale.split('-')[0].toLowerCase();

    if (normalized.startsWith('pl')) return 'pl';
    if (normalized.startsWith('en')) return 'en';
    // Dodaj inne mapowania w razie potrzeby

    return 'en'; // fallback
  }
}
