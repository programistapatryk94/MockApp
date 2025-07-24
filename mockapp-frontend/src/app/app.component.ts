import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../helpers/auth.service';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { SubscriptionDialogComponent } from './subscription/subscription-dialog/subscription-dialog.component';
import { LanguageService } from '../helpers/language.service';
import { catchError, EMPTY } from 'rxjs';
import { SnackbarService } from './shared/snackbar/snackbar.service';
import { MatMenuModule } from '@angular/material/menu';
import { LanguageInfo } from '../helpers/auth.model';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    CommonModule,
    RouterLink,
    TranslateModule,
    MatMenuModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title: string = 'SimpliAPI';
  currentLanguage: LanguageInfo;

  constructor(
    private auth: AuthService,
    private translate: TranslateService,
    private dialog: MatDialog,
    public languageService: LanguageService,
    private snackbarService: SnackbarService
  ) {
    translate.addLangs(this.languageService.getAvailableLanguageNames());

    translate.setDefaultLang(this.languageService.getDefaultLanguageName());

    const appLang = this.languageService.getTranslateLang();
    translate.use(appLang);

    this.currentLanguage = this.languageService.currentLanguage;
  }

  manageSubscription() {
    this.dialog
      .open(SubscriptionDialogComponent)
      .afterClosed()
      .subscribe(() => {});
  }

  switchLanguage(lang: string) {
    this.languageService
      .changeLanguage(lang)
      .pipe(
        catchError((err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
          return EMPTY;
        })
      )
      .subscribe(() => {});
  }

  isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  logout() {
    this.auth.logout();
  }
}
