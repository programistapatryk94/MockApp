import {
  ApplicationConfig,
  importProvidersFrom,
  inject,
  LOCALE_ID,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { TokenInterceptor } from '../helpers/token.interceptor';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { AppInitializer } from '../helpers/app.initializer';
import { AppSessionService } from './shared/session/app-session.service';
import { ServiceApiModule } from '../services/apis/service-api.module';
import { LanguageService } from '../helpers/language.service';
import { LanguageTokenInterceptor } from '../helpers/language-token.interceptor';
import { ErrorInterceptor } from '../helpers/error.interceptor';

export function createTranslateLoader(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export function appInitializerFactory(
  appInit: AppInitializer
): () => Promise<void> {
  return () => appInit.init();
}

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(
      ServiceApiModule,
      TranslateModule.forRoot({
        defaultLanguage: 'pl',
        loader: {
          provide: TranslateLoader,
          useFactory: createTranslateLoader,
          deps: [HttpClient],
        },
      })
    ),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: LanguageTokenInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true,
    },
    AppSessionService,
    provideAppInitializer(
      () => inject(AppInitializer).init() // <-- to działa jak `useFactory`
    ),
    {
      provide: MAT_DIALOG_DEFAULT_OPTIONS,
      useValue: {
        hasBackdrop: true,
        disableClose: true,
      },
    },
    {
      provide: LOCALE_ID,
      useFactory: (languageService: LanguageService) =>
        languageService.currentLocale,
      deps: [LanguageService],
    },
    provideHttpClient(withInterceptorsFromDi()),
  ],
};
