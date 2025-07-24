import { PlatformLocation, registerLocaleData } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AppConsts } from '../app/shared/AppConsts';
import { environment } from '../environments/environment';
import { AppSessionService } from '../app/shared/session/app-session.service';
import { LanguageService } from './language.service';
import localeEn from '@angular/common/locales/en';
import localePl from '@angular/common/locales/pl';

export function loadAngularLocale(locale: string) {
  switch (locale) {
    case 'pl':
      registerLocaleData(localePl);
      break;
    case 'en':
    default:
      registerLocaleData(localeEn);
      break;
  }
}

@Injectable({
  providedIn: 'root',
})
export class AppInitializer {
  constructor(
    private _injector: Injector,
    private _platformLocation: PlatformLocation,
    private _httpClient: HttpClient
  ) {}

  async init(): Promise<void> {
    app.ui.setBusy();
    try {
      AppConsts.appBaseHref = this.getBaseHref();
      const appBaseUrl = this.getDocumentOrigin() + AppConsts.appBaseHref;
      await this.getApplicationConfig(appBaseUrl);
      const appSessionService = this._injector.get(AppSessionService);
      const langService = this._injector.get(LanguageService);
      await appSessionService.init();
      langService.currentLocale =
        appSessionService.localization.currentCulture.name;
      langService.localization = appSessionService.localization;

      if (langService.shouldLoadLocale()) {
        const angularLocale = langService.getAngularLocale();

        loadAngularLocale(angularLocale);
      }
    } catch (err) {
      app.ui.clearBusy();
      throw err;
    }
    app.ui.clearBusy();
  }

  private getBaseHref(): string {
    const baseUrl = this._platformLocation.getBaseHrefFromDOM();
    if (baseUrl) {
      return baseUrl;
    }

    return '/';
  }

  private getDocumentOrigin(): string {
    if (typeof document === 'undefined') return '';

    if (!document.location.origin) {
      const port = document.location.port ? ':' + document.location.port : '';
      return (
        document.location.protocol + '//' + document.location.hostname + port
      );
    }

    return document.location.origin;
  }

  private async getApplicationConfig(appRootUrl: string): Promise<void> {
    const response = await firstValueFrom(
      this._httpClient.get<any>(
        `${appRootUrl}assets/${environment.appConfig}`,
        {
          headers: {},
        }
      )
    );

    AppConsts.appBaseUrl = response.appBaseUrl;
    AppConsts.remoteServiceBaseUrl = response.remoteServiceBaseUrl;
  }
}
